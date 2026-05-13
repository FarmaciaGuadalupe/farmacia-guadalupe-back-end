using Microsoft.EntityFrameworkCore;

namespace WebApi.GraphQL
{
    // DbContext
    using WebApi.Data; 
    
    // Modelos
    using WebApi.Models.Empleados;
    using WebApi.Models;
    using WebApi.GraphQL.Payloads;
    
    using HotChocolate.Types;
    using HotChocolate.Data;
    using HotChocolate;

    // Define QUÉ se puede pedir.
    public class Query
    {
        // Este método expone tu tabla de Empleados.
        // Usamos IQueryable para máxima eficiencia.
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)
        public IQueryable<Employee> GetEmployees(
            [Service] AppDbContext context) // Inyecta tu DbContext
        {
            return context.Employees;
        }

        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<EmployeeRoles> GetEmployeeRoles(
        [Service] AppDbContext context)
        {
            return context.EmployeeRoles;
        }

        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<EmployeeStatuses> GetEmployeeStatus(
            [Service] AppDbContext context)
        {
            return context.EmployeeStatuses;
        }

        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<Brands> GetBrands([Service] AppDbContext context)
        {
            return context.Brands;
        }
        
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<Category> GetCategories([Service] AppDbContext context)
        {
            return context.Categories;
        }
        
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<ActiveIngredient> GetActiveIngredients([Service] AppDbContext context)
        {
            return context.ActiveIngredients;
        }
        
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<AdministrationRoute> GetAdministrationRoutes([Service] AppDbContext context)
        {
            return context.AdministrationRoutes;
        }
        
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<UnitOfMeasure> GetUnitOfMeasures([Service] AppDbContext context)
        {
            return context.UnitOfMeasures;
        }
        
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<DoseUnit> GetDoseUnits([Service] AppDbContext context)
        {
            return context.DoseUnits;
        }
        
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<SupplierType> GetSupplierTypes([Service] AppDbContext context)
        {
            return context.SupplierTypes;
        }
        
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<Supplier> GetSuppliers([Service] AppDbContext context)
        {
            return context.Suppliers;
        }

        
        // -------------------------------------------------------------------
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<Product> GetProducts([Service] AppDbContext context)
        {
            return context.Products;
        }
        
        [UsePaging]
        [UseProjection] 
        [UseFiltering]
        [UseSorting]       
        public IQueryable<Medicine> GetMedicines([Service] AppDbContext context)
        {
            return context.Medicines;
        }
        
        [UsePaging]
        [UseProjection] // Lee las relaciones.
        [UseFiltering]  // (Opcional) Permite filtrar (ej: where name = "...")
        [UseSorting]    // (Opcional) Permite ordenar (ej: order by names)    
        public IQueryable<MedicineActiveIngredient> GetMedicineActiveIngredients([Service] AppDbContext context)
        {
            return context.MedicineActiveIngredients;
        }

        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Batch> GetBatches([Service] AppDbContext context)
        {
            return context.Batches; 
        }

        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Presentation> GetPresentations([Service] AppDbContext context)
        {
            return context.Presentations;
        }


        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Manufacturer> GetManufacturers([Service] AppDbContext context)
        {
            return context.Manufacturers;
        }

        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<PaymentMethod> GetActivePaymentMethods([Service] AppDbContext context)
        {
            return context.PaymentMethods.Where(p => p.IsActive);
        }

        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Promotion> GetActivePromotions([Service] AppDbContext context)
        {
            return context.Promotions.Where(p => p.IsActive);
        }


        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Customer> GetCustomers([Service] AppDbContext context)
        {
            return context.Customers;
        }

        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<PaymentMethod> GetPaymentMethods([Service] AppDbContext context)
        {
            return context.PaymentMethods;
        }

        [UsePaging(IncludeTotalCount = true)] // Opcional: IncludeTotalCount te permite saber el total de páginas
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Sale> GetSales([Service] AppDbContext context)
        {
            // Solo devuelves el IQueryable. 
            // HotChocolate se encarga de añadir los Includes, el WHERE, el ORDER BY y el LIMIT (Paginación).
            return context.Sales;
        }

        [UseProjection]
        public async Task<Sale?> GetSaleById(int id, [Service] AppDbContext context)
        {
            return await context.Sales
                .Include(s => s.Employee) 
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                        .ThenInclude(p => p.medicine)
                .Include(s => s.SalePayments)
                    .ThenInclude(ss => ss.PaymentMethod)
                .FirstOrDefaultAsync(s => s.SaleId == id);  
        }

        public SalesSummary GetTotalSalesByMonth(
            int year, 
            int month, 
            [Service] AppDbContext context)
        {
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var startOfPrevMonth = startOfMonth.AddMonths(-1);

            var total = context.Sales
                .Where(s => s.SaleDate >= startOfMonth && s.SaleDate < endOfMonth && s.Status == "COMPLETED")
                .Sum(s => (decimal?)s.NetTotal) ?? 0m;

            var prevTotal = context.Sales
                .Where(s => s.SaleDate >= startOfPrevMonth && s.SaleDate < startOfMonth && s.Status == "COMPLETED")
                .Sum(s => (decimal?)s.NetTotal) ?? 0m;

            decimal percentage = 0;
            if (prevTotal > 0)
                percentage = ((total - prevTotal) / prevTotal) * 100;
            else if (total > 0)
                percentage = 100;

            return new SalesSummary(total, Math.Round(percentage, 2));
        }

