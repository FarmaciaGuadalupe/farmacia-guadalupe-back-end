using WebApi.Data;
using WebApi.GraphQL.Inputs;
using WebApi.Models;
using WebApi.GraphQL.Payloads;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using HotChocolate.Types;

namespace WebApi.GraphQL.Mutations
{
    [ExtendObjectType(typeof(Mutation))]
    public class BatchMutations
    {
        public async Task<MutationResult> AddBatchAsync(
            AddBatchInput input,
            [Service] AppDbContext context)
        {
            var product = await context.Products.FindAsync(input.ProductId);
            if (product == null)
            {
                return new MutationResult(false, "El producto especificado no existe.");
            }

            // Verificar si el código de lote ya existe para este producto
            var batchExists = await context.Batches.AnyAsync(b => b.product_id == input.ProductId && b.batch_code == input.BatchCode);
            if (batchExists)
            {
                return new MutationResult(false, $"El código de lote '{input.BatchCode}' ya existe para este producto.");
            }

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var batch = new Batch
                {
                    product_id = input.ProductId,
                    batch_code = input.BatchCode,
                    expiration_date = input.ExpirationDate,
                    initial_quantity_units = input.QuantityUnits,
                    current_quantity_units = input.QuantityUnits,
                    is_active = true,
                    created_at = DateTime.Now
                };

                context.Batches.Add(batch);
                
                // Actualizar el stock total del producto
                product.stock_units += input.QuantityUnits;
                context.Products.Update(product);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new MutationResult(true, "Lote agregado exitosamente y stock del producto actualizado.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new MutationResult(false, $"Error al agregar el lote: {ex.Message}");
            }
        }

        public async Task<MutationResult> UpdateBatchAsync(
            UpdateBatchInput input,
            [Service] AppDbContext context)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var batch = await context.Batches
                    .Include(b => b.product)
                    .FirstOrDefaultAsync(b => b.batch_id == input.BatchId && b.product_id == input.ProductId);

                if (batch == null)
                {
                    return new MutationResult(false, "Lote no encontrado para el producto especificado.");
                }

                // Si se está cambiando el código del lote, verificar que no exista ya para ese producto
                if (batch.batch_code != input.BatchCode)
                {
                    var exists = await context.Batches.AnyAsync(b => 
                        b.product_id == input.ProductId && 
                        b.batch_code == input.BatchCode && 
                        b.batch_id != input.BatchId);
                    
                    if (exists)
                    {
                        return new MutationResult(false, $"El código de lote '{input.BatchCode}' ya está siendo usado por otro lote de este producto.");
                    }
                }

                // Calcular la diferencia de stock considerando el estado de activación
                int oldContribution = batch.is_active ? batch.current_quantity_units : 0;
                int newContribution = input.IsActive ? input.CurrentQuantityUnits : 0;
                int stockAdjustment = newContribution - oldContribution;

                // 1. Actualizar campos del lote
                batch.batch_code = input.BatchCode;
                batch.expiration_date = input.ExpirationDate;
                batch.current_quantity_units = input.CurrentQuantityUnits;
                batch.is_active = input.IsActive;

                // 2. Actualizar el stock total del producto basado en el cambio de contribución
                if (stockAdjustment != 0 && batch.product != null)
                {
                    batch.product.stock_units += stockAdjustment;
                    
                    // Asegurar que el stock no sea negativo (por si acaso)
                    if (batch.product.stock_units < 0) batch.product.stock_units = 0;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new MutationResult(true, "Lote actualizado exitosamente y stock total sincronizado.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new MutationResult(false, $"Error al actualizar el lote: {ex.Message}");
            }
        }
    }
}
