namespace WebApi.GraphQL.Payloads;

public record MedicineStock(string MedicineName, int StockUnits, int MinStockUnits, decimal StockPercentage);
