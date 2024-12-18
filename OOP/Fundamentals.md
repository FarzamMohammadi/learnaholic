# APIE: OOP Fundamentals

## Encapsulation

- **Definition**: Encapsulation is the principle of hiding the internal state and functionality of an object and only allowing access and interaction through a set of public methods or properties. It restricts direct access to some of an object's components, meaning the internal representation of an object **cannot** be seen from outside the object's definition.

- **Key Points**:
  - All important information is contained within the object, but only selected data is exposed externally.
  - The inner workings of the object are privately defined and inaccessible to other objects.
  - Interaction with the object is only possible through its public methods or properties.

- **Goals**:
  - Increase program security by preventing unauthorized access to data.
  - Control over object state changes to maintain data integrity.
  - Reduce errors by limiting the interdependencies between components.
  - Make the program more intuitive and easier to maintain.

- **Achieved Through**:
  - Use of access modifiers: `private`, `protected`, `public`, `internal`, `protected internal`.

- **Example**:

    ```csharp
    // Without encapsulation
    public class BankAccountNoEncapsulation
    {
        public decimal balance = 0; // Direct access to field
    }

    // Usage:
    var account1 = new BankAccountNoEncapsulation();
    account1.balance = -500; // Can set invalid negative balance directly

    // With encapsulation
    public class BankAccount
    {
        private decimal balance = 0; // Private field

        public decimal Balance
        {
            get { return balance; }
            private set { balance = value; } // Only class can modify
        }

        public void Deposit(decimal amount)
        {
            if (amount > 0) balance += amount;
        }

        public bool Withdraw(decimal amount)
        {
            if (amount > 0 && balance >= amount)
            {
                balance -= amount;
                return true;
            }
            return false;
        }
    }

    // Usage:
    var account2 = new BankAccount();
    account2.Deposit(100);       // Must use methods
    account2.Withdraw(50);       // Can't withdraw more than balance
    // account2.balance = -500;  // Error: 'balance' is inaccessible due to its protection level

## Abstraction
- **Definition**: Abstraction focuses on exposing only the necessary details to the outside world while hiding the internal implementation details. It allows developers to work with complex systems by modeling classes appropriate to the problem domain.

- **Key Points**:
  - Helps in managing complexity by hiding irrelevant details.
  - Allows focusing on what an object does instead of how it does it.
  - Achieved through abstract classes and interfaces.

- **Goals**:
    - Make the program easier to understand and maintain.
    - Reduce complexity by providing a simplified model of the system.

- **Example**:

    ```csharp
    // Interface - pure contract, no implementations
    public interface IMovie
    {
        string Title { get; }
        int ReleaseYear { get; }
        string Genre { get; }

        double CalculateRating();
        string GetMovieInfo();
        void Play();
    }

    // Abstract class - mix of implemented and abstract methods
    public abstract class MovieBase
    {
        // Implemented properties with backing fields
        public string Title { get; private set; }
        public int ReleaseYear { get; private set; }
        public string Genre { get; private set; }

        // Constructor - can have in abstract class
        protected MovieBase(string title, int releaseYear, string genre)
        {
            Title = title;
            ReleaseYear = releaseYear;
            Genre = genre;
        }

        // Implemented method - shared by all movies
        public string GetMovieInfo()
        {
            return $"{Title} ({ReleaseYear}) - {Genre}";
        }

        // Abstract methods - must be implemented by derived classes
        public abstract double CalculateRating();
        public abstract void Play();
    }

    // Concrete implementation using abstract class
    public class TheatricalMovie : MovieBase
    {
        private int boxOfficeEarnings;

        public TheatricalMovie(string title, int releaseYear, string genre, int boxOfficeEarnings)
            : base(title, releaseYear, genre)
        {
            this.boxOfficeEarnings = boxOfficeEarnings;
        }

        public override double CalculateRating()
        {
            // Example calculation
            return boxOfficeEarnings * 0.000001;
        }

        public override void Play()
        {
            Console.WriteLine("Playing in theatre...");
        }
    }

    // Concrete implementation using interface
    public class StreamingMovie : IMovie
    {
        public string Title { get; private set; }
        public int ReleaseYear { get; private set; }
        public string Genre { get; private set; }
        private int streamCount;

        public StreamingMovie(string title, int releaseYear, string genre, int streamCount)
        {
            Title = title;
            ReleaseYear = releaseYear;
            Genre = genre;
            this.streamCount = streamCount;
        }

        public double CalculateRating()
        {
            // Example calculation
            return streamCount * 0.0001;
        }

        public string GetMovieInfo()
        {
            return $"{Title} ({ReleaseYear}) - {Genre}";
        }

        public void Play()
        {
            Console.WriteLine("Streaming online...");
        }
    }

    // Usage:
    var theatricalMovie = new TheatricalMovie("Inception", 2010, "Sci-Fi", 800000000);
    var streamingMovie = new StreamingMovie("Warrior", 2011, "Drama", 50000000);

    Console.WriteLine(theatricalMovie.GetMovieInfo());
    theatricalMovie.Play();
    Console.WriteLine($"Rating: {theatricalMovie.CalculateRating()}");

    Console.WriteLine(streamingMovie.GetMovieInfo());
    streamingMovie.Play();
    Console.WriteLine($"Rating: {streamingMovie.CalculateRating()}");

    /*
    Output:
    Inception (2010) - Sci-Fi
    Playing in theatre...
    Rating: 800
    Warrior (2011) - Drama
    Streaming online...
    Rating: 5000
    */

