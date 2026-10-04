using Tricount.DTOs.Requests;
using Tricount.DTOs.Responses;
using Tricount.Models;
using Tricount.Repositories.Interfaces;
using Tricount.Services.Interfaces;

namespace Tricount.Services;

public class TricountService : ITricountService
{
    private readonly ITricountRepository _repository;

    public TricountService(ITricountRepository repository)
    {
        _repository = repository;
    }

    public async Task<TricountResponse?> GetByIdAsync(int id)
    {
        var group = await _repository.GetByIdAsync(id);
        return group == null ? null : MapToResponse(group);
    }

    public async Task<IEnumerable<TricountResponse>> GetAllAsync()
    {
        var groups = await _repository.GetAllAsync();
        return groups.Select(MapToResponse);
    }

    public async Task<TricountResponse> CreateTricountAsync(UpdateTricountRequest request)
    {
        var group = new Group
        {
            Name = request.Name,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency,
            CreatedByUserId = request.CreatedByUserId,
            CreatedAt = request.CreatedAt ?? DateTime.UtcNow
        };

        if (request.Participants != null && request.Participants.Count > 0)
        {
            foreach (var participant in request.Participants)
            {
                group.Members.Add(new GroupMember
                {
                    UserId = participant.UserId,
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        var created = await _repository.CreateAsync(group);
        return MapToResponse(created);
    }

    public async Task<TricountResponse> UpdateTricountAsync(int id, UpdateTricountRequest request)
    {
        var group = await _repository.GetByIdAsync(id);
        if (group == null)
        {
            throw new KeyNotFoundException($"Tricount with ID {id} was not found.");
        }

        // Update group properties
        group.Name = request.Name;
        group.Currency = string.IsNullOrWhiteSpace(request.Currency) ? group.Currency : request.Currency;
        group.CreatedByUserId = request.CreatedByUserId;

        if (request.CreatedAt.HasValue)
        {
            group.CreatedAt = request.CreatedAt.Value;
        }

        // Update participants (GroupMembers)
        if (request.Participants != null)
        {
            var incomingIds = request.Participants
                .Where(p => p.Id.HasValue && p.Id.Value > 0)
                .Select(p => p.Id!.Value)
                .ToHashSet();

            // Remove participants that are no longer in the request
            var membersToRemove = group.Members
                .Where(m => m.Id > 0 && !incomingIds.Contains(m.Id))
                .ToList();

            foreach (var member in membersToRemove)
            {
                group.Members.Remove(member);
            }

            // Update existing or add new participants
            foreach (var participantDto in request.Participants)
            {
                if (participantDto.Id.HasValue && participantDto.Id.Value > 0)
                {
                    var existingMember = group.Members.FirstOrDefault(m => m.Id == participantDto.Id.Value);
                    if (existingMember != null)
                    {
                        // Update UserId of existing participant
                        existingMember.UserId = participantDto.UserId;
                    }
                    else
                    {
                        // Add participant with specified Id and UserId
                        group.Members.Add(new GroupMember
                        {
                            Id = participantDto.Id.Value,
                            GroupId = group.Id,
                            UserId = participantDto.UserId,
                            JoinedAt = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    // Add new participant
                    group.Members.Add(new GroupMember
                    {
                        GroupId = group.Id,
                        UserId = participantDto.UserId,
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await _repository.UpdateAsync(group);
        await _repository.SaveChangesAsync();

        return MapToResponse(group);
    }

    private static TricountResponse MapToResponse(Group group)
    {
        return new TricountResponse
        {
            Id = group.Id,
            Name = group.Name,
            Currency = group.Currency,
            CreatedByUserId = group.CreatedByUserId,
            CreatedAt = group.CreatedAt,
            Participants = group.Members.Select(m => new ParticipantResponseDto
            {
                Id = m.Id,
                UserId = m.UserId,
                JoinedAt = m.JoinedAt
            }).ToList()
        };
    }
}
