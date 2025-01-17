# [From Primitive Obsession to Domain Modelling by Mark Seemann](https://blog.ploeh.dk/2015/01/19/from-primitive-obsession-to-domain-modelling/)
Rating: 9/10

This article by Mark Seemann discusses the importance of moving from primitive types to domain models, using a practical example of username validation.

## The Problem
The article starts with a simple controller action using string validation:
- Basic username validation using `IsNullOrWhiteSpace`
- Business rules scattered in controllers
- No clear home for validation logic

## Where Should Business Rules Go?
The article raises important questions:
- Should validation live in controllers?
- Must we remember to validate usernames everywhere?
- Shouldn't business rules belong in the Domain Model?

The core issue identified is **Primitive Obsession** - using a string to represent something that isn't really just any string. A username has specific rules and constraints that a basic string doesn't capture.

## The Solution: Make It a Type
Instead of using raw strings, create a dedicated type that:
- Encapsulates all business rules
- Validates at creation
- Ensures consistent behavior
- Eliminates rule duplication

Implementation shown via code:

**Initial problematic version**

```csharp
public IHttpActionResult Get(string userName)
{
    if (string.IsNullOrWhiteSpace(userName))
        return this.BadRequest("Invalid user name.");
 
    var user = this.repository.FindUser(userName.ToUpper());
    return this.Ok(user);
}
```

**UserName class implementation**

```csharp
public class UserName
{
    private readonly string value;
 
    public UserName(string value)
    {
        if (value == null)
            throw new ArgumentNullException("value");
        if (!UserName.IsValid(value))
            throw new ArgumentException("Invalid value.", "value");
 
        this.value = value;
    }
 
    public static bool IsValid(string candidate)
    {
        if (string.IsNullOrEmpty(candidate))
            return false;
 
        return candidate.Trim().ToUpper() == candidate;
    }
 
    public static bool TryParse(string candidate, out UserName userName)
    {
        userName = null;
        if (string.IsNullOrWhiteSpace(candidate))
            return false;
 
        userName = new UserName(candidate.Trim().ToUpper());
        return true;
    }
 
    public static implicit operator string(UserName userName)
    {
        return userName.value;
    }
 
    public override string ToString()
    {
        return this.value.ToString();
    }
 
    public override bool Equals(object obj)
    {
        var other = obj as UserName;
        if (other == null)
            return base.Equals(obj);
 
        return object.Equals(this.value, other.value);
    }
 
    public override int GetHashCode()
    {
        return this.value.GetHashCode();
    }
}

Note: The use of ToUpper is due to a business rule requiring usernames to be case-insensitive.
```

**Refactored controller using UserName type**

```csharp
public IHttpActionResult Get(string candidate)
{
    UserName userName;
    if (!UserName.TryParse(candidate, out userName))
        return this.BadRequest("Invalid user name.");
 
    var user = this.repository.FindUser(userName);
    return this.Ok(user);
}
```

**Domain model usage in repository interface**

```csharp
User FindUser(UserName userName);
```

## Key Benefits
- Business rules in one place
- Validation guaranteed at creation
- Clear domain expression
- Type safety
- Consistent behavior

## Over-engineering Concerns
The article addresses the common "isn't this over-engineering?" question:
- 15 minutes to implement with tests vs. 0 minutes for strings
- Long-term benefits outweigh initial setup
- Reduces scattered validation logic
- Prevents inconsistent rule application
- Less time spent on bug fixes

The article concludes by noting this approach isn't limited to OOP and can benefit various programming paradigms.