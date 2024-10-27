APIE:
- Encapsulation
  - The principal that seeks to hide the implementation details of an object from the outside world
    - All important information is contained within the object, but only selected data is available externally
    - All the inner workings of the object are privately defined within the object, where other objects have no access to it
    - Instead, access and interaction to the object is allowed only through its public methods
    - This form of data hiding, provides program security and control over object state changes, reduces risk of errors, and make the program more understandable
  - MS Definition (C#): Hiding the internal state and functionality of an object and only allowing access through a set of public methods
  - Achieved: Through the user of access modifiers: private, protected, public, internal, protected internal.
  - Example:
    - ```csharp
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
                if (amount > 0)
                    balance += amount;
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
        account2.Withdraw(50);       // Can't go negative
        // account2.balance = -500;  // Won't compile - field is private

- Abstraction
  - Can be viewed as an expansion of encapsulation (Hiding the internal state and functionality of an object and allowing access and interaction ability only through a set of public methods)
  - High level: Exposing only the necessary details to the outside world, while hiding implementation details.
  - Helps focus on the system's essential elements and ignore the less important details that have no affect on its key features
  - Helps construct more understandable programs
  - MS definition (C#): Modelling the relevant attributes and interactions of entities as classes to define an abstract representation of a system
  - Achieved: Through abstract classes and interfaces
  - Example: Defining a class/interface that outlines methods without implementing them, allowing different implementations
  - Example: A program that contains thousands of lines. Through abstraction, the code in side becomes largely independent of other objects. For instance, in a program that stores information about movies, we can create a "Movie" class that provides access to only the most essential details, such as title, release year, and genre, while hiding the less important information, like shorts, or technical aspects.
  - Example: 
    - ```csharp
        // Interface - pure contract, NO implementations
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

            // Abstract method - must be implemented by child classes
            public abstract double CalculateRating();
            
            // Abstract method - must be implemented by child classes
            public abstract void Play();
        }

        // Concrete implementation using abstract class
        public class TheatricalMovie : MovieBase
        {
            private int boxOfficeEarnings;

            public TheatricalMovie(string title, int releaseYear, string genre) 
                : base(title, releaseYear, genre) { }

            public override double CalculateRating()
            {
                return boxOfficeEarnings * 0.5;
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

            public StreamingMovie(string title, int releaseYear, string genre)
            {
                Title = title;
                ReleaseYear = releaseYear;
                Genre = genre;
            }

            public double CalculateRating()
            {
                return streamCount * 0.1;
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