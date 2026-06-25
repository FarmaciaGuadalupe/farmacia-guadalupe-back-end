using WebApi.Models;
using WebApi.Data;
using HotChocolate;
using HotChocolate.Types;

namespace WebApi.GraphQL.Mutations;

[ExtendObjectType("Mutation")]
public class SupplierMutations
{
    // CREATE: Agregar un nuevo proveedor
    public async Task<Supplier> CreateSupplierAsync(
        int supplierTypeId,
        string? companyName,
        string? taxId,
        string? contactName,
        string? phone,
        string? address,
        string? email,
        string? website,
        [Service] AppDbContext context)
    {
        // Verificar si el tipo de proveedor existe
        var typeExists = await context.SupplierTypes.FindAsync(supplierTypeId);
        if (typeExists == null)
        {
            throw new GraphQLException(new Error("Supplier Type not found", "NOT_FOUND"));
        }

        var supplier = new Supplier
        {
            supplier_type_id = supplierTypeId,
            company_name = companyName,
            tax_id = taxId,
            contact_name = contactName,
            phone = phone,
            address = address,
            email = email,
            website = website,
            is_active = true
        };

        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();
        return supplier;
    }

    // UPDATE: Editar un proveedor existente
    public async Task<Supplier> UpdateSupplierAsync(
        int id,
        int? supplierTypeId,
        string? companyName,
        string? taxId,
        string? contactName,
        string? phone,
        string? address,
        string? email,
        string? website,
        bool? isActive,
        [Service] AppDbContext context)
    {
        var supplier = await context.Suppliers.FindAsync(id)
                       ?? throw new GraphQLException(new Error("Supplier not found", "NOT_FOUND"));

        if (supplierTypeId.HasValue)
        {
            var typeExists = await context.SupplierTypes.FindAsync(supplierTypeId.Value);
            if (typeExists == null)
            {
                throw new GraphQLException(new Error("Supplier Type not found", "NOT_FOUND"));
            }
            supplier.supplier_type_id = supplierTypeId.Value;
        }

        if (companyName != null) supplier.company_name = companyName;
        if (taxId != null) supplier.tax_id = taxId;
        if (contactName != null) supplier.contact_name = contactName;
        if (phone != null) supplier.phone = phone;
        if (address != null) supplier.address = address;
        if (email != null) supplier.email = email;
        if (website != null) supplier.website = website;
        if (isActive.HasValue) supplier.is_active = isActive.Value;

        await context.SaveChangesAsync();
        return supplier;
    }
}
