namespace Tricount.Models;

public class Expense
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public int PaidByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Group? Group { get; set; }
    public User? PaidByUser { get; set; }
    public ICollection<ExpenseParticipant> Participants { get; set; } = new List<ExpenseParticipant>();
}
