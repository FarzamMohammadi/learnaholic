/*
Testing Approach:
Custom assertion implementation for testing purposes
Simulates basic unit testing functionality without requiring test framework dependencies
Common approach in coding interviews where external testing frameworks aren't available
*/

using System.ComponentModel.DataAnnotations;
using IPRequestValidator;

var config = new IpValidatorConfiguration(maxOccurrence: 5, maxRequests: 1_000_000);
var storage = new IpStorage(capacity: 1_000_000);
var validator = new IpValidator(config, storage);

// Test helpers
static byte[] CreateIp(byte a, byte b, byte c, byte d) => new byte[] { a, b, c, d };

// Test 1: Basic validation works
{
    var ip = CreateIp(192, 168, 1, 1);
    Assert(validator.IsValid(ip));
    Assert(storage.GetIpOccurenceCount(ip) == 1);
}

// Test 2: Multiple occurrences of same IP
{
    var ip = CreateIp(192, 168, 1, 2);
    for (int i = 0; i < 5; i++)
    {
        Assert(validator.IsValid(ip));
    }

    Assert(!validator.IsValid(ip)); // 6th attempt should fail
    Assert(storage.GetIpOccurenceCount(ip) == 5);
}

// Test 3: Different IPs don't affect each other's counts
{
    var ip1 = CreateIp(192, 168, 1, 3);
    var ip2 = CreateIp(192, 168, 1, 4);

    Assert(validator.IsValid(ip1));
    Assert(validator.IsValid(ip2));
    Assert(storage.GetIpOccurenceCount(ip1) == 1);
    Assert(storage.GetIpOccurenceCount(ip2) == 1);
}

// Test 4: Window sliding works (needs smaller config for testing)
{
    var smallConfig = new IpValidatorConfiguration(maxOccurrence: 2, maxRequests: 3);
    var smallStorage = new IpStorage(capacity: 3);
    var smallValidator = new IpValidator(smallConfig, smallStorage);

    var ip1 = CreateIp(192, 168, 1, 5);
    var ip2 = CreateIp(192, 168, 1, 6);

    Assert(smallValidator.IsValid(ip1)); // Add ip1
    Assert(smallValidator.IsValid(ip2)); // Add ip2
    Assert(smallValidator.IsValid(ip1)); // Add ip1 again
    Assert(smallValidator.IsValid(ip2)); // Add ip2, should remove first ip1
    Assert(smallStorage.GetIpOccurenceCount(ip1) == 1); // First occurrence was removed
}

// Test 5: Edge cases - IP array validation
try
{
    validator.IsValid(new byte[] { 192, 168, 1 }); // Too short
    Assert(false); // Should not reach here
}
catch (ArgumentException)
{
    Assert(true);
}

// Test 6: Defensive copy works
{
    var ip = CreateIp(192, 168, 1, 7);
    Assert(validator.IsValid(ip));
    ip[0] = 10; // Modify original array
    var newIp = CreateIp(10, 168, 1, 7);
    Assert(storage.GetIpOccurenceCount(newIp) == 0); // Should not find modified IP
}

// Test 7: Configuration validation
try
{
    new IpValidatorConfiguration(maxOccurrence: 6, maxRequests: 5);
    Assert(false); // Should not reach here
}
catch (ArgumentException)
{
    Assert(true);
}

// Test 8: Concurrent access (basic test)
{
    var ip = CreateIp(192, 168, 1, 8);
    var tasks = Enumerable.Range(0, 3).Select(_ =>
        Task.Run(() => validator.IsValid(ip))
    );
    Task.WaitAll(tasks.ToArray());
    Assert(storage.GetIpOccurenceCount(ip) == 3);
}

Console.WriteLine("All tests passed!");

static void Assert(bool condition)
{
    if (condition is not true) throw new ValidationException($"Expected {true} but got {false}");
}