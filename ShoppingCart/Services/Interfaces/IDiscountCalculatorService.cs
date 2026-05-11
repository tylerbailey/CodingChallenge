using CodingChallenge.Models.Implementations;

namespace CodingChallenge.Services.Interfaces
{
    public interface IDiscountCalculatorService
    {
        decimal CalculateDiscountedPrice(CartItem item, DateTime transactionTime, bool isFirstResponder);
    }
}
