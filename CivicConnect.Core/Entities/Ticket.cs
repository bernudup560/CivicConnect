using CivicConnect.Core.Exceptions;

namespace CivicConnect.Core.Entities;

public enum TicketStatus { Open, InProgress, Resolved, Closed }

public enum TicketPriority { Low, Medium, High, Critical }

public class Ticket
{
    // The state machine: for each status, which statuses may come next
    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedStatusTransitions = new()
    {
        [TicketStatus.Open] = new[] { TicketStatus.InProgress, TicketStatus.Closed },
        [TicketStatus.InProgress] = new[] { TicketStatus.Resolved, TicketStatus.Open },
        [TicketStatus.Resolved] = new[] { TicketStatus.Closed, TicketStatus.InProgress },
        [TicketStatus.Closed] = Array.Empty<TicketStatus>()
    };

    private Ticket() { }

    public Ticket(string title, string description, int categoryId, int submittedByUserId, bool isEmergency)
    {
        
        if (string.IsNullOrWhiteSpace(title))
        throw new DomainRuleViolationException("A ticket must have a title.");
    if (!title.Any(char.IsLetter))
        throw new DomainRuleViolationException("The ticket title must contain letters, not just numbers.");

    if (string.IsNullOrWhiteSpace(description))
        throw new DomainRuleViolationException("A ticket must have a description.");
    if (!description.Any(char.IsLetter))
        throw new DomainRuleViolationException("The ticket description must contain letters, not just numbers.");

        Title = title.Trim();
        Description = description.Trim();
        CategoryId = categoryId;
        SubmittedByUserId = submittedByUserId;
        IsEmergency = isEmergency;
        Status = TicketStatus.Open;
        Priority = TicketPriority.Low;
        CreatedAtUtc = DateTime.UtcNow;
        LastUpdatedAtUtc = CreatedAtUtc;
    }

    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public int SubmittedByUserId { get; private set; }
    public User? SubmittedBy { get; private set; }
    public bool IsEmergency { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastUpdatedAtUtc { get; private set; }

    // Called by the service after a priority strategy has decided the priority
    public void AssignPriority(TicketPriority newPriority)
    {
        Priority = newPriority;
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    // Moves the ticket to a new status, but only if the state machine allows it
    public void TransitionTo(TicketStatus newStatus)
    {
        if (!AllowedStatusTransitions[Status].Contains(newStatus))
            throw new DomainRuleViolationException(
                $"A ticket cannot move from {Status} to {newStatus}.");

        Status = newStatus;
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

     public bool IsOverdue(DateTime nowUtc) =>
        (Status == TicketStatus.Open || Status == TicketStatus.InProgress)
        && (nowUtc - CreatedAtUtc).TotalHours > 48;
    
}
