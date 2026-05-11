using CodingChallenge.Consts;
using CodingChallenge.Enums;
using CodingChallenge.Models.Implementations;
using CodingChallenge.Rules.Interfaces;

namespace CodingChallenge.Rules.Implementations
{
    public class Christmas60Rule : IDiscountRule
    {
        public int Priority => DiscountPriorities.Seasonal;
        public decimal DiscountMultiplier => 0.4m;
        public bool IsMatch(CartItem item, DateTime transactionTime, bool isFirstResponder) => 
            item.Category == ProductCategories.Christmas && transactionTime.Month == 12 && transactionTime.Day >= 15 && transactionTime.Day <= 25;
    }
}
