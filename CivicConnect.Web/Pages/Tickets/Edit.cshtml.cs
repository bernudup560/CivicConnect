using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Pages.Tickets
{
    public class EditModel : PageModel
    {
        private readonly ITicketApiClient _apiClient;

        public EditModel(ITicketApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public TicketViewModel? Ticket { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var ticket = await _apiClient.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            // Only owner can edit (admins cannot edit via this page)
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (ticket.CreatedByUserId != userId)
                return Forbid();

            Ticket = ticket;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Ticket == null)
            {
                ErrorMessage = "Invalid ticket data.";
                return Page();
            }

            // Only owner can update
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Ticket.CreatedByUserId != userId)
                return Forbid();

            var success = await _apiClient.UpdateTicketAsync(Ticket);
            if (!success)
            {
                ErrorMessage = "Failed to update the ticket.";
                return Page();
            }

            return RedirectToPage("/Details", new { id = Ticket.Id });
        }
    }
}
