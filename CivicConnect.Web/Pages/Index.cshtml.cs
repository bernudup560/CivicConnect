using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CivicConnect.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ITicketApiClient _apiClient;
    private readonly IWebHostEnvironment _env;

    public IndexModel(ITicketApiClient apiClient, IWebHostEnvironment env)
    {
        _apiClient = apiClient;
        _env = env;
    }

    public IEnumerable<TicketViewModel> Tickets { get; set; } = Enumerable.Empty<TicketViewModel>();
    public string? ErrorMessage { get; set; }
    public bool HasAdminHeroImage { get; set; }
    public bool HasUserHeroImage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchKeyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedCategory { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SortDir { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var items = (await _apiClient.GetTicketsAsync()).AsEnumerable();

            // Apply filters/search
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var q = SearchKeyword.Trim();
                items = items.Where(t =>
                    (!string.IsNullOrWhiteSpace(t.Title) && t.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(t.Location) && t.Location.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(t.TicketNumber) && t.TicketNumber.Contains(q, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrWhiteSpace(SelectedCategory))
            {
                var cat = SelectedCategory.Trim();
                items = items.Where(t => string.Equals(t.CategoryName, cat, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SelectedStatus))
            {
                var st = SelectedStatus.Trim();
                items = items.Where(t => string.Equals(t.Status, st, StringComparison.OrdinalIgnoreCase));
            }

            // Default sort by created date descending unless requested otherwise
            if (!string.IsNullOrWhiteSpace(SortBy) && string.Equals(SortBy, "date", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(SortDir, "asc", StringComparison.OrdinalIgnoreCase))
                    Tickets = items.OrderBy(t => t.CreatedAt).ToList();
                else
                    Tickets = items.OrderByDescending(t => t.CreatedAt).ToList();
            }
            else
            {
                Tickets = items.OrderByDescending(t => t.CreatedAt).ToList();
            }

            var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            // Check both images/ and lib/ locations for the provided hero image
            // Admin hero image file name (preferred)
            var adminImage1 = Path.Combine(wwwroot, "images", "marlis-trio-akbar-eMB60hNHFL8-unsplash.jpg");
            var adminImage2 = Path.Combine(wwwroot, "lib", "marlis-trio-akbar-eMB60hNHFL8-unsplash.jpg");
            HasAdminHeroImage = System.IO.File.Exists(adminImage1) || System.IO.File.Exists(adminImage2);

            // User-facing hero image file name
            var userImage1 = Path.Combine(wwwroot, "images", "pexels-ferdous-9225479.jpg");
            var userImage2 = Path.Combine(wwwroot, "lib", "pexels-ferdous-9225479.jpg");
            HasUserHeroImage = System.IO.File.Exists(userImage1) || System.IO.File.Exists(userImage2);
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load tickets from the CivicConnect service.";
            HasAdminHeroImage = false;
        }
    }
}