        public SalesSummary GetNumberOfSalesByMonth(
            int year, 
            int month, 
            [Service] AppDbContext context)
        {
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var startOfPrevMonth = startOfMonth.AddMonths(-1);

            var count = context.Sales
                .Count(s => s.SaleDate >= startOfMonth && s.SaleDate < endOfMonth && s.Status == "COMPLETED");

            var prevCount = context.Sales
                .Count(s => s.SaleDate >= startOfPrevMonth && s.SaleDate < startOfMonth && s.Status == "COMPLETED");

            decimal percentage = 0;
            if (prevCount > 0)
                percentage = ((decimal)(count - prevCount) / prevCount) * 100;
            else if (count > 0)
                percentage = 100;

            return new SalesSummary(count, Math.Round(percentage, 2));
        }

        public SalesSummary GetTotalSalesByDay(
            DateTime date, 
            [Service] AppDbContext context)
        {
            var day = date.Date;
            var nextDay = day.AddDays(1);
            var prevDay = day.AddDays(-1);

            var total = context.Sales
                .Where(s => s.SaleDate >= day && s.SaleDate < nextDay && s.Status == "COMPLETED")
                .Sum(s => (decimal?)s.NetTotal) ?? 0m;

            var prevTotal = context.Sales
                .Where(s => s.SaleDate >= prevDay && s.SaleDate < day && s.Status == "COMPLETED")
                .Sum(s => (decimal?)s.NetTotal) ?? 0m;

            decimal percentage = 0;
            if (prevTotal > 0)
                percentage = ((total - prevTotal) / prevTotal) * 100;
            else if (total > 0)
                percentage = 100;

            return new SalesSummary(total, Math.Round(percentage, 2));
        }

        public SalesSummary GetNumberOfSalesByDay(
            DateTime date, 
            [Service] AppDbContext context)
        {
            var day = date.Date;
            var nextDay = day.AddDays(1);
            var prevDay = day.AddDays(-1);

            var count = context.Sales
                .Count(s => s.SaleDate >= day && s.SaleDate < nextDay && s.Status == "COMPLETED");

            var prevCount = context.Sales
                .Count(s => s.SaleDate >= prevDay && s.SaleDate < day && s.Status == "COMPLETED");

            decimal percentage = 0;
            if (prevCount > 0)
                percentage = ((decimal)(count - prevCount) / prevCount) * 100;
            else if (count > 0)
                percentage = 100;

            return new SalesSummary(count, Math.Round(percentage, 2));
        }

        public List<ChartData> GetSalesStats(
            DateTime startDate,
            DateTime endDate,
            FrequencyType type,
            [Service] AppDbContext context)
        {
            var query = context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate && s.Status == "COMPLETED");

            switch (type)
            {
                case FrequencyType.Daily:
                    return query
                        .GroupBy(s => s.SaleDate.Date)
                        // 1. Database execution (SQL)
                        .Select(g => new { 
                            Date = g.Key, 
                            Count = g.Count() 
                        }) 
                        .ToList() // Executes the SQL query here
                        // 2. In-memory execution (C#)
                        .Select(x => new ChartData(
                            x.Date.ToString("yyyy-MM-dd"),
                            (decimal)x.Count
                        ))
                        .OrderBy(x => x.Label)
                        .ToList();

                case FrequencyType.Monthly:
                    return query
                        .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                        // 1. Database execution (SQL)
                        .Select(g => new { 
                            g.Key.Year, 
                            g.Key.Month, 
                            Count = g.Count() 
                        })
                        .ToList() // Executes the SQL query here
                        // 2. In-memory execution (C#)
                        .Select(x => new ChartData(
                            $"{x.Year}-{x.Month:D2}",
                            (decimal)x.Count
                        ))
                        .OrderBy(x => x.Label)
                        .ToList();

                case FrequencyType.Yearly:
                    return query
                        .GroupBy(s => s.SaleDate.Year)
                        // 1. Database execution (SQL)
                        .Select(g => new { 
                            Year = g.Key, 
                            Count = g.Count() 
                        })
                        .ToList() // Executes the SQL query here
                        // 2. In-memory execution (C#)
                        .Select(x => new ChartData(
                            x.Year.ToString(),
                            (decimal)x.Count
                        ))
                        .OrderBy(x => x.Label)
                        .ToList();

                default:
                    return new List<ChartData>();
            }
        }

        public List<MedicineStock> GetLowStockMedicines([Service] AppDbContext context)
        {
            var products = context.Products
                .Include(p => p.medicine)
                .Where(p => p.medicine != null)
                .ToList();

            var result = products
                // .Where(p => p.stock_units <= (p.min_stock_units * 1.25m)) // Cerca (hasta 25% por encima del mínimo) o por debajo
                .OrderBy(p => p.stock_units - p.min_stock_units) // Los más críticos primero
                .Take(10)
                .Select(p => {
                    var percentage = p.min_stock_units > 0 ? ((decimal)p.stock_units / p.min_stock_units) * 100 : 0;
                    return new MedicineStock(
                        p.medicine.name,
                        p.stock_units,
                        p.min_stock_units,
                        Math.Round(percentage, 2)
                    );
                })
                .ToList();

            return result;
        }
        
    }
}
