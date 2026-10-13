using Microsoft.AspNetCore.Mvc.RazorPages;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Pages.Tickets
{
    public class ClosedModel : PageModel
    {
        private readonly ITicketApiClient _apiClient;

        public ClosedModel(ITicketApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public List<TicketViewModel> Tickets { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var all = await _apiClient.GetAllTicketsAsync();

                // Only treat 'Closed' as closed; 'Resolved' is not the same
                Tickets = all.Where(t => string.Equals(t.Status, "Closed", StringComparison.OrdinalIgnoreCase))
                             .OrderByDescending(t => t.CreatedAt)
                             .ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load closed tickets: {ex.Message}";
            }
        }
    }
}
