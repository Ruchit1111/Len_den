using Microsoft.EntityFrameworkCore;
using Tricount.Data;
using Tricount.Models;
using Tricount.Repositories.Interfaces;

namespace Tricount.Repositories;

public class TricountRepository : ITricountRepository
{
    private readonly TricountDbContext _context;

    public TricountRepository(TricountDbContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Group>> GetAllAsync()
    {
        return await _context.Groups
            .Include(g => g.Members)
            .ToListAsync();
    }

    public async Task<Group> CreateAsync(Group group)
    {
        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public Task UpdateAsync(Group group)
    {
        _context.Groups.Update(group);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
