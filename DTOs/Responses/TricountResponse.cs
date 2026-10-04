namespace Tricount.DTOs.Responses;

public class TricountResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ParticipantResponseDto> Participants { get; set; } = new();
}

public class ParticipantResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; }
}
