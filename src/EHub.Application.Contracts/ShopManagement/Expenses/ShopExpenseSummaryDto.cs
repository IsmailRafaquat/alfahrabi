namespace EHub.ShopManagement.Expenses;

public class ShopExpenseSummaryDto
{
    public decimal? TotalPostedExpenses { get; set; }
    public decimal? TotalDraftExpenses { get; set; }
    public decimal? TotalCancelledExpenses { get; set; }

    public long PostedExpenseCount { get; set; }
    public long DraftExpenseCount { get; set; }
    public long CancelledExpenseCount { get; set; }
}
