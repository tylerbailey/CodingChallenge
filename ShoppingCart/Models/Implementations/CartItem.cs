using CodingChallenge.Enums;
using CodingChallenge.Models.Interfaces;

namespace CodingChallenge.Models.Implementations
{
    // Cart items may be sold either by quantity or by weight.
    public record CartItem(int Id, string ProductName, ProductCategories Category, decimal Price, decimal Weight = 0, int Quantity = 0) : IEntity
    {
        public decimal GetPurchaseUnits() => Weight > 0 ? Weight : Quantity;
    }
}
