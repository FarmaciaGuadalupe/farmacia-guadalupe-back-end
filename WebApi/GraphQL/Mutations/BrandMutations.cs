using WebApi.Data;
using WebApi.Models;
using HotChocolate;
using HotChocolate.Types;

namespace WebApi.GraphQL.Mutations
{
    [ExtendObjectType(typeof(Mutation))]
    public class BrandMutations
    {
        public async Task<Brands> AddBrandAsync(
            [Service] AppDbContext context, 
            string name, string phone, string email)
        {
            var now = DateTime.UtcNow;
            var newBrand = new Brands
            {
                name = name,
                contact_phone = phone,
                contact_email = email,
                is_active = true,
                created_at = now,
                updated_at = now
            };

            context.Brands.Add(newBrand);
            await context.SaveChangesAsync();

            return newBrand;
        }
   
    public async Task<Brands> UpdateBrandNameAsync(
            [Service] AppDbContext context,
            int id_brand,
            string newName)
        {
            // Buscamos la marca por su ID
            var brand = await context.Brands.FindAsync(id_brand);

            if (brand == null)
            {
                throw new GraphQLException("La marca no existe.");
            }

            // Actualizamos el nombre
            brand.name = newName;

            // Guardamos cambios
            await context.SaveChangesAsync();

            return brand;
        }

        public async Task<Brands> UpdateBrandAsync(
            [Service] AppDbContext context,
            int id_brand,
            string? name,
            string? logo_url,
            string? contact_phone,
            string? contact_email,
            bool? is_active)
        {
            var brand = await context.Brands.FindAsync(id_brand);

            if (brand == null)
            {
                throw new GraphQLException("La marca no existe.");
            }

            if (name != null) brand.name = name;
            if (logo_url != null) brand.logo_url = logo_url;
            if (contact_phone != null) brand.contact_phone = contact_phone;
            if (contact_email != null) brand.contact_email = contact_email;
            if (is_active.HasValue) brand.is_active = is_active.Value;

            brand.updated_at = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return brand;
        }

        // 3. Mutación para DESACTIVAR
        public async Task<Brands> DeactivateBrandAsync(
            [Service] AppDbContext context,
            int id_brand)
        {
            var brand = await context.Brands.FindAsync(id_brand);

            if (brand == null)
            {
                throw new GraphQLException("La marca no existe.");
            }

            // Cambiamos el estado a inactivo (false)
            brand.is_active = false;

            await context.SaveChangesAsync();

            return brand;
        }

        public async Task<Brands> ToggleBrandStatusAsync(
    [Service] AppDbContext context,
    int id_brand)
        {
            var brand = await context.Brands.FindAsync(id_brand);

            if (brand == null)
            {
                throw new GraphQLException("La marca no existe.");
            }

            // LÓGICA DE SWITCH:
            // El operador '!' invierte el valor booleano.
            // Si es true, se vuelve false. Si es false, se vuelve true.
            brand.is_active = !brand.is_active;

            await context.SaveChangesAsync();

            return brand;
        }

    }
}
