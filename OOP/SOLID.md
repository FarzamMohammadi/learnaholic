# SOLID - 
**Created By: Michael Feathers**
**Promoted By: Uncle Bob**

## Single Responsibility Principle (SRP)
A class should do only one thing, and there for have only one reason to change.

```csharp
// ❌ BAD: Single class handling multiple responsibilities
public class Employee
{
    public string Name { get; set; }
    public decimal Salary { get; set; }

    public void CalculateSalary()
    {
        // Salary calculation logic
        Salary = /* calculation */ 1000;
    }

    public void SaveToDatabase()
    {
        // Database logic
        Console.WriteLine($"Saving {Name} to database");
    }

    public void GeneratePayslip()
    {
        // PDF generation logic
        Console.WriteLine($"Generating payslip for {Name}");
    }

    public void SendPayslipEmail()
    {
        // Email logic
        Console.WriteLine($"Sending payslip to {Name}");
    }
}

// ✅ GOOD: Separated responsibilities into focused classes
public class Employee
{
    public string Name { get; set; }
    public decimal Salary { get; set; }

    public void CalculateSalary()
    {
        // Only handles salary calculation
        Salary = /* calculation */ 1000;
    }
}

public class EmployeeRepository
{
    public void Save(Employee employee)
    {
        // Only handles database operations
        Console.WriteLine($"Saving {employee.Name} to database");
    }
}

public class PayslipGenerator
{
    public void GeneratePayslip(Employee employee)
    {
        // Only handles payslip generation
        Console.WriteLine($"Generating payslip for {employee.Name}");
    }
}

public class EmailService
{
    public void SendPayslipEmail(Employee employee)
    {
        // Only handles email communication
        Console.WriteLine($"Sending payslip to {employee.Name}");
    }
}

// Usage Example
public class Example
{
    public void ProcessEmployee()
    {
        // Each class has a single responsibility
        var employee = new Employee { Name = "John" };
        employee.CalculateSalary();

        var repository = new EmployeeRepository();
        repository.Save(employee);

        var payslipGenerator = new PayslipGenerator();
        payslipGenerator.GeneratePayslip(employee);

        var emailService = new EmailService();
        emailService.SendPayslipEmail(employee);
    }
}
```

## Open/Closed Principle (OCP)
Classes should be open for extension and closed for modification.

