using Microsoft.EntityFrameworkCore;
using Tricount.Data;
using Tricount.Models;
using Tricount.Repositories.Interfaces;

namespace Tricount.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly TricountDbContext _context;

    public ExpenseRepository(TricountDbContext context)
    {
        _context = context;
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Expense?> GetWithDetailsAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Group)
                .ThenInclude(g => g!.Members)
                    .ThenInclude(m => m.User)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Expense>> GetAllAsync()
    {
        return await _context.Expenses
            .Include(e => e.Participants)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByGroupIdAsync(int groupId)
    {
        return await _context.Expenses
            .Include(e => e.Participants)
            .Where(e => e.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<Expense> CreateAsync(Expense expense)
    {
        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public Task UpdateAsync(Expense expense)
    {
        _context.Expenses.Update(expense);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Expense expense)
    {
        _context.Expenses.Remove(expense);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
