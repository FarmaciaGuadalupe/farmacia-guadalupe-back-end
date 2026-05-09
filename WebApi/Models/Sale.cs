using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApi.Models.Empleados;

namespace WebApi.Models;

public class Sale
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("sale_id")]
    public int SaleId { get; set; }

    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("receipt_type")]
    public string ReceiptType { get; set; }

    [MaxLength(50)]
    [Column("receipt_number")]
    public string ReceiptNumber { get; set; }

    [MaxLength(50)]
    [Column("prescription_number")]
    public string? PrescriptionNumber { get; set; }

    [MaxLength(150)]
    [Column("doctor_name")]
    public string? DoctorName { get; set; }

    [Required]
    [Column("gross_subtotal", TypeName = "decimal(18,2)")]
    public decimal GrossSubtotal { get; set; }

    [Column("total_discount", TypeName = "decimal(18,2)")]
    public decimal TotalDiscount { get; set; } = 0m;

    [Required]
    [Column("total_tax", TypeName = "decimal(18,2)")]
    public decimal TotalTax { get; set; }

    [Required]
    [Column("net_total", TypeName = "decimal(18,2)")]
    public decimal NetTotal { get; set; }

    [MaxLength(10)]
    [Column("currency")]
    public string Currency { get; set; } = "NIO";

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "COMPLETED";

    [Column("sale_date")]
    public DateTime SaleDate { get; set; } = DateTime.Now;

    // --- Propiedades de Navegación ---

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; }

    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    public virtual ICollection<SalePayment> SalePayments { get; set; } = new List<SalePayment>();
}
