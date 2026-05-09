#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApi.Models.Empleados;

namespace WebApi.Models;

public class Medicine
{
    [Key]
    public int medicine_id { get; set; }

    public int product_id { get; set; }

    public int id_brand { get; set; }

    public int manufacturer_id { get; set; }

    public int category_id { get; set; }

    public int administration_route_id { get; set; }

    [Required]
    [MaxLength(150)]
    public string name { get; set; }

    public string description { get; set; }

    public bool requires_prescription { get; set; } = false;

    // --- Propiedades de Navegación para EF Core & HotChocolate ---
    
    [ForeignKey("product_id")]
    public virtual Product product { get; set; }

    [ForeignKey("id_brand")]
    public virtual Brands brand { get; set; }

    [ForeignKey("manufacturer_id")]
    public virtual Manufacturer manufacturer { get; set; }

    [ForeignKey("category_id")]
    public virtual Category category { get; set; }

    [ForeignKey("administration_route_id")]
    public virtual AdministrationRoute administration_route { get; set; }
    public virtual ICollection<MedicineActiveIngredient> medicine_active_ingredients { get; set; } = new List<MedicineActiveIngredient>();
}
