using Microsoft.AspNetCore.Mvc;
using Tricount.DTOs.Requests;
using Tricount.DTOs.Responses;
using Tricount.Services.Interfaces;

namespace Tricount.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    /// <summary>
    /// Create a new expense.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> Create([FromBody] CreateExpenseRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body cannot be null." });
        }

        var created = await _expenseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing expense by route ID.
    /// Updates: GroupId, Title, Amount, Category, PaidByUserId, and CreatedAt in the Expenses table.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> Update(int id, [FromBody] UpdateExpenseRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body cannot be null." });
        }

        try
        {
            var updated = await _expenseService.UpdateAsync(id, request);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing expense using ID in the request body.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<ExpenseResponse>> UpdateFromBody([FromBody] UpdateExpenseRequest request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest(new { message = "A valid 'id' must be provided in the request body." });
        }

        return await Update(request.Id, request);
    }

    /// <summary>
    /// Get an expense by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> GetById(int id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        if (expense == null)
        {
            return NotFound(new { message = $"Expense with ID {id} not found." });
        }

        return Ok(expense);
    }

    /// <summary>
    /// Get all expenses.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponse>>> GetAll()
    {
        var expenses = await _expenseService.GetAllAsync();
        return Ok(expenses);
    }

    /// <summary>
    /// Get all expenses for a specific group.
    /// </summary>
    [HttpGet("group/{groupId:int}")]
    public async Task<ActionResult<IEnumerable<ExpenseResponse>>> GetByGroupId(int groupId)
    {
        var expenses = await _expenseService.GetByGroupIdAsync(groupId);
        return Ok(expenses);
    }


    /// <summary>
    /// Delete an expense by ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _expenseService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Expense with ID {id} not found." });
        }

        return NoContent();
    }
}
