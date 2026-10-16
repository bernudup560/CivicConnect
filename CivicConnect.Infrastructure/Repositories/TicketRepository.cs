using Microsoft.EntityFrameworkCore;
using CivicConnect.Core.Entities;
using CivicConnect.Core.Interfaces;
using CivicConnect.Infrastructure.Data;

namespace CivicConnect.Infrastructure.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    private readonly CivicConnectDbContext _context;

    public TicketRepository(CivicConnectDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Ticket>> GetTicketsWithCategoryAsync()
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketWithDetailsAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}