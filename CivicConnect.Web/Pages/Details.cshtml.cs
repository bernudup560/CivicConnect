using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly ITicketApiClient _apiClient;

        public DetailsModel(ITicketApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public TicketViewModel? Ticket { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Ticket = await _apiClient.GetTicketByIdAsync(id);

                if (Ticket == null)
                {
                    ErrorMessage = $"Ticket #{id} was not found.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to retrieve ticket details: {ex.Message}";
            }

            return Page();
        }
    }
}