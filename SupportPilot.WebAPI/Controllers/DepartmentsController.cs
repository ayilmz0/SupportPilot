using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportPilot.Application.Features.Departments.GetAll;
using SupportPilot.Application.Features.Departments.Update;
using SupportPilot.Application.Features.Departments.Deactivate;

namespace SupportPilot.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DepartmentsController : ControllerBase
{
    private readonly DepartmentService _departmentService;
    private readonly UpdateDepartmentService _updateDepartmentService;
    private readonly DeactivateDepartmentService _deactivateDepartmentService;

    public DepartmentsController(
        DepartmentService departmentService,
        UpdateDepartmentService updateDepartmentService,
        DeactivateDepartmentService deactivateDepartmentService)
    {
        _departmentService = departmentService;
        _updateDepartmentService = updateDepartmentService;
        _deactivateDepartmentService = deactivateDepartmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var departments = await _departmentService.ExecuteAsync(
            cancellationToken);

        return Ok(departments);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateDepartmentService.ExecuteAsync(
            id,
            request.Name,
            request.Description,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == "Department not found.")
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return Ok(new
        {
            message = "Department updated successfully."
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _deactivateDepartmentService.ExecuteAsync(
            id,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == "Department not found.")
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return Ok(new
        {
            message = "Department deactivated successfully."
        });
    }
}