using CodingChallenge;
using CodingChallenge.Enums;
using CodingChallenge.Models.Implementations;
using CodingChallenge.Repositories.Implementations;
using CodingChallenge.Repositories.Interfaces;
using CodingChallenge.Rules.Implementations;
using CodingChallenge.Rules.Interfaces;
using CodingChallenge.Services.Implementations;
using CodingChallenge.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Immutable;

namespace GroceryStore.Tests
{
    // ────────────────────────────────────────────────────────────────────────────
    // Fakes
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A stacking discount service stub used to verify that swapping the
    /// IDiscountCalculatorService implementation requires zero changes to
    /// GroceryStoreCheckoutCalculator (Open/Closed Principle).
    /// Returns full price for every item regardless of category or customer type.
    /// </summary>
    public class NoDiscountCalculatorService : IDiscountCalculatorService
    {
        public decimal CalculateDiscountedPrice(CartItem item, DateTime transactionTime, bool isFirstResponder)
            => item.GetPurchaseUnits() * item.Price;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // GroceryStoreCheckoutCalculator Tests
    // ────────────────────────────────────────────────────────────────────────────

    [TestFixture]
    public class GroceryStoreCheckoutCalculatorTests
    {
        private ICheckoutCalculatorService _calculator;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection()
       .AddSingleton<IDiscountRule, FirstResponderRule>()
       .AddSingleton<IDiscountRule, SeniorHoursRule>()
       .AddSingleton<IDiscountRule, Christmas20Rule>()
       .AddSingleton<IDiscountRule, Christmas60Rule>()
       .AddSingleton<IDiscountRule, Christmas90Rule>()
       .AddSingleton<IDiscountCalculatorService, NonStackingDiscountCalculatorService>()
       .AddSingleton<ICheckoutCalculatorService, GroceryCheckoutCalculatorService>()
       .BuildServiceProvider();

            _calculator = services.GetRequiredService<ICheckoutCalculatorService>();
        }

        // ────────────────────────────────────────────────
        // CHRISTMAS CATEGORY
        // ────────────────────────────────────────────────

