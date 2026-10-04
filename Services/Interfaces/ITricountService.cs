using Tricount.DTOs.Requests;
using Tricount.DTOs.Responses;

namespace Tricount.Services.Interfaces;

public interface ITricountService
{
    Task<TricountResponse?> GetByIdAsync(int id);
    Task<IEnumerable<TricountResponse>> GetAllAsync();
    Task<TricountResponse> CreateTricountAsync(UpdateTricountRequest request);
    Task<TricountResponse> UpdateTricountAsync(int id, UpdateTricountRequest request);
}
