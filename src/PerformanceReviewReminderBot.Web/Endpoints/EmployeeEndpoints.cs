using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for employee CRUD and teammate management.
/// Maps all routes under <c>/api/employees</c>.
/// </summary>
public static class EmployeeEndpoints
{
    /// <summary>
    /// Registers the <c>/api/employees</c> endpoint group on the application.
    /// </summary>
    public static void MapEmployeeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/employees")
            .WithTags("Employees");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<EmployeeRequest>>();
        group.MapPut("/{id:int}", UpdateAsync).AddEndpointFilter<ValidationFilter<EmployeeRequest>>();
        group.MapDelete("/{id:int}", DeleteAsync);

        group.MapGet("/{id:int}/teammates", GetTeammatesAsync);
        group.MapPost("/{id:int}/teammates/{teammateId:int}", AddTeammateAsync);
        group.MapDelete("/{id:int}/teammates/{teammateId:int}", RemoveTeammateAsync);
    }

    /// <summary>
    /// GET /api/employees — list all employees, optionally filtered by role.
    /// </summary>
    /// <param name="role">Optional query parameter: "Employee" or "TalentManager".</param>
    private static async Task<IResult> GetAllAsync(
        EmployeeService service,
        string? role = null)
    {
        var employees = await service.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(role)
            && Enum.TryParse<EmployeeRole>(role, ignoreCase: true, out var parsed))
        {
            employees = employees.Where(e => e.Role == parsed).ToList();
        }

        return Results.Ok(employees.Select(ToResponse));
    }

    /// <summary>
    /// GET /api/employees/{id} — get a single employee by id.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(
        int id,
        EmployeeService service)
    {
        var employee = await service.GetByIdAsync(id);

        if (employee is null)
        {
            return Results.NotFound(new { message = $"Employee with Id {id} not found." });
        }

        return Results.Ok(ToResponse(employee));
    }

    /// <summary>
    /// POST /api/employees — create a new employee.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        EmployeeRequest request,
        EmployeeService service)
    {
        try
        {
            // Guard against out-of-range enum values that pass integer deserialisation.
            if (!Enum.IsDefined(typeof(EmployeeRole), request.Role))
            {
                return Results.BadRequest(new { message = $"Invalid Role value '{request.Role}'." });
            }

            var employee = new Employee
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = (EmployeeRole)request.Role,
                TalentManagerId = request.TalentManagerId
            };

            var created = await service.CreateAsync(employee);
            var response = ToResponse(created);

            return Results.Created($"/api/employees/{response.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/employees/{id} — update an existing employee.
    /// </summary>
    private static async Task<IResult> UpdateAsync(
        int id,
        EmployeeRequest request,
        EmployeeService service)
    {
        try
        {
            // Guard against out-of-range enum values that pass integer deserialisation.
            if (!Enum.IsDefined(typeof(EmployeeRole), request.Role))
            {
                return Results.BadRequest(new { message = $"Invalid Role value '{request.Role}'." });
            }

            var employee = new Employee
            {
                Id = id,
                FullName = request.FullName,
                Email = request.Email,
                Role = (EmployeeRole)request.Role,
                TalentManagerId = request.TalentManagerId
            };

            var updated = await service.UpdateAsync(employee);

            return Results.Ok(ToResponse(updated));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/employees/{id} — delete an employee.
    /// Returns 409 Conflict when the employee has reviews or managed employees.
    /// </summary>
    private static async Task<IResult> DeleteAsync(
        int id,
        EmployeeService service)
    {
        try
        {
            await service.DeleteAsync(id);

            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/employees/{id}/teammates — list teammates for an employee.
    /// </summary>
    private static async Task<IResult> GetTeammatesAsync(
        int id,
        TeammateService service)
    {
        var teammates = await service.GetTeammatesAsync(id);

        return Results.Ok(teammates.Select(t => new TeammateResponse(t.Id, t.FullName, t.Email)));
    }

    /// <summary>
    /// POST /api/employees/{id}/teammates/{teammateId} — add a teammate.
    /// </summary>
    private static async Task<IResult> AddTeammateAsync(
        int id,
        int teammateId,
        TeammateService service)
    {
        try
        {
            await service.AddTeammateAsync(id, teammateId);

            return Results.Created($"/api/employees/{id}/teammates/{teammateId}", null);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/employees/{id}/teammates/{teammateId} — remove a teammate.
    /// </summary>
    private static async Task<IResult> RemoveTeammateAsync(
        int id,
        int teammateId,
        TeammateService service)
    {
        try
        {
            await service.RemoveTeammateAsync(id, teammateId);

            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Maps an <see cref="Employee"/> entity to an <see cref="EmployeeResponse"/> DTO,
    /// flattening the TalentManager navigation property.
    /// </summary>
    private static EmployeeResponse ToResponse(Employee employee) =>
        new(
            employee.Id,
            employee.FullName,
            employee.Email,
            employee.Role.ToString(),
            employee.TalentManager?.FullName);
}
