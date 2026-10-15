using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CivicConnect.Web.ViewModels;
using CivicConnect.Web.Services;

namespace CivicConnect.Web.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly ITicketApiClient _apiClient;

        public DetailsModel(ITicketApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> OnPostChangePriorityAsync(Guid id, string newPriority)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var success = await _apiClient.UpdateTicketPriorityAsync(id, newPriority);
            if (success)
            {
                Message = $"Priority successfully updated to '{newPriority}'.";
            }
            else
            {
                Message = "Failed to update priority.";
            }

            return await OnGetAsync(id);
        }

        public TicketViewModel? Ticket { get; set; }
        public string? Message { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Ticket = await _apiClient.GetTicketByIdAsync(id);
            if (Ticket == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string newStatus)
        {
            // Only admins may update ticket status
            if (!User.IsInRole("Admin"))
                return Forbid();

            var success = await _apiClient.UpdateTicketStatusAsync(id, newStatus);
            if (success)
            {
                Message = $"Status successfully updated to '{newStatus}'.";
            }
            else
            {
                Message = "Failed to update status.";
            }

            return await OnGetAsync(id);
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var ticket = await _apiClient.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Only the original owner may delete their report (admins cannot delete)
            if (ticket.CreatedByUserId != currentUserId)
                return Forbid();

            var success = await _apiClient.DeleteTicketAsync(id);
            if (!success)
            {
                Message = "Failed to delete the ticket.";
                return await OnGetAsync(id);
            }

            return RedirectToPage("/Tickets/Index");
        }
    }
}
