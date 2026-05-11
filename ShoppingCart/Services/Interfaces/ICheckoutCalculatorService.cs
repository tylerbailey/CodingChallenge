using CodingChallenge.Models.Implementations;

namespace CodingChallenge.Services.Interfaces
{
    public interface ICheckoutCalculatorService
    {
        decimal Calculate(Transaction transaction);
    }
}
