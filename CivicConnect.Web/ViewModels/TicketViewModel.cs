namespace CivicConnect.Web.ViewModels;

public class TicketViewModel
{
    public Guid Id { get; set; }
    // Human-friendly ticket number, assigned when ticket is created (e.g. "CC-00001")
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CalculatedPriority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
}
