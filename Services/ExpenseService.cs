using System.Text.Json;
using Tricount.DTOs.Requests;
using Tricount.DTOs.Responses;
using Tricount.Models;
using Tricount.Repositories.Interfaces;
using Tricount.Services.Interfaces;

namespace Tricount.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repository;
    private readonly ITricountRepository _tricountRepository;

    public ExpenseService(IExpenseRepository repository, ITricountRepository tricountRepository)
    {
        _repository = repository;
        _tricountRepository = tricountRepository;
    }

    public async Task<ExpenseResponse?> GetByIdAsync(int id)
    {
        var expense = await _repository.GetByIdAsync(id);
        return expense == null ? null : MapToResponse(expense);
    }

    public async Task<IEnumerable<ExpenseResponse>> GetAllAsync()
    {
        var expenses = await _repository.GetAllAsync();
        return expenses.Select(MapToResponse);
    }

    public async Task<IEnumerable<ExpenseResponse>> GetByGroupIdAsync(int groupId)
    {
        var expenses = await _repository.GetByGroupIdAsync(groupId);
        return expenses.Select(MapToResponse);
    }

    public async Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request)
    {
        var expense = new Expense
        {
            GroupId = request.GroupId,
            Title = request.Title,
            Amount = request.Amount,
            Category = request.Category,
            PaidByUserId = request.PaidByUserId,
            CreatedAt = request.CreatedAt ?? DateTime.UtcNow
        };

        var participantIds = GetParticipantIds(request.Participants, request.AdditionalData);

        List<int> targetUserIds;
        if (participantIds.Count > 0)
        {
            targetUserIds = participantIds.Distinct().ToList();
        }
        else
        {
            // Default: automatically split equally among all members of the group
            var group = await _tricountRepository.GetByIdAsync(request.GroupId);
            targetUserIds = group?.Members.Select(m => m.UserId).Distinct().ToList() ?? new List<int>();
        }

        // Divide the total amount equally among the participants (e.g. 100 / 2 = 50 each)
        var splits = CalculateEqualSplit(request.Amount, targetUserIds);

        foreach (var split in splits)
        {
            expense.Participants.Add(new ExpenseParticipant
            {
                UserId = split.UserId,
                AmountOwed = split.AmountOwed
            });
        }

        var created = await _repository.CreateAsync(expense);
        return MapToResponse(created);
    }

    public async Task<ExpenseResponse> UpdateAsync(int id, UpdateExpenseRequest request)
    {
        var expense = await _repository.GetByIdAsync(id);
        if (expense == null)
        {
            throw new KeyNotFoundException($"Expense with ID {id} was not found.");
        }

        // Update Expenses table columns
        expense.GroupId = request.GroupId;
        expense.Title = request.Title;
        expense.Amount = request.Amount;
        expense.Category = request.Category;
        expense.PaidByUserId = request.PaidByUserId;

        if (request.CreatedAt.HasValue)
        {
            expense.CreatedAt = request.CreatedAt.Value;
        }

        var participantIds = GetParticipantIds(request.Participants, request.AdditionalData);

        if (participantIds.Count > 0)
        {
            var targetUserIds = participantIds.Distinct().ToList();
            var splits = CalculateEqualSplit(request.Amount, targetUserIds);

            expense.Participants.Clear();
            foreach (var split in splits)
            {
                expense.Participants.Add(new ExpenseParticipant
                {
                    ExpenseId = expense.Id,
                    UserId = split.UserId,
                    AmountOwed = split.AmountOwed
                });
            }
        }
        else if (expense.Amount != request.Amount && expense.Participants.Count > 0)
        {
            // If total amount was updated and participants were not specified, re-split existing participants
            var targetUserIds = expense.Participants.Select(p => p.UserId).ToList();
            var splits = CalculateEqualSplit(request.Amount, targetUserIds);

            expense.Participants.Clear();
            foreach (var split in splits)
            {
                expense.Participants.Add(new ExpenseParticipant
                {
                    ExpenseId = expense.Id,
                    UserId = split.UserId,
                    AmountOwed = split.AmountOwed
                });
            }
        }

        await _repository.UpdateAsync(expense);
        await _repository.SaveChangesAsync();

        return MapToResponse(expense);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var expense = await _repository.GetByIdAsync(id);
        if (expense == null)
        {
            return false;
        }

        await _repository.DeleteAsync(expense);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static List<int> GetParticipantIds(List<int>? participants, Dictionary<string, object>? additionalData)
    {
        if (participants != null && participants.Count > 0)
        {
            return participants;
        }

        if (additionalData != null)
        {
            foreach (var key in new[] { "participantUserIds", "ParticipantUserIds" })
            {
                if (additionalData.TryGetValue(key, out var val) && val is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    var ids = new List<int>();
                    foreach (var item in element.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out int id))
                        {
                            ids.Add(id);
                        }
                    }
                    if (ids.Count > 0)
                    {
                        return ids;
                    }
                }
            }
        }

        return new List<int>();
    }

    private static List<(int UserId, decimal AmountOwed)> CalculateEqualSplit(decimal totalAmount, List<int> userIds)
    {
        if (userIds == null || userIds.Count == 0)
        {
            return new List<(int UserId, decimal AmountOwed)>();
        }

        int count = userIds.Count;
        decimal baseAmount = Math.Floor((totalAmount / count) * 100m) / 100m;
        int remainderCents = (int)Math.Round((totalAmount - (baseAmount * count)) * 100m);

        var splits = new List<(int UserId, decimal AmountOwed)>();
        for (int i = 0; i < count; i++)
        {
            decimal share = baseAmount + (i < remainderCents ? 0.01m : 0.00m);
            splits.Add((userIds[i], share));
        }

        return splits;
    }

    private static ExpenseResponse MapToResponse(Expense expense)
    {
        return new ExpenseResponse
        {
            Id = expense.Id,
            GroupId = expense.GroupId,
            Title = expense.Title,
            Amount = expense.Amount,
            Category = expense.Category,
            PaidByUserId = expense.PaidByUserId,
            CreatedAt = expense.CreatedAt,
            Participants = expense.Participants.Select(p => new ExpenseParticipantResponseDto
            {
                Id = p.Id,
                UserId = p.UserId,
                AmountOwed = p.AmountOwed
            }).ToList()
        };
    }
}
