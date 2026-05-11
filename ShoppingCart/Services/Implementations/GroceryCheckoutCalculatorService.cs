using CodingChallenge.Services.Interfaces;
using CodingChallenge.Models.Implementations;

namespace CodingChallenge.Services.Implementations
{
    // Coordinates checkout pricing by applying item-level discount rules and rounding the final total.    
    public class GroceryCheckoutCalculatorService(IDiscountCalculatorService discountService) : ICheckoutCalculatorService
    {
        private readonly IDiscountCalculatorService _discountCalculatorService = discountService;

        public decimal Calculate(Transaction transaction)
        {
            var totalPrice = transaction.Items.Sum(item => _discountCalculatorService.CalculateDiscountedPrice(item, transaction.TransactionTime, transaction.IsFirstResponder));
            return Math.Round(totalPrice, 2);
        }
    }
}
