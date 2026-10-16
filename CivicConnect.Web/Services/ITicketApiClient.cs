using CivicConnect.Web.ViewModels;

namespace CivicConnect.Web.Services;

public interface ITicketApiClient
{
    Task<IEnumerable<TicketViewModel>> GetTicketsAsync();
    Task<TicketViewModel?> GetTicketByIdAsync(Guid id);
    Task<bool> CreateTicketAsync(CreateTicketViewModel model);
    Task<IEnumerable<TicketViewModel>> GetAllTicketsAsync();
    Task<TicketViewModel> CreateTicketAsync(TicketViewModel ticket);
    Task<bool> UpdateTicketStatusAsync(Guid id, string newStatus);
    Task<bool> UpdateTicketAsync(TicketViewModel updatedTicket);
    Task<bool> DeleteTicketAsync(Guid id);
    Task<bool> UpdateTicketPriorityAsync(Guid id, string newPriority);
}
