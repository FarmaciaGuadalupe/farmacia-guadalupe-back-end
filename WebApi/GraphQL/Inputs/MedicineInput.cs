using System;
using System.Collections.Generic;

namespace WebApi.GraphQL.Inputs
{
    public record MedicineActiveIngredientInput(
        int ActiveIngredientId,
        decimal DoseValue,
        int DoseUnitId
    );

    public record AddMedicineInput(
        string Name,
        string Barcode,
        int IdBrand,
        int ManufacturerId,
        int CategoryId,
        int AdministrationRouteId,
        bool RequiresPrescription,
        int SupplierId,
        int PresentationId,
        int UnitOfMeasureId,
        int UnitsPerPresentation,
        string Currency,
        decimal CostPrice,
        decimal PricePerUnit,
        decimal PriceFullPresentation,
        bool IsFractionable,
        string Description,
        string BatchCode,
        DateTime ExpirationDate,
        int Units,
        int StockUnits,
        int MinStockUnits,
        List<MedicineActiveIngredientInput> Ingredients
    );

    public record UpdateMedicineInput(
        int MedicineId,
        string Name,
        int IdBrand,
        int CategoryId,
        int AdministrationRouteId,
        string Description,
        List<MedicineActiveIngredientInput> Ingredients
    );
}
