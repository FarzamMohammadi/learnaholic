#include <iostream>
#include <string>
#include <vector>

using namespace std;

class RecursiveBinarySearcher
{
public:
    static int search(const vector<int>& arr, int left, int right, int target)
    {
        if (arr.empty() || left > right) return -1;

        int mid = left + (right - left) / 2;

        if (arr[mid] == target) return mid;

        // Target is smaller than current mid point value
        // Search left subarray
        if (arr[mid] > target) return search(arr, left, mid - 1, target);

        // Target is larger than mid point value
        // Search right subarray
        return search(arr, mid + 1, right, target);
    }
};

void printTestResult(const string& testName, const vector<int>& arr, int target, int result)
{
    cout << "\nTest: " << testName << "\n";
    cout << "Array: [";
    for (size_t i = 0; i < arr.size(); i++)
    {
        cout << arr[i];
        if (i < arr.size() - 1) cout << ", ";
    }
    cout << "]\n";
    cout << "Target: " << target << "\n";
    cout << "Result Index: " << result;
    if (result != -1)
    {
        cout << " (Found " << arr[result] << " at index " << result << ")";
    }
    else
    {
        cout << " (Not found in array)";
    }
    cout << "\n";
}

void runTests()
{
    // Test case 1: Empty array
    {
        vector<int> arr;
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 5);
        printTestResult("Empty array", arr, 5, result);
    }

    // Test case 2: Single element array - element exists
    {
        vector<int> arr = {5};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 5);
        printTestResult("Single element (exists)", arr, 5, result);
    }

    // Test case 3: Single element array - element doesn't exist
    {
        vector<int> arr = {5};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 3);
        printTestResult("Single element (doesn't exist)", arr, 3, result);
    }

    // Test case 4: Multiple elements - target at start
    {
        vector<int> arr = {1, 3, 5, 7, 9};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 1);
        printTestResult("Target at start", arr, 1, result);
    }

    // Test case 5: Multiple elements - target in middle
    {
        vector<int> arr = {1, 3, 5, 7, 9};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 5);
        printTestResult("Target in middle", arr, 5, result);
    }

    // Test case 6: Multiple elements - target at end
    {
        vector<int> arr = {1, 3, 5, 7, 9};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 9);
        printTestResult("Target at end", arr, 9, result);
    }

    // Test case 7: Multiple elements - target doesn't exist (too small)
    {
        vector<int> arr = {1, 3, 5, 7, 9};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 0);
        printTestResult("Target too small", arr, 0, result);
    }

    // Test case 8: Multiple elements - target doesn't exist (too large)
    {
        vector<int> arr = {1, 3, 5, 7, 9};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 10);
        printTestResult("Target too large", arr, 10, result);
    }

    // Test case 9: Multiple elements - target doesn't exist (between elements)
    {
        vector<int> arr = {1, 3, 5, 7, 9};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 4);
        printTestResult("Target between elements", arr, 4, result);
    }

    // Test case 10: Array with duplicate elements - find first occurrence
    {
        vector<int> arr = {1, 2, 2, 2, 3, 4, 5};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 2);
        printTestResult("Duplicate elements", arr, 2, result);
    }

    // Test case 11: Array with negative numbers
    {
        vector<int> arr = {-10, -5, 0, 5, 10};
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, -5);
        printTestResult("Negative numbers", arr, -5, result);
    }

    // Test case 12: Large array with many elements
    {
        vector<int> arr;
        for (int i = 0; i < 100; i += 2)
        {
            arr.push_back(i);
        }
        int result = RecursiveBinarySearcher::search(arr, 0, arr.size() - 1, 76);
        printTestResult("Large array", arr, 76, result);
    }
}

int main()
{
    cout << "Recursive Binary Search Algorithm Testing\n";
    cout << "========================================\n";
    runTests();
    return 0;
}