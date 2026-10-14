using Microsoft.AspNetCore.Mvc.RazorPages;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;
using System.Security.Claims;

namespace CivicConnect.Web.Pages.Tickets;

public class MyModel : PageModel
{
    private readonly ITicketApiClient _apiClient;

    public MyModel(ITicketApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<TicketViewModel> Tickets { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "user-guid-001";
            var all = (await _apiClient.GetTicketsAsync()).ToList();
            Tickets = all.Where(t => string.Equals(t.CreatedByUserId, userId, StringComparison.OrdinalIgnoreCase)).OrderByDescending(t => t.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load your tickets: {ex.Message}";
        }
    }
}
