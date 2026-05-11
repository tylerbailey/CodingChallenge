using CodingChallenge.Consts;
using CodingChallenge.Models.Implementations;
using CodingChallenge.Rules.Interfaces;

namespace CodingChallenge.Rules.Implementations
{
    public class FirstResponderRule : IDiscountRule
    {
        public int Priority => DiscountPriorities.CustomerOverride;
        public decimal DiscountMultiplier => 0.9m;
        public bool IsMatch(CartItem item, DateTime transactionTime, bool isFirstResponder) => isFirstResponder;

    }
}
