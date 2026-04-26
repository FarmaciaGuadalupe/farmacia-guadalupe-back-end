using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Models.DTOs;
using WebApi.Models.Empleados;
using WebApi.Models.Empleados.DTOs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _dataBase;

        public EmployeeController(AppDbContext context)
        {
            _dataBase = context;
        }
        
        [HttpPost("createRole/")]
        public async Task<ActionResult<ApiResponse>> CrearCargo([FromBody] RoleDto cargoDto)
        {
            if (string.IsNullOrWhiteSpace(cargoDto.name))
            {
                return BadRequest(new ApiResponse
                {
                    success = false,
                    message = "El nombre del cargo es obligatorio"
                });
            }

            // Validar si ya existe un cargo con el mismo nombre (ignorando mayúsculas/minúsculas)
            bool cargoExistente = await _dataBase.EmployeeRoles
                .AnyAsync(c => c.name.ToLower() == cargoDto.name.ToLower());

            if (cargoExistente)
            {
                return Conflict(new ApiResponse
                {
                    success = false,
                    message = "El cargo ya existe"
                });
            }

            var cargo = new EmployeeRoles
            {
                name = cargoDto.name
            };

            try
            {
                _dataBase.EmployeeRoles.Add(cargo);
                await _dataBase.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    success = true,
                    message = "Cargo agregado"
                });
            }
            catch (DbUpdateException dbEx)
            {
                var root = dbEx.GetBaseException()?.Message; // <- mensaje de SQL real
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"No se pudo guardar el cargo: {root}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }

        }

        [HttpGet("getAllRoles/")]
        public async Task<ActionResult<ApiResponse>> GetAllCargos()
        {
            try
            {
                var cargos = await _dataBase.EmployeeRoles.ToListAsync();

                return Ok(new ApiResponse
                {
                    success = true,
                    data = cargos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error al obtener los cargos: {ex.Message}"
                });
            }
        }

        [HttpPut("updateRole/{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateCargo(int id, [FromBody] UpdateCargoDto cargoDto)
        {
            var cargo = await _dataBase.EmployeeRoles.FindAsync(id);
            if (cargo == null)
            {
                return NotFound(new ApiResponse
                {
                    success = false,
                    message = "Cargo no encontrado"
                });
            }

            // Actualizar solo si viene en el DTO
            if (!string.IsNullOrWhiteSpace(cargoDto.name))
            {
                // Validar duplicados
                bool existe = await _dataBase.EmployeeRoles
                    .AnyAsync(c => c.EmployeeRoleId != id && c.name.ToLower() == cargoDto.name.ToLower());
                if (existe)
                {
                    return Conflict(new ApiResponse
                    {
                        success = false,
                        message = "Ya existe un cargo con ese nombre"
                    });
                }

                cargo.name = cargoDto.name.Trim();
            }

            if (cargoDto.status.HasValue)
            {
                cargo.status = cargoDto.status.Value;
            }

            try
            {
                await _dataBase.SaveChangesAsync();
                return Ok(new ApiResponse
                {
                    success = true,
                    message = "Cargo actualizado",
                    data = cargo
                });
            }
            catch (DbUpdateException dbEx)
            {
                var root = dbEx.GetBaseException()?.Message;
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error al actualizar el cargo: {root}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }


        //--------------------------------------------------------------
        [HttpPost("createEmployeeStatus")]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] EmployeeStatusDto dto)
        {
            var name = dto?.name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new ApiResponse { success = false, message = "El nombre es obligatorio" });

            bool existe = await _dataBase.EmployeeStatuses.AnyAsync(x => x.name.ToLower() == name.ToLower());
            if (existe)
                return Conflict(new ApiResponse { success = false, message = "Ya existe un estado con ese nombre" });

            var entity = new EmployeeStatuses { name = name, status = true };

            try
            {
                _dataBase.EmployeeStatuses.Add(entity);
                await _dataBase.SaveChangesAsync();
                return Ok(new ApiResponse { success = true, message = "Estado creado", data = entity });
            }
            catch (DbUpdateException dbEx)
            {
                var root = dbEx.GetBaseException()?.Message;
                return StatusCode(500, new ApiResponse { success = false, message = $"No se pudo crear: {root}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("getAllEmployeeStatus")]
        public async Task<ActionResult<ApiResponse>> GetAll()
        {
            try
            {
                var list = await _dataBase.EmployeeStatuses.ToListAsync();
                return Ok(new ApiResponse { success = true, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { success = false, message = $"Error al obtener: {ex.Message}" });
            }
        }

        // UPDATE (name y/o status)
        [HttpPut("updateEmployeeStatus/{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateEmployeeStatusDto dto)
        {
            var entity = await _dataBase.EmployeeStatuses.FindAsync(id);
            if (entity == null)
                return NotFound(new ApiResponse { success = false, message = "Estado no encontrado" });

            // Actualizar nombre si viene
            if (!string.IsNullOrWhiteSpace(dto.name))
            {
                entity.name = dto.name.Trim();
            }

            // Actualizar estado si viene
            if (dto.status.HasValue)
                entity.status = dto.status.Value;

            try
            {
                await _dataBase.SaveChangesAsync();
                return Ok(new ApiResponse
                {
                    success = true,
                    message = "Estado actualizado",
                    data = entity
                });
            }
            catch (DbUpdateException dbEx)
            {
                var root = dbEx.GetBaseException()?.Message;
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error al actualizar: {root}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        //-------------------------------------------------------------

        [HttpPost("createEmployee")]
        public async Task<ActionResult<ApiResponse>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            // Validaciones mínimas
            if (dto == null)
                return BadRequest(new ApiResponse { success = false, message = "Datos inválidos" });

            if (string.IsNullOrWhiteSpace(dto.names) ||
                string.IsNullOrWhiteSpace(dto.lastnames) ||
                string.IsNullOrWhiteSpace(dto.user) ||
                string.IsNullOrWhiteSpace(dto.password))
            {
                return BadRequest(new ApiResponse
                {
                    success = false,
                    message = "names, lastnames, user y password son obligatorios"
                });
            }

            // Normalizaciones simples
            var names = dto.names.Trim();
            var lastnames = dto.lastnames.Trim();
            var username = dto.user.Trim();
            var phone = dto.phone?.Trim();
            var email = dto.email?.Trim();
            var url_photo = dto.url_photo?.Trim();

            // Por defecto Role=1 y Status=1
            var roleId = (dto.EmployeeRoleId.HasValue && dto.EmployeeRoleId.Value > 0) ? dto.EmployeeRoleId.Value : 1;
            var statusId = (dto.EmployeeStatusId.HasValue && dto.EmployeeStatusId.Value > 0) ? dto.EmployeeStatusId.Value : 1;

            // (Opcional) Validar duplicado de user
            bool userExists = await _dataBase.Employees.AnyAsync(e => e.user.ToLower() == username.ToLower());
            if (userExists)
            {
                return Conflict(new ApiResponse
                {
                    success = false,
                    message = "El usuario ya existe"
                });
            }

            // (Opcional pero recomendado) Verificar que existan RoleId y StatusId referenciados
            bool roleOk = await _dataBase.EmployeeRoles.AnyAsync(r => r.EmployeeRoleId == roleId);
            bool statusOk = await _dataBase.EmployeeStatuses.AnyAsync(s => s.EmployeeStatusId == statusId);

            if (!roleOk || !statusOk)
            {
                return BadRequest(new ApiResponse
                {
                    success = false,
                    message = "RoleId o StatusId no válidos (asegúrate que existan; por defecto deberían existir los ID=1)."
                });
            }

            var employee = new Employee
            {
                names = names,
                lastnames = lastnames,
                phone = phone,
                user = username,
                // Nota: en producción NO guardes password en texto plano.
                // Aquí lo dejamos como pides; idealmente usa hashing (ej. BCrypt/ASP.NET Identity).
                password = BCrypt.Net.BCrypt.HashPassword(dto.password),
                email = email,
                url_photo = url_photo,
                hiring_date = dto.hiring_date == default ? DateTime.UtcNow : dto.hiring_date,
                EmployeeRoleId = roleId,
                EmployeeStatusId = statusId
            };

            try
            {
                _dataBase.Employees.Add(employee);
                await _dataBase.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    success = true,
                    message = "Empleado creado",
                    data = employee
                });
            }
            catch (DbUpdateException dbEx)
            {
                var root = dbEx.GetBaseException()?.Message;
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"No se pudo crear el empleado: {root}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }


        [HttpGet("getAllEmployees")]
        public async Task<ActionResult<ApiResponse>> GetAllEmployees()
        {
            try
            {
                var employees = await _dataBase.Employees
                    .Include(e => e.EmployeeRole)
                    .Include(e => e.EmployeeStatus)
                    .Select(e => new EmployeeDto
                    {
                        EmployeeId = e.EmployeeId,
                        names = e.names,
                        lastnames = e.lastnames,
                        phone = e.phone,
                        user = e.user,
                        email = e.email,
                        url_photo = e.url_photo,
                        hiring_date = e.hiring_date,
                        EmployeeRoleId = e.EmployeeRoleId,
                        EmployeeStatusId = e.EmployeeStatusId,
                        RoleName = e.EmployeeRole.name,
                        StatusName = e.EmployeeStatus.name
                    })
                    .ToListAsync();

                return Ok(new ApiResponse
                {
                    success = true,
                    data = employees
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error al obtener empleados: {ex.Message}"
                });
            }
        }

        [HttpGet("getEmployeeById/{id}")]
        public async Task<ActionResult<ApiResponse>> GetEmployeeById(int id)
        {
            try
            {
                var employee = await _dataBase.Employees
                    .Include(e => e.EmployeeRole)
                    .Include(e => e.EmployeeStatus)
                    .Where(e => e.EmployeeId == id)
                    .Select(e => new EmployeeDto
                    {
                        EmployeeId = e.EmployeeId,
                        names = e.names,
                        lastnames = e.lastnames,
                        phone = e.phone,
                        user = e.user,
                        email = e.email,
                        url_photo = e.url_photo,
                        hiring_date = e.hiring_date,
                        EmployeeRoleId = e.EmployeeRoleId,
                        EmployeeStatusId = e.EmployeeStatusId,
                        RoleName = e.EmployeeRole.name,
                        StatusName = e.EmployeeStatus.name
                    })
                    .FirstOrDefaultAsync();

                if (employee == null)
                {
                    return NotFound(new ApiResponse
                    {
                        success = false,
                        message = "Empleado no encontrado"
                    });
                }

                return Ok(new ApiResponse
                {
                    success = true,
                    data = employee
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    success = false,
                    message = $"Error al obtener empleado: {ex.Message}"
                });
            }
        }

        [HttpPut("updateEmployee/{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var emp = await _dataBase.Employees.FindAsync(id);
            if (emp == null)
                return NotFound(new ApiResponse { success = false, message = "Empleado no encontrado" });

            // Validación de user (único)
            if (!string.IsNullOrWhiteSpace(dto.user))
            {
                var newUser = dto.user.Trim();
                bool userExists = await _dataBase.Employees
                    .AnyAsync(e => e.EmployeeId != id && e.user.ToLower() == newUser.ToLower());
                if (userExists)
                    return Conflict(new ApiResponse { success = false, message = "Ya existe otro empleado con ese usuario" });
                emp.user = newUser;
            }

            // Campos de texto
            if (!string.IsNullOrWhiteSpace(dto.names)) emp.names = dto.names.Trim();
            if (!string.IsNullOrWhiteSpace(dto.lastnames)) emp.lastnames = dto.lastnames.Trim();
            if (!string.IsNullOrWhiteSpace(dto.phone)) emp.phone = dto.phone.Trim();

            // Email (opcional; acepta null/"" para borrar)
            if (dto.email != null) // diferencia entre “no enviado” y “enviado vacío”
            {
                var email = dto.email.Trim();
                if (email.Length == 0) emp.email = null;          // borra
                else if (email.Length > 100) return BadRequest(new ApiResponse { success = false, message = "Email demasiado largo (máx 100)" });
                else
                {
                    try
                    {
                        // Validación simple de formato
                        var _ = new System.Net.Mail.MailAddress(email);
                        emp.email = email;
                    }
                    catch
                    {
                        return BadRequest(new ApiResponse { success = false, message = "Email no válido" });
                    }
                }
            }

            // URL foto (opcional; acepta null/"" para borrar)
            if (dto.url_photo != null)
            {
                var url = dto.url_photo.Trim();
                if (url.Length == 0) emp.url_photo = null;       // borra
                else if (url.Length > 255) return BadRequest(new ApiResponse { success = false, message = "url_photo demasiado larga (máx 255)" });
                else emp.url_photo = url;
            }

            // Password (opcional)
            if (!string.IsNullOrWhiteSpace(dto.password))
                emp.password = dto.password; // TODO: hashear en prod

            // Fecha de contratación
            if (dto.hiring_date.HasValue) emp.hiring_date = dto.hiring_date.Value;

            // FK Rol
            if (dto.EmployeeRoleId.HasValue)
            {
                bool roleOk = await _dataBase.EmployeeRoles.AnyAsync(r => r.EmployeeRoleId == dto.EmployeeRoleId.Value);
                if (!roleOk) return BadRequest(new ApiResponse { success = false, message = "EmployeeRoleId no válido" });
                emp.EmployeeRoleId = dto.EmployeeRoleId.Value;
            }

            // FK Status
            if (dto.EmployeeStatusId.HasValue)
            {
                bool statusOk = await _dataBase.EmployeeStatuses.AnyAsync(s => s.EmployeeStatusId == dto.EmployeeStatusId.Value);
                if (!statusOk) return BadRequest(new ApiResponse { success = false, message = "EmployeeStatusId no válido" });
                emp.EmployeeStatusId = dto.EmployeeStatusId.Value;
            }

            try
            {
                await _dataBase.SaveChangesAsync();
                return Ok(new ApiResponse
                {
                    success = true,
                    message = "Empleado actualizado",
                    data = new
                    {
                        emp.EmployeeId,
                        emp.names,
                        emp.lastnames,
                        emp.phone,
                        emp.user,
                        emp.email,
                        emp.url_photo,
                        emp.hiring_date,
                        emp.EmployeeRoleId,
                        emp.EmployeeStatusId
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                var root = dbEx.GetBaseException()?.Message;
                return StatusCode(500, new ApiResponse { success = false, message = $"Error al actualizar: {root}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { success = false, message = $"Error: {ex.Message}" });
            }
        }



    }
}

