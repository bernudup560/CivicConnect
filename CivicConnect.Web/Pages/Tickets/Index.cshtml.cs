using Microsoft.AspNetCore.Mvc.RazorPages;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Pages.Tickets
{
    public class IndexModel : PageModel
    {
        private readonly ITicketApiClient _apiClient;

        public IndexModel(ITicketApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Bound properties for the search/filter form (supports GET so form uses query string)
        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public string? SelectedCategory { get; set; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public string? SelectedStatus { get; set; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public string? SortDir { get; set; }

        public List<TicketViewModel> Tickets { get; set; } = new();
        public int TotalCount { get; set; }
        public int OpenCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var result = await _apiClient.GetTicketsAsync();

                // Minimal filtering happens here so the Razor page can render and the properties exist for binding.
                Tickets = result?.ToList() ?? new List<TicketViewModel>();

                // By default, hide tickets that are 'Closed' from the main dashboard.
                if (string.IsNullOrWhiteSpace(SelectedStatus))
                {
                    Tickets = Tickets.Where(t => !string.Equals(t.Status, "Closed", StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Compute stats
                var allTickets = result?.ToList() ?? new List<TicketViewModel>();
                TotalCount = allTickets.Count;
                OpenCount = allTickets.Count(t => string.Equals(t.Status, "Submitted", StringComparison.OrdinalIgnoreCase));
                InProgressCount = allTickets.Count(t => string.Equals(t.Status, "In Progress", StringComparison.OrdinalIgnoreCase));
                ResolvedCount = allTickets.Count(t => string.Equals(t.Status, "Resolved", StringComparison.OrdinalIgnoreCase));
                ClosedCount = allTickets.Count(t => string.Equals(t.Status, "Closed", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    Tickets = Tickets.Where(t => t.Title.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase)
                                                || t.Location.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(SelectedCategory))
                {
                    Tickets = Tickets.Where(t => string.Equals(t.CategoryName, SelectedCategory, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(SelectedStatus))
                {
                    Tickets = Tickets.Where(t => string.Equals(t.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Sorting
                if (!string.IsNullOrWhiteSpace(SortBy) && string.Equals(SortBy, "date", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(SortDir, "asc", StringComparison.OrdinalIgnoreCase))
                        Tickets = Tickets.OrderBy(t => t.CreatedAt).ToList();
                    else
                        Tickets = Tickets.OrderByDescending(t => t.CreatedAt).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load tickets from the CivicConnect service: {ex.Message}";
            }
        }
    }
}