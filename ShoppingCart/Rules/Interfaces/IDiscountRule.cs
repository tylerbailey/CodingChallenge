using CodingChallenge.Models.Implementations;

namespace CodingChallenge.Rules.Interfaces
{
    public interface IDiscountRule
    {
        int Priority { get; }
        decimal DiscountMultiplier { get; }
        bool IsMatch(CartItem item, DateTime transactionTime, bool isFirstResponder);
    }
}
