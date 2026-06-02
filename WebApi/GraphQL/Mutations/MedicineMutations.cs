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
    public class MedicineMutations
    {
        public async Task<MutationResult> AddMedicineAsync(
            AddMedicineInput input,
            [Service] AppDbContext context)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. Crear el Producto
                var product = new Product
                {
                    barcode = input.Barcode,
                    supplier_id = input.SupplierId,
                    presentation_id = input.PresentationId,
                    unit_of_measure_id = input.UnitOfMeasureId,
                    units_per_presentation = input.UnitsPerPresentation,
                    stock_units = input.StockUnits,
                    min_stock_units = input.MinStockUnits,
                    cost_price = input.CostPrice,
                    price_full_presentation = input.PriceFullPresentation,
                    price_per_unit = input.PricePerUnit,
                    is_fractionable = input.IsFractionable,
                    currency = input.Currency,
                    created_at = DateTime.Now,
                    product_status_id = 1 // Activo por defecto
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

                // 2. Crear la Medicina
                var medicine = new Medicine
                {
                    product_id = product.product_id,
                    name = input.Name,
                    id_brand = input.IdBrand,
                    manufacturer_id = input.ManufacturerId,
                    category_id = input.CategoryId,
                    administration_route_id = input.AdministrationRouteId,
                    requires_prescription = input.RequiresPrescription,
                    description = input.Description // Mapeamos dosage a description
                };

                context.Medicines.Add(medicine);
                await context.SaveChangesAsync();

                // 3. Agregar Ingredientes Activos
                if (input.Ingredients != null && input.Ingredients.Count > 0)
                {
                    foreach (var ing in input.Ingredients)
                    {
                        var medicineActiveIngredient = new MedicineActiveIngredient
                        {
                            medicine_id = medicine.medicine_id,
                            active_ingredient_id = ing.ActiveIngredientId,
                            dose_value = ing.DoseValue,
                            dose_unit_id = ing.DoseUnitId
                        };
                        context.MedicineActiveIngredients.Add(medicineActiveIngredient);
                    }
                    await context.SaveChangesAsync();
                }

                // 4. Crear el Lote Inicial (Batch)
                var batch = new Batch
                {
                    product_id = product.product_id,
                    batch_code = input.BatchCode,
                    expiration_date = input.ExpirationDate,
                    initial_quantity_units = input.Units,
                    current_quantity_units = input.Units,
                    is_active = true,
                    created_at = DateTime.Now
                };

                context.Batches.Add(batch);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new MutationResult(true, "Medicina, producto y lote creados exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new MutationResult(false, $"Error al crear la medicina: {ex.Message}");
            }
        }

        public async Task<MutationResult> UpdateMedicineAsync(
            UpdateMedicineInput input,
            [Service] AppDbContext context)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var medicine = await context.Medicines
                    .Include(m => m.medicine_active_ingredients)
                    .FirstOrDefaultAsync(m => m.medicine_id == input.MedicineId);

                if (medicine == null)
                {
                    return new MutationResult(false, "Medicina no encontrada");
                }

                // 1. Actualizar campos básicos
                medicine.name = input.Name;
                medicine.id_brand = input.IdBrand;
                medicine.category_id = input.CategoryId;
                medicine.administration_route_id = input.AdministrationRouteId;
                medicine.description = input.Description;

                // 2. Actualizar Ingredientes Activos
                // Eliminamos los actuales
                context.MedicineActiveIngredients.RemoveRange(medicine.medicine_active_ingredients);

                // Agregamos los nuevos
                if (input.Ingredients != null && input.Ingredients.Count > 0)
                {
                    foreach (var ing in input.Ingredients)
                    {
                        var medicineActiveIngredient = new MedicineActiveIngredient
                        {
                            medicine_id = medicine.medicine_id,
                            active_ingredient_id = ing.ActiveIngredientId,
                            dose_value = ing.DoseValue,
                            dose_unit_id = ing.DoseUnitId
                        };
                        context.MedicineActiveIngredients.Add(medicineActiveIngredient);
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new MutationResult(true, "Medicina actualizada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new MutationResult(false, $"Error al actualizar la medicina: {ex.Message}");
            }
        }
    }
}
