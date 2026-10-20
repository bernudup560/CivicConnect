


using CivicConnect.Api.Contracts;
using CivicConnect.Core.Interfaces;
using CivicConnect.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CivicConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAllTickets(CancellationToken cancellationToken)
    {
        var tickets = await _ticketService.GetAllTicketsAsync(cancellationToken);
        return Ok(tickets.Select(TicketResponse.FromEntity));
    }

    [HttpGet("{ticketId:int}")]
    public async Task<ActionResult<TicketResponse>> GetTicketById(int ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(ticketId, cancellationToken);
        
        if (ticket == null)
        {
            return NotFound(new { message = $"Ticket #{ticketId} was not found." });
        }

        return Ok(TicketResponse.FromEntity(ticket));
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> CreateTicket([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTicketCommand(
            request.Title, 
            request.Description, 
            request.CategoryId,
            request.SubmittedByUserId, 
            request.IsEmergency
        );

        var createdTicket = await _ticketService.CreateTicketAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetTicketById),
            new { ticketId = createdTicket.Id },
            TicketResponse.FromEntity(createdTicket)
        );
    }

    [HttpPatch("{ticketId:int}/status")]
    public async Task<ActionResult<TicketResponse>> ChangeTicketStatus(int ticketId, [FromBody] ChangeTicketStatusRequest request, CancellationToken cancellationToken)
    {
        var updatedTicket = await _ticketService.ChangeTicketStatusAsync(ticketId, request.NewStatus, cancellationToken);
        return Ok(TicketResponse.FromEntity(updatedTicket));
    }
}