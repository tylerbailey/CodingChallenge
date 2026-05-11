// Ncontracts Code Challenge
//
//
// Please take a look at the code below.
// Even though the program runs and generates correct result, we consider the code to be bad.
// For example, if we want to expand the Christmas discount to January or if we want to introduce a first responder discount, the code can get really messy.
// We ask you to refactor the code so that it's easier to apply new changes to it.
// Once done, please send me a link to your gists.

using CodingChallenge.Enums;
using CodingChallenge.Models.Implementations;
using CodingChallenge.Repositories.Implementations;
using CodingChallenge.Repositories.Interfaces;
using CodingChallenge.Rules.Interfaces;
using CodingChallenge.Rules.Implementations;
using CodingChallenge.Services.Implementations;
using CodingChallenge.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CodingChallenge
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddScoped<IDiscountCalculatorService, NonStackingDiscountCalculatorService>()
                .AddScoped<ICheckoutCalculatorService, GroceryCheckoutCalculatorService>()
                .AddScoped<IRepository<Transaction>, Repository<Transaction>>()
                .AddScoped<IDiscountRule, FirstResponderRule>()
                .AddScoped<IDiscountRule, SeniorHoursRule>()
                .AddScoped<IDiscountRule, Christmas20Rule>()
                .AddScoped<IDiscountRule, Christmas60Rule>()
                .AddScoped<IDiscountRule, Christmas90Rule>()
                .BuildServiceProvider();
            using var scope = services.CreateScope();

            var calculator = scope.ServiceProvider.GetRequiredService<ICheckoutCalculatorService>();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Transaction>>();

            var program = new Program();
           
            program.ChristmasShoppingAtTheGroceryStore(calculator, repository);
            program.BuyingFood(calculator, repository);
        }

        void ChristmasShoppingAtTheGroceryStore(ICheckoutCalculatorService calculator, IRepository<Transaction> transactionRepository)
        {
            var transaction = new Transaction(
                Id: 1,
                Items:
                    [
                        new CartItem (Id: 1, ProductName: "Lights", Category: ProductCategories.Christmas, Price: 5.99m, Quantity : 10),
                        new CartItem (Id: 2, ProductName: "Tree", Category: ProductCategories.Christmas, Price: 169m, Quantity : 1),
                        new CartItem (Id: 3, ProductName: "Ornaments", Category: ProductCategories.Christmas, Price: 8m, Quantity : 15),
                    ],
                TransactionTime: new DateTime(2020, 11, 30)
            );
            var total = calculator.Calculate(transaction);
            transactionRepository.Save(transaction);
            Console.WriteLine(total);

            transaction = transaction with { TransactionTime = new DateTime(2020, 12, 30) };
            var totalAfterChristmas = calculator.Calculate(transaction);
            transactionRepository.Update(transaction);
            Console.WriteLine(totalAfterChristmas);
        }

        void BuyingFood(ICheckoutCalculatorService calculator, IRepository<Transaction> transactionRepository)
        {
            var transaction = new Transaction(
                Id: 2,
                Items:
                    [
                        new CartItem (Id: 1, ProductName: "Apple", Category: ProductCategories.Food, Price: 3.27m, Weight : 0.79m),
                        new CartItem (Id: 2, ProductName: "Scallop", Category: ProductCategories.Food, Price: 18m, Weight : 1.5m),
                        new CartItem (Id: 3, ProductName: "Salad", Category: ProductCategories.Food, Price: 6.99m, Quantity : 1),
                        new CartItem (Id: 4, ProductName: "Ground Beef", Category: ProductCategories.Food, Price: 7.99m, Weight : 1.5m),
                        new CartItem (Id: 5, ProductName: "Red Wine", Category: ProductCategories.Food, Price: 25.99m, Quantity : 1)
                    ],
                TransactionTime: new DateTime(2020, 11, 30)
            );

            var total = calculator.Calculate(transaction);
            transactionRepository.Save(transaction);
            Console.WriteLine(total);

            transaction = transaction with { TransactionTime = new DateTime(2020, 11, 30, 7, 11, 0) };
            var seniorHourTotal = calculator.Calculate(transaction);
            transactionRepository.Update(transaction);
            Console.WriteLine(seniorHourTotal);
        }
    }
}


