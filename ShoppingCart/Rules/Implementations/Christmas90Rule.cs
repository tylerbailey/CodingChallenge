using CodingChallenge.Consts;
using CodingChallenge.Enums;
using CodingChallenge.Models.Implementations;
using CodingChallenge.Rules.Interfaces;

namespace CodingChallenge.Rules.Implementations
{
    public class Christmas90Rule : IDiscountRule
    {
        public int Priority => DiscountPriorities.Seasonal;
        public decimal DiscountMultiplier => 0.10m;
        public bool IsMatch(CartItem item, DateTime transactionTime, bool isFirstResponder)
        {
            var isPostChristmasClearance =
             transactionTime.Month == 12 &&
             transactionTime.Day > 25;

            var isJanuaryClearance =
                transactionTime.Month == 1 &&
                transactionTime.Day < 15;

            return item.Category == ProductCategories.Christmas
                   && (isPostChristmasClearance || isJanuaryClearance);
        }
    }
}
