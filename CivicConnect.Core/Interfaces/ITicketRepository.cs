using CivicConnect.Core.Entities;

namespace CivicConnect.Core.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetTicketsWithCategoryAsync();
    Task<Ticket?> GetTicketWithDetailsAsync(int id);
}