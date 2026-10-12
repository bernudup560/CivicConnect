using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CivicConnect.Web.Services;
using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Pages;

public class CreateModel : PageModel
{
    private readonly ITicketApiClient _apiClient;

    public CreateModel(ITicketApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public CreateTicketViewModel Ticket { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        LoadCategories();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadCategories();
            return Page();
        }

        try
        {
            var success = await _apiClient.CreateTicketAsync(Ticket);

            if (success)
            {
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
}