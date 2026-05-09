using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Models.Empleados;

namespace WebApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeRoles> EmployeeRoles { get; set; }
        public DbSet<EmployeeStatuses> EmployeeStatuses { get; set; }

        public DbSet<Brands> Brands { get; set; }
        
        public DbSet<Category> Categories { get; set; }
        
        public DbSet<AdministrationRoute> AdministrationRoutes { get; set; }
        
        public DbSet<Presentation> Presentations { get; set; }
        
        public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }
        
        public DbSet<DoseUnit> DoseUnits { get; set; }
        
        public DbSet<Manufacturer> Manufacturers { get; set; }
        
        public DbSet<ActiveIngredient> ActiveIngredients { get; set; }
        
        public DbSet<SupplierType> SupplierTypes { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }
        
        public DbSet<Product> Products => Set<Product>();
        
        public DbSet<Medicine> Medicines => Set<Medicine>();
        
        public DbSet<MedicineActiveIngredient> MedicineActiveIngredients => Set<MedicineActiveIngredient>();
        
        public DbSet<Batch> Batches => Set<Batch>();

        public DbSet<Customer> Customers { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionProduct> PromotionProducts { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<SalePayment> SalePayments { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");
                entity.HasKey(e => e.EmployeeId);
                entity.Property(e => e.names).IsRequired().HasMaxLength(100);
                entity.Property(e => e.lastnames).IsRequired().HasMaxLength(100);
                entity.Property(e => e.phone).HasMaxLength(20);
                entity.Property(e => e.user).IsRequired().HasMaxLength(50);
                entity.Property(e => e.password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.hiring_date).IsRequired();
                entity.Property(e => e.email).HasMaxLength(100);
                entity.Property(e => e.url_photo).HasMaxLength(255);

                entity.HasOne(e => e.EmployeeRole)
                      .WithMany(c => c.Employees)
                      .HasForeignKey(e => e.EmployeeRoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EmployeeStatus)
                      .WithMany(est => est.Employees)
                      .HasForeignKey(e => e.EmployeeStatusId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeRoles> (entity =>
            {
                entity.ToTable("EmployeeRoles");
                entity.HasKey(c => c.EmployeeRoleId);
                entity.Property(c => c.name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.status).HasDefaultValue(true);
            });

            modelBuilder.Entity<EmployeeStatuses>(entity =>
            {
                entity.ToTable("EmployeeStatuses");
                entity.HasKey(est => est.EmployeeStatusId);
                entity.Property(est => est.name).IsRequired().HasMaxLength(100);
                entity.Property(est => est.status).HasDefaultValue(true);
            });

            modelBuilder.Entity<Brands>(entity =>
            {
                entity.ToTable("brands");
                entity.HasKey(b => b.id_brand);
                entity.Property(b => b.name).IsRequired().HasMaxLength(100);
                entity.Property(b => b.name).IsRequired(); 
                entity.Property(b => b.logo_url).HasMaxLength(250);
                entity.Property(b => b.contact_phone).HasMaxLength(15);
                entity.Property(b => b.contact_email).HasMaxLength(100);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");
                entity.HasKey(c => c.category_id);
                entity.Property(c => c.name).HasMaxLength(100);
                entity.Property(c => c.description).HasMaxLength(10000);
                entity.Property(c => c.is_active).HasDefaultValue(true);
            });

            modelBuilder.Entity<AdministrationRoute>(entity =>
            {
                entity.ToTable("AdministrationRoute");
                entity.HasKey(a => a.administration_route_id); 
                entity.Property(a => a.name).HasMaxLength(100);
            });

            modelBuilder.Entity<ActiveIngredient>(entity =>
            {
                entity.ToTable("ActiveIngredient");
                entity.HasKey(a => a.active_ingredient_id); 
                entity.Property(a => a.name).HasMaxLength(100);
            });

            modelBuilder.Entity<Presentation>(entity =>
            {
                entity.ToTable("Presentation");
                entity.HasKey(a => a.presentation_id);
                entity.Property(a => a.name).HasMaxLength(100);
            });

            modelBuilder.Entity<UnitOfMeasure>(entity =>
            {   
                entity.ToTable("UnitOfMeasure");
                entity.HasKey(u => u.unit_of_measure_id);
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.ToTable("Manufacturer");
                entity.HasKey(u => u.manufacturer_id);
            });

            modelBuilder.Entity<DoseUnit>(entity =>
            {
                entity.ToTable("DoseUnit");
                entity.HasKey(d => d.dose_unit_id); 
            });

            modelBuilder.Entity<SupplierType>(entity =>
            {
                entity.ToTable("SupplierType");
                entity.HasKey(st => st.supplier_type_id); 
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Supplier");
                entity.HasKey(s => s.supplier_id);

                entity.HasOne(s => s.type)
                    .WithMany(st => st.Suppliers)
                    .HasForeignKey(s => s.supplier_type_id) // Changed to the correct FK
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --------------------------------------------------------
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");
                entity.HasKey(e => e.product_id);

                entity.Property(e => e.barcode).HasMaxLength(50);

                // Precisión para decimales
                entity.Property(e => e.price_full_presentation).HasPrecision(10, 2);
                entity.Property(e => e.price_per_unit).HasPrecision(10, 2);

                // Valores por defecto
                entity.Property(e => e.units_per_presentation).HasDefaultValue(1);
                entity.Property(e => e.stock_units).HasDefaultValue(0);
                entity.Property(e => e.min_stock_units).HasDefaultValue(10);
                entity.Property(e => e.product_status_id).HasDefaultValue(1);
                entity.Property(e => e.currency).HasDefaultValue("NIO").HasMaxLength(20);
                entity.Property(e => e.created_at).HasDefaultValueSql("GETDATE()");

                // --- RELACIONES CORREGIDAS ---

                // Relación con Supplier
                // Nota: Asegúrate que en la clase Supplier la colección se llame 'Products' y no 'Suppliers'
                entity.HasOne(d => d.supplier)
                    .WithMany() 
                    .HasForeignKey(d => d.supplier_id)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Presentation
                entity.HasOne(d => d.presentation) // Minúscula como en tu clase
                    .WithMany() // Si Presentation no tiene una lista de productos, déjalo vacío
                    .HasForeignKey(d => d.presentation_id)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con UnitOfMeasure
                entity.HasOne(d => d.unit_of_measure) // Minúscula como en tu clase
                    .WithMany()
                    .HasForeignKey(d => d.unit_of_measure_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.ToTable("Medicine");
                entity.HasKey(m => m.medicine_id);

                entity.Property(m => m.name).IsRequired().HasMaxLength(150);
                entity.Property(m => m.requires_prescription).HasDefaultValue(false);

                // Relación 1:1 o 1:N con Product (Depende de si un producto solo puede ser una medicina)
                // Generalmente es 1:1 en farmacia (Un item de inventario = un medicamento específico)
                entity.HasOne(m => m.product)
                    .WithOne(p => p.medicine) // Si Product no tiene una propiedad virtual Medicine
                    .HasForeignKey<Medicine>(m => m.product_id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.brand)
                    .WithMany()
                    .HasForeignKey(m => m.id_brand)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.manufacturer)
                    .WithMany()
                    .HasForeignKey(m => m.manufacturer_id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.category)
                    .WithMany()
                    .HasForeignKey(m => m.category_id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.administration_route)
                    .WithMany()
                    .HasForeignKey(m => m.administration_route_id)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<MedicineActiveIngredient>(entity =>
            {
                entity.ToTable("MedicineActiveIngredient");

                // Configuración de la Clave Primaria Compuesta
                entity.HasKey(ma => new { ma.medicine_id, ma.active_ingredient_id });

                entity.Property(ma => ma.dose_value)
                    .HasPrecision(10, 2)
                    .IsRequired();

                // Relaciones
                entity.HasOne(ma => ma.medicine)
                    .WithMany(m => m.medicine_active_ingredients)
                    .HasForeignKey(ma => ma.medicine_id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ma => ma.active_ingredient)
                    .WithMany()
                    .HasForeignKey(ma => ma.active_ingredient_id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ma => ma.dose_unit)
                    .WithMany()
                    .HasForeignKey(ma => ma.dose_unit_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Batch>(entity =>
            {
                entity.ToTable("Batch");
                entity.HasKey(b => b.batch_id);

                entity.Property(b => b.batch_code).IsRequired().HasMaxLength(50);
                entity.HasIndex(b => new { b.product_id, b.batch_code }).IsUnique();
                entity.Property(b => b.expiration_date).IsRequired();
                entity.Property(b => b.is_active).HasDefaultValue(true);
                entity.Property(b => b.created_at).HasDefaultValueSql("GETDATE()");

                // Relación con Product
                // Un producto puede tener muchos lotes (ej: Aspirinas de diferentes vencimientos)
                entity.HasOne(b => b.product)
                    .WithMany(p => p.batches) // Recuerda agregar public virtual ICollection<Batch> batches a la clase Product
                    .HasForeignKey(b => b.product_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer");
                entity.HasKey(c => c.CustomerId);
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("PaymentMethod");
                entity.HasKey(p => p.PaymentMethodId);
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotion");
                entity.HasKey(p => p.PromotionId);
                entity.Property(p => p.DiscountValue).HasPrecision(18, 4);
            });

            modelBuilder.Entity<PromotionProduct>(entity =>
            {
                entity.ToTable("PromotionProduct");
                entity.HasKey(pp => new { pp.PromotionId, pp.ProductId });

                entity.HasOne(pp => pp.Promotion)
                    .WithMany(p => p.PromotionProducts)
                    .HasForeignKey(pp => pp.PromotionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pp => pp.Product)
                    .WithMany()
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("Sale");
                entity.HasKey(s => s.SaleId);
                entity.Property(s => s.GrossSubtotal).HasPrecision(18, 2);
                entity.Property(s => s.TotalDiscount).HasPrecision(18, 2);
                entity.Property(s => s.TotalTax).HasPrecision(18, 2);
                entity.Property(s => s.NetTotal).HasPrecision(18, 2);
                entity.Property(s => s.SaleDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(s => s.Customer)
                    .WithMany(c => c.Sales)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Employee)
                    .WithMany()
                    .HasForeignKey(s => s.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.ToTable("SaleDetail");
                entity.HasKey(sd => sd.SaleDetailId);
                entity.Property(sd => sd.AppliedCostPrice).HasPrecision(18, 4);
                entity.Property(sd => sd.AppliedUnitPrice).HasPrecision(18, 4);
                entity.Property(sd => sd.AppliedTaxRate).HasPrecision(5, 4);
                entity.Property(sd => sd.CalculatedDiscount).HasPrecision(18, 2);
                entity.Property(sd => sd.CalculatedTax).HasPrecision(18, 2);
                entity.Property(sd => sd.LineTotal).HasPrecision(18, 2);

                entity.HasOne(sd => sd.Sale)
                    .WithMany(s => s.SaleDetails)
                    .HasForeignKey(sd => sd.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sd => sd.Product)
                    .WithMany()
                    .HasForeignKey(sd => sd.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sd => sd.Batch)
                    .WithMany()
                    .HasForeignKey(sd => sd.BatchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sd => sd.Promotion)
                    .WithMany()
                    .HasForeignKey(sd => sd.PromotionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SalePayment>(entity =>
            {
                entity.ToTable("SalePayment");
                entity.HasKey(sp => sp.SalePaymentId);
                entity.Property(sp => sp.Amount).HasPrecision(18, 2);

                entity.HasOne(sp => sp.Sale)
                    .WithMany(s => s.SalePayments)
                    .HasForeignKey(sp => sp.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sp => sp.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(sp => sp.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.ToTable("InventoryTransaction");
                entity.HasKey(it => it.TransactionId);
                entity.Property(it => it.UnitCost).HasPrecision(18, 4);
                entity.Property(it => it.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(it => it.Product)
                    .WithMany()
                    .HasForeignKey(it => it.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(it => it.Batch)
                    .WithMany()
                    .HasForeignKey(it => it.BatchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(it => it.Employee)
                    .WithMany()
                    .HasForeignKey(it => it.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
