using CodingChallenge.Models.Implementations;
using CodingChallenge.Rules.Interfaces;
using CodingChallenge.Services.Interfaces;

namespace CodingChallenge.Services.Implementations
{
     // Resolves the highest-priority matching discount rule for a cart item.
    public class NonStackingDiscountCalculatorService(IEnumerable<IDiscountRule> rules) : IDiscountCalculatorService
    {
        private readonly IReadOnlyList<IDiscountRule> _rules = rules.ToList();

        public decimal CalculateDiscountedPrice(CartItem item, DateTime transactionTime, bool isFirstResponder)
        {
            var multiplier = _rules
           .Where(rule => rule.IsMatch(item, transactionTime, isFirstResponder))
           .OrderByDescending(rule => rule.Priority)
           .Select(rule => rule.DiscountMultiplier)
           .FirstOrDefault(1m);

            return multiplier * item.GetPurchaseUnits() * item.Price;
            
        }
    }
}