## Inheritance
- **Definition**: Inheritance allows a new class (derived/child class) to gain the methods and properties of an existing class (base/parent class), promoting code reusability and hierarchical classifications.

- **Types of Inheritance in C#**:
    - **Single Inheritance**: A class inherits from one base class.
    - **Multilevel Inheritance**: A class inherits from a derived class, forming a hierarchy.
    - **Hierarchical Inheritance**: Multiple classes inherit from the same base class.
    >Note: C# does not support multiple inheritance of classes (a class cannot inherit from more than one base class), but it can implement multiple interfaces.

- **Goals**:
    - Reduce code duplication by reusing existing code.
    - Simplify maintenance by having a centralized base class.
    - Create a natural and logical hierarchical relationship between classes.

- **Example**:

    ```csharp
    // Base class (parent)
    public class Vehicle
    {
        public string Brand { get; set; }
        public int Year { get; set; }

        public virtual void Start()
        {
            Console.WriteLine($"{Brand} vehicle starting...");
        }
    }

    // Single Inheritance: Car inherits from Vehicle
    public class Car : Vehicle
    {
        public int NumberOfDoors { get; set; }

        public override void Start()
        {
            Console.WriteLine($"{Brand} car engine starting...");
        }
    }

    // Single Inheritance: Motorcycle inherits from Vehicle
    public class Motorcycle : Vehicle
    {
        public bool HasSidecar { get; set; }

        public override void Start()
        {
            Console.WriteLine($"{Brand} motorcycle revving up...");
        }
    }

    // Multilevel Inheritance: ElectricCar inherits from Car
    public class ElectricCar : Car
    {
        public int BatteryCapacity { get; set; }

        public override void Start()
        {
            Console.WriteLine($"{Brand} electric car powering up...");
        }
    }

    // Hierarchical Inheritance: SportsCar and SUV inherit from Car
    public class SportsCar : Car
    {
        public bool HasTurbo { get; set; }

        public override void Start()
        {
            Console.WriteLine($"{Brand} sports car roaring to life...");
        }
    }

    public class SUV : Car
    {
        public bool HasAllWheelDrive { get; set; }

        public override void Start()
        {
            Console.WriteLine($"{Brand} SUV engine starting...");
        }
    }

    // Usage
    public void Example()
    {
        // Single inheritance example
        Car car = new Car
        {
            Brand = "Toyota",
            Year = 2020,
            NumberOfDoors = 4
        };

        // Multilevel inheritance example
        ElectricCar tesla = new ElectricCar
        {
            Brand = "Tesla",
            Year = 2023,
            NumberOfDoors = 4,
            BatteryCapacity = 100
        };

        // Hierarchical inheritance examples
        SportsCar ferrari = new SportsCar
        {
            Brand = "Ferrari",
            Year = 2023,
            NumberOfDoors = 2,
            HasTurbo = true
        };

        SUV rangeRover = new SUV
        {
            Brand = "Range Rover",
            Year = 2023,
            NumberOfDoors = 4,
            HasAllWheelDrive = true
        };

        // Demonstrating polymorphism
        Vehicle[] vehicles = { car, tesla, ferrari, rangeRover };
        foreach (var vehicle in vehicles)
        {
            vehicle.Start();
        }
    }

    /*
        Output:
        Toyota car engine starting...
        Tesla electric car powering up...
        Ferrari sports car roaring to life...
        Range Rover SUV engine starting...
    */
    
    /*
                     Vehicle
                    /       \
                  Car     Motorcycle
                /  |  \
               /   |   \
         SportsCar |  SUV
                   |
                ElectricCar
                
    Single Inheritance: Car and Motorcycle inherit from Vehicle.
    Multilevel Inheritance: ElectricCar inherits from Car, which inherits from Vehicle.
    Hierarchical Inheritance: SportsCar, SUV, and ElectricCar all inherit from Car. */
    ```