- **Key Points**:
  - Open for extension (should be able to add new behaviors)
  - Closed for modification (shouldn't need to change existing code)

- **Benefits**
  - Reduces risk of bugs in existing code
  - Makes maintenance easier
  - Enables safe scaling of features

- **Common Implementation Techniques**:
  - Interfaces
  - Abstract classes
  - Inheritance
  - Polymorphism
  - Strategy pattern - Algorithms can be changed at runtime
    - Defines a family of algorithms, encapsulates each one, and makes them interchangeable. 
    -   Example:
        ```csharp
        // Example: Different shipping methods for an e-commerce store

        // 1. Define the strategy interface
        public interface IShippingStrategy
        {
            decimal CalculateShipping(decimal orderWeight);
        }

        // 2. Implement concrete strategies
        public class StandardShipping : IShippingStrategy
        {
            public decimal CalculateShipping(decimal orderWeight)
                => orderWeight * 2.0m;
        }

        public class ExpressShipping : IShippingStrategy
        {
            public decimal CalculateShipping(decimal orderWeight)
                => orderWeight * 3.5m;
        }

        // 3. Context class that uses the strategy
        public class Order
        {
            private readonly IShippingStrategy _shippingStrategy;
            public decimal Weight { get; set; }

            public Order(IShippingStrategy shippingStrategy)
            {
                _shippingStrategy = shippingStrategy;
            }

            public decimal CalculateShippingCost()
                => _shippingStrategy.CalculateShipping(Weight);
        }

        // 4. Usage
        var standardOrder = new Order(new StandardShipping()) { Weight = 10 };
        var expressOrder = new Order(new ExpressShipping()) { Weight = 10 };

        decimal standardCost = standardOrder.CalculateShippingCost(); // 20
        decimal expressCost = expressOrder.CalculateShippingCost();   // 35
        ```

- **Warning Signs**:
  - Large if/else chains
  - Frequent modification of existing classes
  - Switch statements for type checking
  - Direct class modifications for new features

- **Best Practices**:
  - Design for inheritance
  - Depend on abstractions
  - Use (third party) plugins/modules when possible (you extend functionality without modifying the core system)
  - Plan for future extensions

- **Example**:

    ```csharp
    // ❌ BAD: Violates OCP - needs modification for each new discount type
    public class OrderProcessor
    {
        public decimal CalculateDiscount(Order order)
        {
            decimal discount = 0;
            
            // Need to modify this method every time we add a new discount rule
            if (order.Value > 1000)
                discount += order.Value * 0.1m; // 10% discount for large orders
                
            if (order.CustomerType == "Premium")
                discount += order.Value * 0.05m; // 5% for premium customers
                
            if (order.IsHolidaySeason)
                discount += order.Value * 0.05m; // 5% holiday discount
                
            return discount;
        }
    }

    // ✅ GOOD: Follows OCP - can add new discount rules without changing existing code
    public abstract class DiscountRule
    {
        public abstract decimal CalculateDiscount(Order order);
    }

    // Core discount rules defined once
    public class LargeOrderDiscount : DiscountRule
    {
        public override decimal CalculateDiscount(Order order)
        {
            if (order.Value > 1000)
                return order.Value * 0.1m;
            return 0;
        }
    }

    public class PremiumCustomerDiscount : DiscountRule
    {
        public override decimal CalculateDiscount(Order order)
        {
            if (order.CustomerType == "Premium")
                return order.Value * 0.05m;
            return 0;
        }
    }

    // The processor that's truly closed for modification
    public class OrderProcessor
    {
        private readonly List<DiscountRule> _discountRules;

        public OrderProcessor(List<DiscountRule> discountRules)
        {
            _discountRules = discountRules;
        }

        // This method never needs to change
        public decimal CalculateDiscount(Order order)
        {
            return _discountRules.Sum(rule => rule.CalculateDiscount(order));
        }
    }

    // Later, we can add new discount rules without modifying OrderProcessor
    public class HolidaySeasonDiscount : DiscountRule
    {
        public override decimal CalculateDiscount(Order order)
        {
            if (order.IsHolidaySeason)
                return order.Value * 0.05m;
            return 0;
        }
    }

    // Usage
    public class Example
    {
        public void ProcessOrder()
        {
            var rules = new List<DiscountRule>
            {
                new LargeOrderDiscount(),
                new PremiumCustomerDiscount(),
                // Can add new rules without changing OrderProcessor
                new HolidaySeasonDiscount()
            };

            var processor = new OrderProcessor(rules);
            var order = new Order { Value = 2000, CustomerType = "Premium", IsHolidaySeason = true };
            var discount = processor.CalculateDiscount(order);
        }
    }
    /*
    True Closure for Modification:
    - The OrderProcessor class is truly closed - its CalculateDiscount method never needs to change
    - The core logic for combining discounts is written once and stays fixed

    Open for Extension:
    - New discount types can be added by creating new DiscountRule classes
    - No existing code needs to be modified to add new discount behaviors

    The Strategy Pattern:
    - Uses the Strategy pattern to make the code genuinely extensible
    - Each discount rule is a strategy that can be plugged into the processor
    */
    ```

## Liskov Substitution Principle (LSP)
Derived classed should be substitutable for their base classes. Objects of a super class should be replaceable with objects of a subclass without affecting correctness.

- **Key Points**: These rules ensure that any code working with the base class will work correctly with any of its subclasses without knowing the specific subtype.
  - Subtypes (derived classes) MUST maintain contracts
    - Preconditions cannot be strengthened
      - Subclass methods should accept anything base class implements
      - Can't add more restrictions in subclass
      - Can relax requirements but not make them stricter
    - Postconditions cannot be weakened
      - Subclass must guarantee at least what base class promises
      - Can make stronger guarantees but not weaker
      - Must maintain or exceed base class exceptions
    - Invariants must be preserved
      - Core properties that must always be true
      - All class variants must maintain these rules
      - Ensure consistent behavior across inheritance hierarchy

- **Warning Signs**:
  - Empty methods in a subclass
  - Throwing exceptions in subclass methods
  - Type checking (if/else) base on subclass
  - Overridden methods that don't maintain base behavior

- **Best Practices**:
  - Use inheritance only for true "is-a" relationships
  - Design by contract
  - Use composition over inheritance when in doubt
  - Test substitutability

- **Example**:

    ```csharp
    // ❌ BAD: Violates LSP - Square can't properly substitute Rectangle
    public class Rectangle
    {
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }

        public int CalculateArea()
        {
            return Width * Height;
        }
    }

    public class Square : Rectangle
    {
        private int _size;

        public override int Width
        {
            get { return _size; }
            set { _size = value; }
        }

        public override int Height
        {
            get { return _size; }
            set { _size = value; }
        }
    }

    // This test will fail for Square!
    public class Test
    {
        public void TestRectangle(Rectangle rectangle)
        {
            rectangle.Width = 5;
            rectangle.Height = 10;
            
            // Should be 50 for rectangle, but will be 100 for square!
            Console.WriteLine(rectangle.CalculateArea());
        }
    }

    // ✅ GOOD: Follows LSP - Each shape has its own contract
    public interface IShape
    {
        int CalculateArea();
    }

    public class Rectangle : IShape
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public int CalculateArea()
        {
            return Width * Height;
        }
    }

    public class Square : IShape
    {
        public int Size { get; set; }

        public int CalculateArea()
        {
            return Size * Size;
        }
    }

    // Usage Example
    public class Example
    {
        public void CalculateAreas()
        {
            // Bad example - LSP violation
            Rectangle rect = new Rectangle { Width = 5, Height = 10 };
            Rectangle square = new Square { Width = 5 }; // Height will also become 5!
            
            Console.WriteLine(rect.CalculateArea());    // 50
            Console.WriteLine(square.CalculateArea());  // 25 (unexpected!)

            // Good example - LSP compliant
            IShape goodRect = new Rectangle { Width = 5, Height = 10 };
            IShape goodSquare = new Square { Size = 5 };
            
            Console.WriteLine(goodRect.CalculateArea());   // 50
            Console.WriteLine(goodSquare.CalculateArea()); // 25 (as expected)
        }
    }

    /*
    1. The LSP Violation:

    - Square inherits from Rectangle but breaks its behavior
    - Changing Width also changes Height in Square
    - Code that works with Rectangle won't work correctly with Square
    - This violates the "substitutability" principle

    1. The Solution:

    - Use an interface instead of inheritance
    - Each shape has its own independent implementation
    - No unexpected behavior when using either shape
    - Both shapes can be used interchangeably through the interface

    1. Why It Matters:

    - Prevents subtle bugs in code that expects Rectangle behavior
    - Makes the code more predictable
    - Maintains the "is-substitutable-for" relationship
    */
    ```

## Interface Segregation Principle (ISP)
Many client-specific interfaces are better than one general-purpose interface. Clients should not be forced to depend upon interfaces that they do not use.

- **Key Points**:
  - Split large, general purpose interfaces into smaller, more specific ones
  - Interfaces should be focused and cohesive
  - Classes should only implement the methods they need 
    - Prevent "Interface Pollution" by refraining from making classes implement irrelevant methods

- **Example**:

    ```csharp
    // BAD - Fat interface that violates ISP
    public interface IWorkstation
    {
        void Print();
        void Scan();
        void SendFax();
        void PhotoCopy();
    }

    public class SimplePrinter : IWorkstation
    {
        public void Print() => Console.WriteLine("Printing...");
        
        // Forced to implement unnecessary methods
        public void Scan() => throw new NotImplementedException();
        public void SendFax() => throw new NotImplementedException();
        public void PhotoCopy() => throw new NotImplementedException();
    }

    // GOOD - Segregated interfaces
    public interface IPrinter
    {
        void Print();
    }

    public interface IScanner
    {
        void Scan();
    }

    public interface IFaxMachine
    {
        void SendFax();
    }

    // Classes implement only what they need
    public class SimplePrinter : IPrinter
    {
        public void Print() => Console.WriteLine("Printing...");
    }

    public class AllInOnePrinter : IPrinter, IScanner, IFaxMachine
    {
        public void Print() => Console.WriteLine("Printing...");
        public void Scan() => Console.WriteLine("Scanning...");
        public void SendFax() => Console.WriteLine("Sending fax...");
    }

    // Client code only depends on what it needs
    public class DocumentProcessor
    {
        private readonly IPrinter _printer;

        public DocumentProcessor(IPrinter printer)
        {
            _printer = printer;
        }

        public void ProcessDocument()
        {
            _printer.Print(); // Only cares about printing
        }
    }

## Dependency Inversion Principle (DIP) 
Classes should depend on interfaces and abstract classes instead of concrete classes and function. Described another way: High-level modules should not depend on low-level modules. Both should depend on abstractions

Depend on abstractions, not concretions!

- **Key Points**:
  - Dependency Direction
    - High-level modules defined interfaces
    - Low-level modules implement interfaces
    - Dependencies point inward (A key part of DIP and Clean Architecture)
      - This rule dictates that dependencies should point inward toward higher-level modules. By following this rule, you create a system where the core business logic of your application is decoupled from external dependencies. This makes it more adaptable to changes and easier to test.
      - Example
        
        ```csharp
        // Core (Inner Layer) - Business Rules
        public interface IUserRepository     // ◄── Interface defined here
        {                                   //     (pointing inward)
            User GetById(int id);
        }

        public class UserService    // Core business logic
        {
            private readonly IUserRepository _repository;

            public UserService(IUserRepository repository)
            {
                _repository = repository;
            }
        }

        // Infrastructure (Outer Layer) - Implementation Details
        public class SqlUserRepository : IUserRepository  // Implements interface from core
        {
            public User GetById(int id) 
            {
                // SQL implementation
            }
        }

        // Visualization of Dependency Flow:
        //
        // Outer Layer         │  Inner Layer
        // (Infrastructure)    │  (Core Business)
        //                     │
        // SqlUserRepository ──┼──► IUserRepository
        //                     │         ▲
        //                     │         │
        //                     │    UserService
    
- **Implementation Techniques**:
  - Constructor Injection
  - Property Injection
  - Method Injection

- **Benefits**:
  - Loose coupling
  - Easier testing
  - Flexible configuration
  - Better reusability
  - Simpler maintenance