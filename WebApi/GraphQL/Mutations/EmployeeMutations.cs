using WebApi.Data;
using WebApi.GraphQL.Inputs;
using WebApi.Models.Empleados;
using WebApi.GraphQL.Payloads;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using HotChocolate.Types;
using System.Net.Mail; // Necesario para la validación de email

namespace WebApi.GraphQL.Mutations
{
    // Extendemos el tipo 'Mutation' principal para añadir estos métodos
    [ExtendObjectType(typeof(Mutation))]
    public class EmployeeMutations
    {
        // ========================================================================
        //                              ROLES (CARGOS)
        // ========================================================================

        /// <summary>
        /// Crea un nuevo cargo (Role) en el sistema.
        /// </summary>
        public async Task<MutationResult> CreateRoleAsync(
            CreateRoleInput input,
            [Service] AppDbContext context)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return new MutationResult(false, "El nombre del cargo es obligatorio");

            // Validar duplicados (ignorando mayúsculas/minúsculas)
            bool cargoExistente = await context.EmployeeRoles
                .AnyAsync(c => c.name.ToLower() == input.Name.ToLower());

            if (cargoExistente)
                return new MutationResult(false, "El cargo ya existe");

            var cargo = new EmployeeRoles
            {
                name = input.Name
            };

            try
            {
                context.EmployeeRoles.Add(cargo);
                await context.SaveChangesAsync();
                return new MutationResult(true, "Cargo agregado");
            }
            catch (Exception ex)
            {
                return new MutationResult(false, $"Error al guardar el cargo: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza un cargo existente.
        /// </summary>
        public async Task<MutationResult> UpdateRoleAsync(
            UpdateRoleInput input,
            [Service] AppDbContext context)
        {
            var cargo = await context.EmployeeRoles.FindAsync(input.Id);
            if (cargo == null)
                return new MutationResult(false, "Cargo no encontrado");

            // Actualizar nombre si viene en el input
            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                // Validar duplicados excluyendo el actual (para no chocar consigo mismo)
                bool existe = await context.EmployeeRoles
                    .AnyAsync(c => c.EmployeeRoleId != input.Id && c.name.ToLower() == input.Name.ToLower());

                if (existe)
                    return new MutationResult(false, "Ya existe un cargo con ese nombre");

                cargo.name = input.Name.Trim();
            }

            // Actualizar status si viene
            if (input.Status.HasValue)
            {
                cargo.status = input.Status.Value;
            }

            try
            {
                await context.SaveChangesAsync();
                return new MutationResult(true, "Cargo actualizado");
            }
            catch (Exception ex)
            {
                return new MutationResult(false, $"Error al actualizar el cargo: {ex.Message}");
            }
        }

        // ========================================================================
        //                              ESTATUS (EMPLOYEE STATUS)
        // ========================================================================

        /// <summary>
        /// Crea un nuevo estado para empleados.
        /// </summary>
        public async Task<MutationResult> CreateEmployeeStatusAsync(
            CreateStatusInput input,
            [Service] AppDbContext context)
        {
            var name = input.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return new MutationResult(false, "El nombre es obligatorio");

            bool existe = await context.EmployeeStatuses.AnyAsync(x => x.name.ToLower() == name.ToLower());
            if (existe)
                return new MutationResult(false, "Ya existe un estado con ese nombre");

            var entity = new EmployeeStatuses { name = name, status = true };

            try
            {
                context.EmployeeStatuses.Add(entity);
                await context.SaveChangesAsync();
                return new MutationResult(true, "Estado creado");
            }
            catch (Exception ex)
            {
                return new MutationResult(false, $"Error al crear estado: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza un estado existente.
        /// </summary>
        public async Task<MutationResult> UpdateEmployeeStatusAsync(
            UpdateStatusInput input,
            [Service] AppDbContext context)
        {
            var entity = await context.EmployeeStatuses.FindAsync(input.Id);
            if (entity == null)
                return new MutationResult(false, "Estado no encontrado");

            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                entity.name = input.Name.Trim();
            }

            if (input.Status.HasValue)
                entity.status = input.Status.Value;

            try
            {
                await context.SaveChangesAsync();
                return new MutationResult(true, "Estado actualizado");
            }
            catch (Exception ex)
            {
                return new MutationResult(false, $"Error al actualizar estado: {ex.Message}");
            }
        }

        // ========================================================================
        //                              EMPLEADOS (EMPLOYEES)
        // ========================================================================

        /// <summary>
        /// Crea un nuevo empleado con validaciones completas.
        /// </summary>
        public async Task<MutationResult> AddEmployeeAsync(
            AddEmployeeInput input,
            [Service] AppDbContext context)
        {
            // 1. Validaciones de campos obligatorios
            if (string.IsNullOrWhiteSpace(input.Names) ||
                string.IsNullOrWhiteSpace(input.Lastnames) ||
                string.IsNullOrWhiteSpace(input.User) ||
                string.IsNullOrWhiteSpace(input.Password))
            {
                return new MutationResult(false, "Names, Lastnames, User y Password son obligatorios");
            }

            // 2. Normalizaciones (Trim)
            var names = input.Names.Trim();
            var lastnames = input.Lastnames.Trim();
            var username = input.User.Trim();
            var phone = input.Phone?.Trim();
            var email = input.Email?.Trim();
            var url_photo = input.Url_photo?.Trim();

            // 3. Valores por defecto: Role=1 y Status=1 si envían 0 o nada
            var roleId = (input.EmployeeRoleId > 0) ? input.EmployeeRoleId : 1;
            var statusId = (input.EmployeeStatusId > 0) ? input.EmployeeStatusId : 1;

            // 4. Validar usuario duplicado
            bool userExists = await context.Employees.AnyAsync(e => e.user.ToLower() == username.ToLower());
            if (userExists)
                return new MutationResult(false, "El usuario ya existe");

            // 5. Verificar integridad referencial (que existan Roles y Estatus)
            bool roleOk = await context.EmployeeRoles.AnyAsync(r => r.EmployeeRoleId == roleId);
            bool statusOk = await context.EmployeeStatuses.AnyAsync(s => s.EmployeeStatusId == statusId);

            if (!roleOk || !statusOk)
                return new MutationResult(false, "RoleId o StatusId no válidos (asegúrate que existan; por defecto ID=1).");

            // 6. Crear la entidad
            var newEmployee = new Employee
            {
                names = names,
                lastnames = lastnames,
                phone = phone,
                user = username,
                password = input.Password, // TODO: Hashear esto en un servicio real
                email = email,
                url_photo = url_photo,
                // Si no envían fecha, usamos UtcNow
                hiring_date = input.Hiring_date == default ? DateTime.UtcNow : input.Hiring_date,
                EmployeeRoleId = roleId,
                EmployeeStatusId = statusId
            };

            try
            {
                context.Employees.Add(newEmployee);
                await context.SaveChangesAsync();
                return new MutationResult(true, "Empleado creado exitosamente");
            }
            catch (Exception ex)
            {
                return new MutationResult(false, $"Error al crear empleado: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza un empleado existente.
        /// </summary>
        /// <summary>
        /// Actualiza un empleado existente.
        /// </summary>
        public async Task<MutationResult> UpdateEmployeeAsync(
            UpdateEmployeeInput input,
            [Service] AppDbContext context)
        {
            var emp = await context.Employees.FindAsync(input.EmployeeId);
            if (emp == null)
                return new MutationResult(false, "Empleado no encontrado");

            // 1. Validación de user duplicado (si se intenta cambiar)
            if (!string.IsNullOrWhiteSpace(input.User))
            {
                var newUser = input.User.Trim();
                // Buscamos si existe ALGUIEN MÁS con ese usuario (distinto ID)
                bool userExists = await context.Employees
                    .AnyAsync(e => e.EmployeeId != input.EmployeeId && e.user.ToLower() == newUser.ToLower());

                if (userExists)
                    return new MutationResult(false, "Ya existe otro empleado con ese usuario");

                emp.user = newUser;
            }

            // 2. Actualizar textos básicos (si vienen en el input)
            if (!string.IsNullOrWhiteSpace(input.Names)) emp.names = input.Names.Trim();
            if (!string.IsNullOrWhiteSpace(input.Lastnames)) emp.lastnames = input.Lastnames.Trim();
            if (!string.IsNullOrWhiteSpace(input.Phone)) emp.phone = input.Phone.Trim();

            // 3. Validación lógica de Email (Idéntica al controlador)
            if (input.Email != null)
            {
                var mailStr = input.Email.Trim();
                if (mailStr.Length == 0)
                {
                    // Si envían string vacío, borramos el email
                    emp.email = null;
                }
                else if (mailStr.Length > 100)
                {
                    return new MutationResult(false, "Email demasiado largo (máx 100)");
                }
                else
                {
                    try
                    {
                        var _ = new MailAddress(mailStr); // Valida formato
                        emp.email = mailStr;
                    }
                    catch
                    {
                        return new MutationResult(false, "Email no válido");
                    }
                }
            }

            // 4. Validación lógica de Foto (Idéntica al controlador)
            if (input.Url_photo != null)
            {
                var url = input.Url_photo.Trim();
                if (url.Length == 0)
                    emp.url_photo = null; // Borrar foto
                else if (url.Length > 255)
                    return new MutationResult(false, "URL de foto demasiado larga (máx 255)");
                else
                    emp.url_photo = url;
            }

            // 5. Actualizar Password (si se envía)
            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                emp.password = input.Password; // ¡Recuerda Hashear!
            }

            // 6. Fecha contratación
            if (input.Hiring_date.HasValue)
                emp.hiring_date = input.Hiring_date.Value;

            // 7. Actualizar Rol (validando que exista)
            if (input.EmployeeRoleId.HasValue)
            {
                bool roleOk = await context.EmployeeRoles.AnyAsync(r => r.EmployeeRoleId == input.EmployeeRoleId.Value);
                if (!roleOk) return new MutationResult(false, "EmployeeRoleId no válido");
                emp.EmployeeRoleId = input.EmployeeRoleId.Value;
            }

            // 8. Actualizar Estado (validando que exista)
            if (input.EmployeeStatusId.HasValue)
            {
                bool statusOk = await context.EmployeeStatuses.AnyAsync(s => s.EmployeeStatusId == input.EmployeeStatusId.Value);
                if (!statusOk) return new MutationResult(false, "EmployeeStatusId no válido");
                emp.EmployeeStatusId = input.EmployeeStatusId.Value;
            }

            try
            {
                await context.SaveChangesAsync();
                return new MutationResult(true, "Empleado actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return new MutationResult(false, $"Error al actualizar: {ex.Message}");
            }
        }

        /// <summary>
        /// Alterna el estado del empleado entre ID 1 (Activo) y ID 2 (Inactivo).
        /// </summary>
        public async Task<MutationResult> ToggleEmployeeStatusAsync(
            int employeeId,
            [Service] AppDbContext context)
        {
            var emp = await context.Employees.FindAsync(employeeId);
            if (emp == null)
                return new MutationResult(false, "Empleado no encontrado");

            // Alternar entre 1 y 2
            emp.EmployeeStatusId = (emp.EmployeeStatusId == 1) ? 2 : 1;

            try
            {
                await context.SaveChangesAsync();
                string statusName = (emp.EmployeeStatusId == 1) ? "Activo" : "Inactivo";
                return new MutationResult(true, $"Estado del empleado cambiado a {statusName}");
            }
            catch (Exception ex)
            {
                return new MutationResult(false, $"Error al cambiar el estado: {ex.Message}");
            }
        }
    }
}