        [Test]
        public void Calculate_ChristmasItem_InDecemberBeforeDay15_Applies20PercentDiscount()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 12, 10)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(80m));
        }

        [Test]
        public void Calculate_ChristmasItem_InDecemberDay15To25_Applies60PercentDiscount()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Tree", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 12, 20)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(40m));
        }

        [Test]
        public void Calculate_ChristmasItem_InDecemberAfterDay25_Applies90PercentDiscount()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Ornaments", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 12, 26)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(10m));
        }

        [Test]
        public void Calculate_ChristmasItem_OnDecemberDay15_Applies60PercentDiscount()
        {
            // Boundary: day == 15 falls into the <= 25 branch
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 12, 15)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(40m));
        }

        [Test]
        public void Calculate_ChristmasItem_OnDecemberDay25_Applies60PercentDiscount()
        {
            // Boundary: day == 25 is still <= 25
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 12, 25)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(40m));
        }

        [Test]
        public void Calculate_ChristmasItem_NotInDecemberOrJanuary_NoDiscount()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 2)],
                TransactionTime: new DateTime(2024, 11, 10)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(200m));
        }

        [Test]
        public void Calculate_ChristmasItem_InJanuaryBeforeDay15_AppliesClearanceDiscount()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 1, 10)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(10m));
        }

        [Test]
        public void Calculate_ChristmasItem_InJanuaryOnOrAfterDay15_NoDiscount()
        {
            // Boundary: Jan 15 falls through to default — full price
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 1, 15)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(100m));
        }

        [Test]
        public void Calculate_ChristmasItem_QuantityIsMultiplied()
        {
            // 3 items at $50, Dec 10 → 20% off each → 3 * 40 = 120
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 50m, Quantity: 3)],
                TransactionTime: new DateTime(2024, 12, 10)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(120m));
        }

        // ────────────────────────────────────────────────
        // FOOD CATEGORY — WEIGHT-BASED
        // ────────────────────────────────────────────────

        [Test]
        public void Calculate_FoodItemWithWeight_DuringSeniorHours_Applies10PercentDiscount()
        {
            // Hour 7 is in (6, 8] → senior discount → weight * price * 0.9
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Apple", Category: ProductCategories.Food, Price: 10m, Weight: 2m)],
                TransactionTime: new DateTime(2024, 6, 1, 7, 0, 0)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(18m)); // 2 * 10 * 0.9
        }

        [Test]
        public void Calculate_FoodItemWithWeight_OutsideSeniorHours_NoDiscount()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Apple", Category: ProductCategories.Food, Price: 10m, Weight: 2m)],
                TransactionTime: new DateTime(2024, 6, 1, 10, 0, 0)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(20m)); // 2 * 10
        }

        [Test]
        public void Calculate_FoodItemWithWeight_AtHour6_NoSeniorDiscount()
        {
            // Boundary: hour == 6 is NOT in (6, 8] — no discount
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Apple", Category: ProductCategories.Food, Price: 10m, Weight: 2m)],
                TransactionTime: new DateTime(2024, 6, 1, 6, 0, 0)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(20m));
        }

        [Test]
        public void Calculate_FoodItemWithWeight_AtHour8_AppliesSeniorDiscount()
        {
            // Boundary: hour == 8 IS in (6, 8]
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Apple", Category: ProductCategories.Food, Price: 10m, Weight: 2m)],
                TransactionTime: new DateTime(2024, 6, 1, 8, 0, 0)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(18m));
        }

        // ────────────────────────────────────────────────
        // FOOD CATEGORY — QUANTITY-BASED
        // ────────────────────────────────────────────────

        [Test]
        public void Calculate_FoodItemNoWeight_DuringSeniorHours_Applies10PercentDiscount()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Salad", Category: ProductCategories.Food, Price: 10m, Quantity: 3)],
                TransactionTime: new DateTime(2024, 6, 1, 7, 0, 0)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(27m)); // 3 * 10 * 0.9
        }

        [Test]
        public void Calculate_FoodItemNoWeight_OutsideSeniorHours_NoDiscount()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Salad", Category: ProductCategories.Food, Price: 10m, Quantity: 3)],
                TransactionTime: new DateTime(2024, 6, 1, 12, 0, 0)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(30m));
        }

        // ────────────────────────────────────────────────
        // FIRST RESPONDER
        // ────────────────────────────────────────────────

        [Test]
        public void Calculate_FirstResponder_AnyCategory_AppliesFirstResponderDiscount()
        {
            // First responder branch fires before category check
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Misc", Category: ProductCategories.Food, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 6, 1, 12, 0, 0),
                IsFirstResponder: true
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(90m)); // 100 * 0.90
        }

        [Test]
        public void Calculate_FirstResponder_ChristmasItem_AppliesFirstResponderDiscountNotChristmas()
        {
            // First responder discount takes priority over Christmas discount
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 12, 10),
                IsFirstResponder: true
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(90m)); // FR discount, not 80m Christmas discount
        }

        [Test]
        public void Calculate_FirstResponder_Multiple_AnyCategory_AppliesFirstResponderDiscount()
        {
            // First responder branch fires before category check
            var transaction = new Transaction(
                Id: 1,
                Items:
                [
                    new CartItem(Id: 1, ProductName: "Misc", Category: ProductCategories.Food, Price: 100m, Quantity: 1),
                    new CartItem(Id: 2, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1),
                    new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1),
                ],
                TransactionTime: new DateTime(2024, 3, 10, 12, 0, 0),
                IsFirstResponder: true
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(270m)); // 300 * 0.90
        }
        // ────────────────────────────────────────────────
        // DEFAULT CATEGORY
        // ────────────────────────────────────────────────

        [Test]
        public void Calculate_UnknownCategory_ReturnsFullPrice()
        {
            // Default branch — quantity * price, no discount
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Misc", Category: (ProductCategories)99, Price: 50m, Quantity: 2)],
                TransactionTime: new DateTime(2024, 6, 1, 12, 0, 0)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(100m));
        }

        // ────────────────────────────────────────────────
        // MIXED CART
        // ────────────────────────────────────────────────

        [Test]
        public void Calculate_MixedCart_ReturnsSumOfAllLineItems()
        {
            // Christmas Dec 10 → $100 * 0.80 = $80
            // Food weighted senior hour → 2 * $10 * 0.90 = $18
            // Total = $98
            var transaction = new Transaction(
                Id: 1,
                Items:
                [
                    new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1),
                    new CartItem(Id: 2, ProductName: "Apple",  Category: ProductCategories.Food,      Price: 10m,  Weight: 2m)
                ],
                TransactionTime: new DateTime(2024, 12, 10, 7, 0, 0)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(98m));
        }

        // ────────────────────────────────────────────────
        // EDGE CASES
        // ────────────────────────────────────────────────

        [Test]
        public void Calculate_EmptyCart_ReturnsZero()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [],
                TransactionTime: new DateTime(2024, 6, 1)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(0m));
        }

        [Test]
        public void Calculate_ZeroPriceItem_ReturnsZero()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Free Item", Category: ProductCategories.Food, Price: 0m, Quantity: 5)],
                TransactionTime: new DateTime(2024, 6, 1)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(0m));
        }

        [Test]
        public void Calculate_ZeroQuantityAndWeight_ReturnsZero()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Ghost Item", Category: ProductCategories.Food, Price: 50m)],
                TransactionTime: new DateTime(2024, 6, 1)
            );

            var result = _calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(0m));
        }

        // ────────────────────────────────────────────────
        // OPEN/CLOSED — STRATEGY SWAP
        // ────────────────────────────────────────────────

        [Test]
        public void Calculate_WithNoDiscountService_ReturnsFullPrice()
        {
            // Demonstrates that swapping IDiscountCalculatorService requires
            // zero changes to GroceryStoreCheckoutCalculator
            var services = new ServiceCollection()
                .AddSingleton<IDiscountCalculatorService, NoDiscountCalculatorService>()
                .AddSingleton<ICheckoutCalculatorService, GroceryCheckoutCalculatorService>()
                .BuildServiceProvider();

            var calculator = services.GetRequiredService<ICheckoutCalculatorService>();

            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 100m, Quantity: 1)],
                TransactionTime: new DateTime(2024, 12, 10) // would normally get 20% off
            );

            var result = calculator.Calculate(transaction);

            Assert.That(result, Is.EqualTo(100m)); // no discount applied
        }

        // ────────────────────────────────────────────────────────────────────────────
        // Rules Tests
        // ────────────────────────────────────────────────────────────────────────────

        [Test]
        public void ServiceCollection_RegistersDiscountRules()
        {
            var services = new ServiceCollection()
                .AddSingleton<IDiscountRule, FirstResponderRule>()
                .AddSingleton<IDiscountRule, SeniorHoursRule>()
                .AddSingleton<IDiscountRule, Christmas20Rule>()
                .AddSingleton<IDiscountRule, Christmas60Rule>()
                .AddSingleton<IDiscountRule, Christmas90Rule>()
                .BuildServiceProvider();

            var rules = services.GetServices<IDiscountRule>().ToList();

            Assert.That(rules, Has.Count.EqualTo(5));
        }

        [Test]
        public void DiscountCalculator_WhenMultipleRulesMatch_UsesHighestPriorityRule()
        {
            var item = new CartItem(1, "Lights", ProductCategories.Christmas, 100m, Quantity: 1);

            var service = new NonStackingDiscountCalculatorService(
            [
                new Christmas20Rule(),
        new FirstResponderRule()
            ]);

            var result = service.CalculateDiscountedPrice(
                item,
                new DateTime(2024, 12, 10),
                isFirstResponder: true);

            Assert.That(result, Is.EqualTo(90m));
        }
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Repository Tests
    // ────────────────────────────────────────────────────────────────────────────

    [TestFixture]
    public class RepositoryTests
    {
        private IRepository<Transaction> _repository;

        [SetUp]
        public void SetUp()
        {
            _repository = new Repository<Transaction>();
        }

        [Test]
        public void Save_Transaction_CanBeRetrievedById()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [new CartItem(Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 5.99m, Quantity: 10)],
                TransactionTime: new DateTime(2024, 12, 10)
            );

            _repository.Save(transaction);
            var retrieved = _repository.Retrieve(1);

            Assert.That(retrieved, Is.EqualTo(transaction));
        }

        [Test]
        public void Retrieve_NonExistentId_ReturnsNull()
        {
            var result = _repository.Retrieve(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Update_ExistingTransaction_ReplacesCorrectly()
        {
            var transaction = new Transaction(
                Id: 1,
                Items: [],
                TransactionTime: new DateTime(2024, 12, 10)
            );
            _repository.Save(transaction);

            var updated = transaction with { TransactionTime = new DateTime(2024, 12, 30) };
            _repository.Update(updated);

            var retrieved = _repository.Retrieve(1);
            Assert.That(retrieved!.TransactionTime, Is.EqualTo(new DateTime(2024, 12, 30)));
        }

        [Test]
        public void Update_NonExistentTransaction_DoesNothing()
        {
            var transaction = new Transaction(
                Id: 999,
                Items: [],
                TransactionTime: new DateTime(2024, 12, 10)
            );

            // Should not throw
            Assert.DoesNotThrow(() => _repository.Update(transaction));
        }

        [Test]
        public void Save_MultipleTransactions_AllRetrievableById()
        {
            var t1 = new Transaction(Id: 1, Items: [], TransactionTime: new DateTime(2024, 12, 10));
            var t2 = new Transaction(Id: 2, Items: [], TransactionTime: new DateTime(2024, 11, 30));

            _repository.Save(t1);
            _repository.Save(t2);

            Assert.That(_repository.Retrieve(1), Is.EqualTo(t1));
            Assert.That(_repository.Retrieve(2), Is.EqualTo(t2));
        }

        [Test]
        public void Record_ValueEquality_TwoIdenticalTransactionsAreEqual()
        {
            // Demonstrates record value equality — relevant to Retrieve using ==
            var t1 = new Transaction(Id: 1, Items: [], TransactionTime: new DateTime(2024, 12, 10));
            var t2 = new Transaction(Id: 1, Items: [], TransactionTime: new DateTime(2024, 12, 10));

            Assert.That(t1, Is.EqualTo(t2));
        }
    }

    // ────────────────────────────────────────────────────────────────────────────
    // CartItem Tests
    // ────────────────────────────────────────────────────────────────────────────

    [TestFixture]
    public class CartItemTests
    {
        [Test]
        public void GetUnits_WeightGreaterThanZero_ReturnsWeight()
        {
            var item = new CartItem(Id: 1, ProductName: "Apple", Category: ProductCategories.Food, Price: 3.27m, Weight: 0.79m);

            Assert.That(item.GetPurchaseUnits(), Is.EqualTo(0.79m));
        }

        [Test]
        public void GetUnits_WeightIsZero_ReturnsQuantity()
        {
            var item = new CartItem(Id: 1, ProductName: "Salad", Category: ProductCategories.Food, Price: 6.99m, Quantity: 2);

            Assert.That(item.GetPurchaseUnits(), Is.EqualTo(2));
        }

        [Test]
        public void GetUnits_BothZero_ReturnsZero()
        {
            var item = new CartItem(Id: 1, ProductName: "Ghost", Category: ProductCategories.Food, Price: 10m);

            Assert.That(item.GetPurchaseUnits(), Is.EqualTo(0));
        }

        [Test]
        public void Record_ValueEquality_TwoIdenticalItemsAreEqual()
        {
            var a = new CartItem(Id: 1, ProductName: "Apple", Category: ProductCategories.Food, Price: 3.27m, Weight: 0.79m);
            var b = new CartItem(Id: 1, ProductName: "Apple", Category: ProductCategories.Food, Price: 3.27m, Weight: 0.79m);

            Assert.That(a, Is.EqualTo(b));
        }
    }
}