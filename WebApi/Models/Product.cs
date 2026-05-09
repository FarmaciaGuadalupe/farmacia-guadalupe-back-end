namespace WebApi.Models;

public class Product
{

    public int product_id { get; set; }

    public string barcode { get; set; }

    public int supplier_id { get; set; }

    public int presentation_id { get; set; }

    public int unit_of_measure_id { get; set; }

    // Lógica de Inventario
    public int units_per_presentation { get; set; } = 1;

    public int stock_units { get; set; } = 0;

    public int min_stock_units { get; set; } = 10;

    // Lógica de Precios
    public decimal cost_price { get; set; } = 0m;

    public decimal? price_full_presentation { get; set; }

    public decimal? price_per_unit { get; set; }

    public bool is_fractionable { get; set; } = false;

    // Metadatos
    public int product_status_id { get; set; } = 1;

    public string currency { get; set; } = "NIO";

    public DateTime created_at { get; set; } = DateTime.Now;

    // --- Propiedades de Navegación (Virtuales para EF Core) ---
    // Estas son las que HotChocolate usará para navegar por el grafo
        
    public Supplier supplier { get; set; }
    public Presentation presentation { get; set; }
    public UnitOfMeasure unit_of_measure { get; set; }
    
    public Medicine medicine { get; set; }
    
    public virtual ICollection<Batch> batches { get; set; } = new List<Batch>();
}
