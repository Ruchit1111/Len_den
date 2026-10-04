using System.Text.Json.Serialization;
using Tricount.Common.Helpers;

namespace Tricount.DTOs.Requests;

public class CreateExpenseRequest
{
    public int GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public int PaidByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// User IDs of the participants sharing this expense.
    /// The total amount is divided equally among these participants.
    /// For example: if amount is 100 and participants are [1, 2], each participant's AmountOwed will be 50.
    /// If omitted or empty, automatically splits equally among all members of the group.
    /// </summary>
    [JsonConverter(typeof(ParticipantIdsConverter))]
    public List<int> Participants { get; set; } = new();

    /// <summary>
    /// Captures any alternative payload formats like participantUserIds
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}
