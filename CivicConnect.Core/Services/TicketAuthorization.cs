namespace CivicConnect.Core.Services;

public static class TicketAuthorization
{
    public static bool CanDelete(string? ticketOwnerId, string? currentUserId) =>
        !string.IsNullOrWhiteSpace(ticketOwnerId)
        && !string.IsNullOrWhiteSpace(currentUserId)
        && ticketOwnerId == currentUserId;
}