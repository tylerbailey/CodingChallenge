using CodingChallenge.Models.Interfaces;
using System.Collections.Immutable;

namespace CodingChallenge.Models.Implementations
{
    // Immutable checkout snapshot used for pricing and persistence.
    public record Transaction(int Id, ImmutableArray<CartItem> Items, DateTime TransactionTime, bool IsFirstResponder = false) : IEntity;
}
