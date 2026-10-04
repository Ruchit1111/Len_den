using Tricount.Models;

namespace Tricount.Repositories.Interfaces;

public interface ITricountRepository
{
    Task<Group?> GetByIdAsync(int id);
    Task<IEnumerable<Group>> GetAllAsync();
    Task<Group> CreateAsync(Group group);
    Task UpdateAsync(Group group);
    Task SaveChangesAsync();
}
