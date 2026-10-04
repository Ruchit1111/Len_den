using Microsoft.AspNetCore.Mvc;
using Tricount.DTOs.Requests;
using Tricount.DTOs.Responses;
using Tricount.Services.Interfaces;

namespace Tricount.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TricountsController : ControllerBase
{
    private readonly ITricountService _tricountService;

    public TricountsController(ITricountService tricountService)
    {
        _tricountService = tricountService;
    }

    /// <summary>
    /// Update a tricount (group) and its participants.
    /// Updates: Id, Name, Currency, CreatedByUserId, CreatedAt in Groups, and Id & UserId in Participants.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TricountResponse>> Update(int id, [FromBody] UpdateTricountRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body cannot be null." });
        }

        try
        {
            var updated = await _tricountService.UpdateTricountAsync(id, request);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update a tricount using the ID provided in the request body.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<TricountResponse>> UpdateFromBody([FromBody] UpdateTricountRequest request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest(new { message = "A valid 'id' must be provided in the request body." });
        }

        return await Update(request.Id, request);
    }

    /// <summary>
    /// Create a new tricount with initial participants.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TricountResponse>> Create([FromBody] UpdateTricountRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body cannot be null." });
        }

        var created = await _tricountService.CreateTricountAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Get tricount by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TricountResponse>> GetById(int id)
    {
        var result = await _tricountService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = $"Tricount with ID {id} not found." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all tricounts.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TricountResponse>>> GetAll()
    {
        var result = await _tricountService.GetAllAsync();
        return Ok(result);
    }
}
