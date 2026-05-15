using WebApi.Data;
using WebApi.GraphQL.Inputs;
using WebApi.GraphQL.Payloads;
using WebApi.Models;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using HotChocolate.Types;

namespace WebApi.GraphQL.Mutations
{
    [ExtendObjectType(typeof(Mutation))]
    public class SaleMutations
    {
        public async Task<SalePayload> CreateSaleAsync(
            CreateSaleInput input,
            [Service] AppDbContext context)
        {
            if (input.Details == null || !input.Details.Any())
                return new SalePayload(null, null, false, "La venta debe tener al menos un detalle.");

            if (input.Payments == null || !input.Payments.Any())
                return new SalePayload(null, null, false, "La venta debe tener al menos un pago.");

            // Iniciar Transacción ACID
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                decimal totalGrossSubtotal = 0;
                decimal totalDiscount = 0;
                decimal totalTax = 0;
                decimal netTotal = 0;

                var saleDetails = new List<SaleDetail>();
                var inventoryUpdates = new List<(Batch batch, Product product, int quantity)>();

                // PASO A y B: Validación de Inventario y Cálculos
                foreach (var detailInput in input.Details)
                {
                    // Obtener Producto y Lote
                    var product = await context.Products.FindAsync(detailInput.ProductId);
                    var batch = await context.Batches.FindAsync(detailInput.BatchId);

                    if (product == null)
                        throw new Exception($"Producto con ID {detailInput.ProductId} no encontrado.");
                    
                    if (batch == null || batch.product_id != detailInput.ProductId)
                        throw new Exception($"Lote con ID {detailInput.BatchId} no válido para el producto.");

                    // Validación de Inventario
                    if (batch.current_quantity_units < detailInput.Quantity)
                        throw new Exception($"Inventario insuficiente para el producto {detailInput.ProductId} en el lote {batch.batch_code}. Disponible: {batch.current_quantity_units}");

                    // Validación de Expiración
                    if (batch.expiration_date <= DateTime.Now)
                        throw new Exception($"El lote {batch.batch_code} está vencido (Expiró: {batch.expiration_date:dd/MM/yyyy}).");

                    //// Precios
                    //decimal unitPrice = detailInput.IsFullPresentation 
                    //    ? (product.price_full_presentation / (product.units_per_presentation > 0 ? product.units_per_presentation : 1)) ?? 0
                    //    : product.price_per_unit ?? 0;

                    decimal unitPrice = detailInput.IsFullPresentation
                        ? product.price_full_presentation ?? 0
                        : product.price_per_unit ?? 0;

                    int unitsToDeduct = detailInput.IsFullPresentation
                        ? detailInput.Quantity * (product.units_per_presentation > 0 ? product.units_per_presentation : 1)
                        : detailInput.Quantity;

                    // CORRECCIÓN: Validación de Inventario usando unitsToDeduct en lugar de detailInput.Quantity
                    if (batch.current_quantity_units < unitsToDeduct)
                        throw new Exception($"Inventario insuficiente para el producto {detailInput.ProductId} en el lote {batch.batch_code}. Disponible: {batch.current_quantity_units}, Requerido: {unitsToDeduct}");

                    // Validación de Expiración
                    if (batch.expiration_date <= DateTime.Now)
                        throw new Exception($"El lote {batch.batch_code} está vencido (Expiró: {batch.expiration_date:dd/MM/yyyy}).");

                    //if (unitPrice <= 0)
                    //    throw new Exception($"El producto {detailInput.ProductId} no tiene un precio configurado.");

                    // Cálculos de línea
                    decimal lineSubtotal = unitPrice * detailInput.Quantity;
                    decimal lineDiscount = 0; // Aquí se aplicaría lógica de Promotion si se requiere
                    
                    // Aplicar promoción si existe
                    if (detailInput.PromotionId.HasValue)
                    {
                        var promo = await context.Promotions.FindAsync(detailInput.PromotionId.Value);
                        if (promo != null && promo.IsActive)
                        {
                            if (promo.CalculationType == "PERCENTAGE")
                                lineDiscount = lineSubtotal * promo.DiscountValue;
                            else
                                lineDiscount = promo.DiscountValue;
                        }
                    }

                    decimal taxRate = 0.15m; // IVA 15% por defecto
                    decimal lineTax = (lineSubtotal - lineDiscount) * taxRate;
                    decimal lineTotal = (lineSubtotal - lineDiscount) + lineTax;

                    // Acumular totales de cabecera
                    totalGrossSubtotal += lineSubtotal;
                    totalDiscount += lineDiscount;
                    totalTax += lineTax;
                    netTotal += lineTotal;

                    // Preparar detalle
                    saleDetails.Add(new SaleDetail
                    {
                        ProductId = detailInput.ProductId,
                        BatchId = detailInput.BatchId,
                        PromotionId = detailInput.PromotionId,
                        Quantity = detailInput.Quantity,
                        IsFullPresentation = detailInput.IsFullPresentation,
                        AppliedCostPrice = product.cost_price,
                        AppliedUnitPrice = unitPrice,
                        AppliedTaxRate = taxRate,
                        CalculatedDiscount = lineDiscount,
                        CalculatedTax = lineTax,
                        LineTotal = lineTotal
                    });

                    inventoryUpdates.Add((batch, product, unitsToDeduct));
                }

                // PASO G: Validación de Pagos (Antes de insertar para fallar rápido)
                decimal totalPayments = input.Payments.Sum(p => p.Amount);
                if (Math.Abs(totalPayments - netTotal) > 0.01m)
                    throw new Exception($"El total de los pagos (C$ {totalPayments}) no coincide con el total de la venta (C$ {netTotal}).");

                // PASO C: Inserción Cabecera
                var sale = new Sale
                {
                    EmployeeId = input.EmployeeId,
                    CustomerId = input.CustomerId,
                    ReceiptType = input.ReceiptType,
                    ReceiptNumber = input.ReceiptNumber,
                    PrescriptionNumber = input.PrescriptionNumber,
                    DoctorName = input.DoctorName,
                    Currency = input.Currency ?? "NIO",
                    SaleDate = DateTime.Now,
                    GrossSubtotal = totalGrossSubtotal,
                    TotalDiscount = totalDiscount,
                    TotalTax = totalTax,
                    NetTotal = netTotal,
                    Status = "COMPLETED"
                };

                context.Sales.Add(sale);
                await context.SaveChangesAsync(); // Obtenemos SaleId

                // PASO D, E y F: Detalles, Inventario y Kardex
                for (int i = 0; i < saleDetails.Count; i++)
                {
                    var detail = saleDetails[i];
                    var (batchToUpdate, productToUpdate, unitsToDeduct) = inventoryUpdates[i];

                    detail.SaleId = sale.SaleId;
                    context.SaleDetails.Add(detail);

                    // Descuento de Inventario
                    batchToUpdate.current_quantity_units -= unitsToDeduct;
                    
                    // Sincronizar stock total en Product
                    productToUpdate.stock_units -= unitsToDeduct;
                    context.Products.Update(productToUpdate);

                    // PASO F: Kardex
                    context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = detail.ProductId,
                        BatchId = detail.BatchId,
                        EmployeeId = input.EmployeeId,
                        TransactionType = "SALE",
                        QuantityMoved = -unitsToDeduct,
                        StockAfterTransaction = batchToUpdate.current_quantity_units,
                        UnitCost = productToUpdate.cost_price,
                        ReferenceDocumentType = "SALE_INVOICE",
                        ReferenceDocumentId = sale.SaleId,
                        CreatedAt = DateTime.Now
                    });
                }

                // PASO G: Inserción de Pagos
                foreach (var payInput in input.Payments)
                {
                    context.SalePayments.Add(new SalePayment
                    {
                        SaleId = sale.SaleId,
                        PaymentMethodId = payInput.PaymentMethodId,
                        Amount = payInput.Amount,
                        TransactionReference = payInput.TransactionReference
                    });
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SalePayload(sale.SaleId, sale.ReceiptNumber, true, "Venta registrada exitosamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new SalePayload(null, null, false, $"Error en la venta: {ex.Message}");
            }
        }
    }
}
