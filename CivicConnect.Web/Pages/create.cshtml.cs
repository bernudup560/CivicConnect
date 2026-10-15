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

        if (!ModelState.IsValid)
        {
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
                Title = Ticket.Title,
                Description = Ticket.Description,
                Location = Ticket.Location,
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
            new SelectListItem { Value = "1", Text = "Roads & Potholes" },
            new SelectListItem { Value = "2", Text = "Water & Sanitation" },
            new SelectListItem { Value = "3", Text = "Electricity & Lighting" },
            new SelectListItem { Value = "4", Text = "Parks & Recreation" },
            new SelectListItem { Value = "5", Text = "Waste Management" }
        };
    }

    private static string MapCategoryId(int categoryId) => categoryId switch
    {
        1 => "Roads & Potholes",
        2 => "Water & Sanitation",
        3 => "Electricity & Lighting",
        4 => "Parks & Recreation",
        5 => "Waste Management",
        _ => "General"
    };
}