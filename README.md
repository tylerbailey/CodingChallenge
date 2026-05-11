# Shopping Cart Refactor Challenge

## Overview

This project refactors the original grocery store checkout implementation into a more extensible, testable, and maintainable architecture.

The original implementation relied on deeply nested conditional logic inside the checkout calculator. While functionally correct, adding new pricing rules or discount scenarios would quickly become difficult to maintain and scale.

The refactored solution introduces a rule-based pricing engine using dependency injection and strategy composition to support future extension without modifying existing checkout logic.

---

# Refactoring Goals

The primary goals of the refactor were:

- Eliminate deeply nested conditional logic
- Improve maintainability and readability
- Support new discount rules without modifying existing calculator logic
- Improve testability through dependency injection and abstraction
- Demonstrate separation of concerns and extensibility

---

# Architectural Changes

## Rule-Based Discount Engine

Discount logic was extracted into independent implementations of:

```csharp
IDiscountRule
```

Each rule is responsible only for determining:
- whether it applies
- the discount multiplier
- its precedence

Examples:
- `Christmas20Rule`
- `Christmas60Rule`
- `Christmas90Rule`
- `SeniorHoursRule`
- `FirstResponderRule`

This allows new discount behavior to be added without modifying the checkout calculator.

---

## Priority-Based Non-Stacking Discounts

The checkout system resolves discounts using a highest-priority-wins approach.

```csharp
.OrderByDescending(rule => rule.Priority)
```

This prevents discount stacking while still allowing multiple rules to match.

Priority values are centralized through:

```csharp
DiscountPriorities
```

to avoid magic numbers and improve readability.

---

## Separation of Concerns

### `GroceryCheckoutCalculatorService`
Responsible only for:
- orchestrating checkout
- aggregating totals
- delegating pricing behavior

### `NonStackingDiscountCalculatorService`
Responsible only for:
- resolving matching discount rules
- selecting the highest-priority rule
- calculating discounted item totals

### `IDiscountRule`
Responsible only for:
- discount eligibility
- discount metadata

---

## Immutable Domain Models

`Transaction` and `CartItem` are implemented as immutable records to improve:
- predictability
- value equality semantics
- testability
- safety during pricing calculations

---

# Dependency Injection

Discount rules are registered through the .NET dependency injection container and injected into the pricing engine as:

```csharp
IEnumerable<IDiscountRule>
```

This allows pricing behavior to be composed dynamically without changing the calculator implementation.

---

# Testing Strategy

The solution includes unit tests covering:

- Christmas discount tiers
- Date boundary conditions
- Senior hours logic
- Weight-based vs quantity-based pricing
- First responder override behavior
- Mixed-cart calculations
- Empty and zero-value edge cases
- Repository behavior
- Rule precedence behavior
- Strategy replacement/Open-Closed Principle validation

The tests focus on validating business behavior rather than implementation details.

---

# Tradeoffs and Scope Decisions

## Simplified Customer Context

The implementation intentionally passes only:

```csharp
bool isFirstResponder
```

rather than introducing a full customer context object.

For the scope of this exercise, this kept the design simpler while still demonstrating extensibility and rule composition.

In a production system, this would likely evolve into a richer pricing or customer context model as additional customer attributes and pricing policies were introduced.

---

## Repository Implementation

The repository implementation is intentionally in-memory and lightweight.

Its purpose in this exercise is to demonstrate:
- abstraction boundaries
- dependency injection
- separation of pricing logic from persistence concerns

rather than production-grade persistence.

---

# Potential Future Enhancements

Examples of future enhancements supported by the current design:

- Employee discounts
- Coupon codes
- Loyalty pricing
- Regional pricing rules
- Time-window promotions
- Stackable discount policies
- Configurable rule priorities
- Database-backed rule configuration

---

# Technologies Used

- .NET 8
- C#
- NUnit
- Microsoft Dependency Injection

---

# Running the Solution

## Run the application

```bash
dotnet run
```

## Run tests

```bash
dotnet test
```

