namespace Tricount.Models;

public class ExpenseParticipant
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public int UserId { get; set; }
    public decimal AmountOwed { get; set; }

    // Navigation properties
    public Expense? Expense { get; set; }
    public User? User { get; set; }
}
