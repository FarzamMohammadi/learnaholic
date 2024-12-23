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