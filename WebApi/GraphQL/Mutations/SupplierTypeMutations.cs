using WebApi.Models;
using WebApi.Data;
using HotChocolate;
using HotChocolate.Types;

namespace WebApi.GraphQL.Mutations;

[ExtendObjectType("Mutation")]
public class SupplierTypeMutations
{
    // CREATE: Agregar un nuevo tipo de proveedor
    public async Task<SupplierType> CreateSupplierTypeAsync(
        string typeName, 
        string description, 
        [Service] AppDbContext context)
    {
        var supplierType = new SupplierType 
        { 
            type_name = typeName, 
            description = description 
        };

        context.SupplierTypes.Add(supplierType);
        await context.SaveChangesAsync();
        return supplierType;
    }

    // UPDATE: Modificar un tipo de proveedor existente
    public async Task<SupplierType> UpdateSupplierTypeAsync(
        int id, 
        string? typeName, 
        string? description, 
        [Service] AppDbContext context)
    {
        var supplierType = await context.SupplierTypes.FindAsync(id) 
                           ?? throw new GraphQLException(new Error("Supplier Type not found", "NOT_FOUND"));

        if (typeName != null) supplierType.type_name = typeName;
        if (description != null) supplierType.description = description;

        await context.SaveChangesAsync();
        return supplierType;
    }
}
