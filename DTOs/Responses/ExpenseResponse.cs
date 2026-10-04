namespace Tricount.DTOs.Responses;

public class ExpenseResponse
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public int PaidByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ExpenseParticipantResponseDto> Participants { get; set; } = new();
}

public class ExpenseParticipantResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal AmountOwed { get; set; }
}
