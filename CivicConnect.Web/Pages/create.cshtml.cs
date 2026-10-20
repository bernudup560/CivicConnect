using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Pages;

public class CreateModel : PageModel
{
    private readonly ITicketApiClient _apiClient;

    [TempData]
    public string? CreatedTicketNumber { get; set; }

    public CreateModel(ITicketApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public CreateTicketViewModel Ticket { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public Microsoft.AspNetCore.Mvc.IActionResult OnGet()
    {
        // Require authentication to access the create page
        if (!User?.Identity?.IsAuthenticated ?? true)
        {
            // Redirect to login page
            return RedirectToPage("/Account/Login", new { ReturnUrl = "/Create" });
        }

        LoadCategories();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User?.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Account/Login", new { ReturnUrl = "/Create" });
        }

        // Block pure numbers or symbols in Title, Location, and Description
        if (!string.IsNullOrWhiteSpace(Ticket.Title) && !Ticket.Title.Any(char.IsLetter))
        {
            ModelState.AddModelError("Ticket.Title", "Issue Title must contain descriptive text, not just numbers.");
        }

        if (!string.IsNullOrWhiteSpace(Ticket.Location) && !Ticket.Location.Any(char.IsLetter))
        {
            ModelState.AddModelError("Ticket.Location", "Location must include a street or suburb name, not just numbers.");
        }

        if (!string.IsNullOrWhiteSpace(Ticket.Description) && !Ticket.Description.Any(char.IsLetter))
        {
            ModelState.AddModelError("Ticket.Description", "Description must contain descriptive text, not just numbers.");
        }

        if (!ModelState.IsValid)
        {
            LoadCategories();
            return Page();
        }

        // Guard: ensure the three text fields are present (not null/empty),
        // so the .Trim() calls below are provably safe.
        if (string.IsNullOrWhiteSpace(Ticket.Title) ||
            string.IsNullOrWhiteSpace(Ticket.Location) ||
            string.IsNullOrWhiteSpace(Ticket.Description))
        {
            ModelState.AddModelError(string.Empty, "Title, location and description are required.");
            LoadCategories();
            return Page();
        }

        try
        {
            // Map form model to view model and set the creating user from the current claim
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var ticketVm = new TicketViewModel
            {
                Id = Guid.NewGuid(),
                Title = (Ticket.Title ?? string.Empty).Trim(),
                Description = (Ticket.Description ?? string.Empty).Trim(),
                Location = (Ticket.Location ?? string.Empty).Trim(),
                CategoryName = MapCategoryId(Ticket.CategoryId),
                CalculatedPriority = Ticket.IsEmergency ? "Emergency" : "Low",
                Status = "Submitted",
                CreatedAt = DateTime.Now,
                CreatedByUserId = userId ?? "user-guid-001"
            };

            var created = await _apiClient.CreateTicketAsync(ticketVm);
            if (created != null)
            {
                // Store the generated ticket number in TempData so Index page can show a popup
                CreatedTicketNumber = created.TicketNumber;
                return RedirectToPage("/Index");
            }

            ErrorMessage = "Failed to submit ticket. Please verify your inputs and try again.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"API Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }

        LoadCategories();
        return Page();
    }

    private void LoadCategories()
    {
        CategoryOptions = new List<SelectListItem>
        {
        new SelectListItem { Value = "1", Text = "Water and sanitation" },
        new SelectListItem { Value = "2", Text = "Roads and potholes" },
        new SelectListItem { Value = "3", Text = "Street lighting" },
        new SelectListItem { Value = "4", Text = "Waste collection" },
        new SelectListItem { Value = "5", Text = "Parks and recreation" }
        };
    }

    private static string MapCategoryId(int categoryId) => categoryId switch
    {
        1 => "Water and sanitation",
    2 => "Roads and potholes",
    3 => "Street lighting",
    4 => "Waste collection",
    5 => "Parks and recreation",
    _ => "General"
    };
}