namespace Tricount.DTOs.Requests;

public class UpdateTricountRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public int CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<ParticipantRequestDto> Participants { get; set; } = new();
}

public class ParticipantRequestDto
{
    public int? Id { get; set; }
    public int UserId { get; set; }
}