## Polymorphism

- **Definition**: Polymorphism (meaning "many forms") is the principle that allows objects to be treated as instances of their parent class or interface while maintaining their unique behaviors. It enables you to:

1. Write code that works with objects based on their shared interfaces rather than specific implementations
2. Define multiple methods with the same name but different parameters (compile-time)
3. Allow different classes to provide their own implementation of the same method (runtime)

This principle is fundamental to achieving loose coupling, enabling code reuse, and making systems more flexible and maintainable.

- **Key Points**:
  - **Compile-time Polymorphism (Method Overloading)**:
    - Same method name with different signatures (parameters)
    - Resolved at compile time based on method signatures
    - Also known as static binding or early binding
  
  - **Runtime Polymorphism**:
    - Also known as dynamic binding or late binding
    - Resolved at runtime based on the object's actual type
    - Implemented through:
      - **Method Overriding**:
        - Derived classes override methods of the base class using the `virtual` and `override` keywords
      - **Interface-based Polymorphism**:
        - Allows for swapping implementations without changing dependent code
        - Enables code to be flexible and extensible
        - Fundamental for adhering to the Dependency Inversion Principle

- **Goals**:
  - Enable code flexibility by treating objects through shared interfaces
  - Promote code reuse through shared implementations
  - Achieve loose coupling between components
  - Make code easier to test through interface-based design
  - Support extension without modifying existing code
  - Reduce maintenance costs through better organization

- **Example 1: Compile-time Polymorphism (Method Overloading)**
    ```csharp
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public double Add(double a, double b)
        {
            return a + b;
        }

        public int Add(int a, int b, int c)
        {
            return a + b + c;
        }
    }

    // Usage:
    var calculator = new Calculator();
    int sum1 = calculator.Add(1, 2);        // Calls Add(int, int)
    double sum2 = calculator.Add(1.5, 2.5); // Calls Add(double, double)
    int sum3 = calculator.Add(1, 2, 3);     // Calls Add(int, int, int)
    ```

    - **Examples: Runtime Polymorphism**

    **1. Method Overriding:**
    ```csharp
    public class Animal
    {
        public virtual void Speak()
        {
            Console.WriteLine("The animal makes a sound.");
        }
    }

    public class Dog : Animal
    {
        public override void Speak()
        {
            Console.WriteLine("The dog barks.");
        }
    }

    public class Cat : Animal
    {
        public override void Speak()
        {
            Console.WriteLine("The cat meows.");
        }
    }

    // Usage:
    Animal myAnimal = new Animal();
    Animal myDog = new Dog();
    Animal myCat = new Cat();

    myAnimal.Speak(); // Output: The animal makes a sound.
    myDog.Speak();    // Output: The dog barks.
    myCat.Speak();    // Output: The cat meows.
    ```

- **Example 2: Interface-based Polymorphism:**
    ```csharp
    // Interface defining the contract
    public interface INotificationService
    {
        void Send(string message);
    }

    // First implementation using Email
    public class EmailNotificationService : INotificationService
    {
        public void Send(string message)
        {
            // Code to send email
            Console.WriteLine($"Email sent: {message}");
        }
    }

    // New implementation using SMS
    public class SmsNotificationService : INotificationService
    {
        public void Send(string message)
        {
            // Code to send SMS
            Console.WriteLine($"SMS sent: {message}");
        }
    }

    // Client code depending on the abstraction
    public class NotificationManager
    {
        private readonly INotificationService _notificationService;

        // Constructor Injection
        public NotificationManager(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Notify(string message)
        {
            _notificationService.Send(message);
        }
    }

    // Usage
    public void Example()
    {
        // Using EmailNotificationService
        INotificationService emailService = new EmailNotificationService();
        NotificationManager managerEmail = new NotificationManager(emailService);
        managerEmail.Notify("Hello via Email!");

        // Switching to SmsNotificationService without changing NotificationManager
        INotificationService smsService = new SmsNotificationService();
        NotificationManager managerSms = new NotificationManager(smsService);
        managerSms.Notify("Hello via SMS!");
    }

    /*
        Output:
        Email sent: Hello via Email!
        SMS sent: Hello via SMS!
    */
    ```

## SOLID - 
**Created By: Michael Feathers**
**Promoted By: Uncle Bob**

### Single Responsibility Principle (SRP)
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

### Open/Closed Principle (OCP)
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

### Liskov Substitution Principle (LSP)
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

### Interface Segregation Principle (ISP)
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

### Dependency Inversion Principle (DIP) 
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