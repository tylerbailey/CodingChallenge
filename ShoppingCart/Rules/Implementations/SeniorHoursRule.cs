using CodingChallenge.Consts;
using CodingChallenge.Enums;
using CodingChallenge.Models.Implementations;
using CodingChallenge.Rules.Interfaces;

namespace CodingChallenge.Rules.Implementations
{
    public class SeniorHoursRule : IDiscountRule
    {
        public int Priority => DiscountPriorities.SeniorHours;
        public decimal DiscountMultiplier => 0.90m;
        public bool IsMatch(CartItem item, DateTime transactionTime, bool isFirstResponder) =>
            item.Category == ProductCategories.Food && transactionTime.TimeOfDay.Hours > 6 && transactionTime.TimeOfDay.Hours <= 8;
    }
}
