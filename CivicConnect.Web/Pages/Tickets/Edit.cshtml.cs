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
            if (Ticket == null || Ticket.Id == Guid.Empty)
            {
                ErrorMessage = "Invalid ticket data.";
                return Page();
            }

            // Fetch the existing ticket from the API to validate ownership and preserve fields
            var existing = await _apiClient.GetTicketByIdAsync(Ticket.Id);
            if (existing == null)
            {
                return NotFound();
            }

            // Only the original owner may update
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (existing.CreatedByUserId != userId)
                return Forbid();

            // Apply editable fields from the form to the stored ticket model
            existing.Title = Ticket.Title;
            existing.Description = Ticket.Description;
            existing.CategoryName = Ticket.CategoryName;
            existing.Location = Ticket.Location;

            var success = await _apiClient.UpdateTicketAsync(existing);
            if (!success)
            {
                ErrorMessage = "Failed to update the ticket.";
                return Page();
            }

            return RedirectToPage("/Details", new { id = existing.Id });
        }
    }
}
