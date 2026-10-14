using System.Collections.Concurrent;
using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Services;

public class TicketApiClient : ITicketApiClient
{
    private readonly ConcurrentDictionary<Guid, TicketViewModel> _store = new();
    private int _ticketCounter = 0;

    // Demo user id used by the login page
    private const string DemoUserId = "user-guid-001";

    public TicketApiClient()
    {
        // Seed sample tickets
        var t1 = new TicketViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Pothole on Main St",
            Description = "Large pothole outside 12 Main St causing vehicle damage.",
            Location = "12 Main St",
            CategoryName = "Roads & Potholes",
            CalculatedPriority = "High",
            Status = "Submitted",
            CreatedAt = DateTime.Now.AddDays(-2),
            CreatedByUserId = DemoUserId
        };

        var t2 = new TicketViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Water Leak on 4th Ave",
            Description = "Burst pipe causing flooding near the intersection of 4th Ave and Pine.",
            Location = "4th Ave & Pine",
            CategoryName = "Water & Sanitation",
            CalculatedPriority = "Emergency",
            Status = "In Progress",
            CreatedAt = DateTime.Now.AddDays(-5),
            CreatedByUserId = DemoUserId
        };

        var t3 = new TicketViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Streetlight failure at Oak Ave",
            Description = "Streetlight #45 flickers and goes out after sunset.",
            Location = "Oak Ave near 7",
            CategoryName = "Electricity & Lighting",
            CalculatedPriority = "Low",
            Status = "Resolved",
            CreatedAt = DateTime.Now.AddDays(-10),
            CreatedByUserId = DemoUserId
        };

        var t4 = new TicketViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Illegal dumping behind community center",
            Description = "Large piles of household waste dumped behind the community center.",
            Location = "Community Center rear",
            CategoryName = "Waste & Dumping",
            CalculatedPriority = "Medium",
            Status = "Submitted",
            CreatedAt = DateTime.Now.AddDays(-1),
            CreatedByUserId = DemoUserId
        };

        var t5 = new TicketViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Broken traffic light at 3rd & Elm",
            Description = "Traffic signal stuck on red causing congestion.",
            Location = "3rd & Elm",
            CategoryName = "Roads & Potholes",
            CalculatedPriority = "High",
            Status = "In Progress",
            CreatedAt = DateTime.Now.AddHours(-6),
            CreatedByUserId = DemoUserId
        };

        foreach (var t in new[] { t1, t2, t3, t4, t5 })
        {
            _store[t.Id] = t;
        }

        // Initialize counter based on seeded items (so new tickets get increasing numbers)
        _ticketCounter = _store.Count;
    }

    // Legacy compatibility methods
    public Task<IEnumerable<TicketViewModel>> GetTicketsAsync()
        => GetAllTicketsAsync();

    public Task<TicketViewModel?> GetTicketByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var ticket);
        return Task.FromResult(ticket);
    }

    public Task<bool> CreateTicketAsync(CreateTicketViewModel model)
    {
        var ticket = new TicketViewModel
        {
            Id = Guid.NewGuid(),
            TicketNumber = GenerateTicketNumber(),
            Title = model.Title,
            Description = model.Description,
            Location = model.Location,
            CategoryName = MapCategoryId(model.CategoryId),
            CalculatedPriority = model.IsEmergency ? "Emergency" : "Low",
            Status = "Submitted",
            CreatedAt = DateTime.Now,
            CreatedByUserId = DemoUserId
        };

        _store[ticket.Id] = ticket;
        return Task.FromResult(true);
    }

    // New in-memory CRUD methods
    public Task<IEnumerable<TicketViewModel>> GetAllTicketsAsync()
    {
        var items = _store.Values.OrderByDescending(t => t.CreatedAt).AsEnumerable();
        return Task.FromResult(items);
    }

    public Task<TicketViewModel> CreateTicketAsync(TicketViewModel ticket)
    {
        if (ticket.Id == Guid.Empty) ticket.Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(ticket.TicketNumber)) ticket.TicketNumber = GenerateTicketNumber();
        if (ticket.CreatedAt == default) ticket.CreatedAt = DateTime.Now;
        if (string.IsNullOrWhiteSpace(ticket.Status)) ticket.Status = "Submitted";
        if (string.IsNullOrWhiteSpace(ticket.CreatedByUserId)) ticket.CreatedByUserId = DemoUserId;

        _store[ticket.Id] = ticket;
        return Task.FromResult(ticket);
    }

    public Task<bool> UpdateTicketStatusAsync(Guid id, string newStatus)
    {
        if (_store.TryGetValue(id, out var ticket))
        {
            ticket.Status = newStatus;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> UpdateTicketAsync(TicketViewModel updatedTicket)
    {
        if (updatedTicket == null || updatedTicket.Id == Guid.Empty)
            return Task.FromResult(false);

        if (_store.TryGetValue(updatedTicket.Id, out var ticket))
        {
            ticket.Title = updatedTicket.Title;
            ticket.Description = updatedTicket.Description;
            ticket.CategoryName = updatedTicket.CategoryName;
            ticket.Location = updatedTicket.Location;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> DeleteTicketAsync(Guid id)
    {
        var removed = _store.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    private static string MapCategoryId(int categoryId) => categoryId switch
    {
        1 => "Roads & Potholes",
        2 => "Water & Sanitation",
        3 => "Electricity & Lighting",
        _ => "General"
    };

    private string GenerateTicketNumber()
    {
        // Simple sequential ticket numbers with prefix. Thread-safe increment.
        var next = System.Threading.Interlocked.Increment(ref _ticketCounter);
        return $"CC-{next:D5}"; // CC-00001, CC-00002, ...
    }
}
