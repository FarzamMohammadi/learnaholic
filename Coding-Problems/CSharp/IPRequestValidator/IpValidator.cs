using System.Text;

namespace IPRequestValidator;

public class IpValidatorConfiguration
{
    public int MaximumAllowedIpOccurrence { get; }
    public int MaximumTotalAllowedRequests { get; }

    public IpValidatorConfiguration(int maxOccurrence, int maxRequests)
    {
        if (maxOccurrence <= 0) throw new ArgumentException("Must be positive!", nameof(maxOccurrence));
        if (maxRequests <= 0) throw new ArgumentException("Must be positive!", nameof(maxRequests));
        if (maxOccurrence > maxRequests) throw new ArgumentException("Max occurrence cannot exceed max requests.");

        MaximumAllowedIpOccurrence = maxOccurrence;
        MaximumTotalAllowedRequests = maxRequests;
    }
}

public interface IIpValidator
{
    public bool IsValid(byte[] ip);
}

public interface IIpStorage
{
    public int TotalCount { get; }
    public void Add(byte[] ip);
    public void RemoveLast();
    public int GetIpOccurenceCount(byte[] ip);
}

public class IpValidator
(
    IpValidatorConfiguration configuration,
    IIpStorage storage
) : IIpValidator
{
    private readonly IpValidatorConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IIpStorage _storage = storage ?? throw new ArgumentNullException(nameof(storage));

    public bool IsValid(byte[] ip)
    {
        ArgumentNullException.ThrowIfNull(ip);

        if (ip.Length != 4) throw new ArgumentException("Invalid IP address length.", nameof(ip));

        var occurenceCount = _storage.GetIpOccurenceCount(ip);

        if (occurenceCount >= _configuration.MaximumAllowedIpOccurrence) return false;

        _storage.Add(ip);

        if (_storage.TotalCount > _configuration.MaximumTotalAllowedRequests) _storage.RemoveLast();

        return true;
    }
}

public class IpStorage : IIpStorage
{
    private readonly object _lock = new();

    private readonly Queue<byte[]> _store;
    private readonly Dictionary<string, int> _ipOccurenceCount;

    public IpStorage(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        _store = new(capacity);
        _ipOccurenceCount = new(capacity);
    }

    /*
     Initial implementation: private readonly Dictionary<byte[], int> ipOccuranceCount = new();

     Issue with Dictionary with byte[] as key (major issue):
        - Arrays use reference equality
        - Two identical byte arrays will be considered different keys
        - Will cause memory leaks and incorrect counting
     */

    // Improved key generation with StringBuilder for better performance
    private static readonly ThreadLocal<StringBuilder> KeyBuilder = new(() => new StringBuilder(15));

    private static string GetIpKey(byte[] ip)
    {
        var builder = KeyBuilder.Value!;
        builder.Clear();

        // Format: xxx.xxx.xxx.xxx
        builder.Append(ip[0]).Append('.')
            .Append(ip[1]).Append('.')
            .Append(ip[2]).Append('.')
            .Append(ip[3]);

        return builder.ToString();
    }

    public int TotalCount
    {
        get
        {
            lock (_lock)
            {
                return _store.Count;
            }
        }
    }

    public void Add(byte[] ip)
    {
        ArgumentNullException.ThrowIfNull(ip);

        lock (_lock)
        {
            var key = GetIpKey(ip);

            _store.Enqueue((byte[])ip.Clone()); // Defensive copy

            /*
             A defensive copy `(byte[])ip.Clone()` is needed because byte[] is a reference type.
             Without cloning, multiple references could point to the same array, and if the original array is modified elsewhere, it would affect our stored value.

             Example:
                 byte[] ip = new byte[] {192, 168, 1, 1};
                 ipStorage.Add(ip);      // Stores reference to original array
                 ip[0] = 10;            // Changes original array
                 // Now the stored IP in our system is also changed to {10, 168, 1, 1}!
            */

            _ipOccurenceCount.TryGetValue(key, out var count);
            _ipOccurenceCount[key] = count + 1;
        }
    }

    public void RemoveLast()
    {
        lock (_lock)
        {
            if (TotalCount == 0) throw new InvalidOperationException("Ip storage is empty.");

            var lastIp = _store.Dequeue();
            var key = GetIpKey(lastIp);

            if (_ipOccurenceCount[key] <= 1) _ipOccurenceCount.Remove(key);
            else _ipOccurenceCount[key]--;
        }
    }

    public int GetIpOccurenceCount(byte[] ip)
    {
        ArgumentNullException.ThrowIfNull(ip);

        lock (_lock)
        {
            return _ipOccurenceCount.GetValueOrDefault(GetIpKey(ip));
        }
    }
}