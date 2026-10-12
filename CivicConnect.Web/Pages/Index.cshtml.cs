using Microsoft.AspNetCore.Mvc.RazorPages;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ITicketApiClient _apiClient;

    public IndexModel(ITicketApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IEnumerable<TicketViewModel> Tickets { get; set; } = Enumerable.Empty<TicketViewModel>();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Tickets = await _apiClient.GetTicketsAsync();
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load tickets from the CivicConnect service.";
        }
    }
}