#include <iomanip>
#include <iostream>
#include <vector>

using namespace std;

/**
 * BINARY SEARCH QUICK REFERENCE
 * ----------------------------
 * Key Points to Remember:
 * 1. Array MUST be sorted first
 * 2. Returns index of element or -1 if not found
 * 3. Time complexity: O(log n)
 * 4. Three implementations demonstrated:
 *    - Halving: Traditional approach with left/right pointers
 *    - Jumping: Alternative approach with decreasing jump sizes
 *    - Insertion Point: Finds where element should be inserted to maintain sort order
 */

/**
 * Classic Binary Search - Halving Method
 * ------------------------------------
 * How it works:
 * 1. Start with full array (left = 0, right = length-1)
 * 2. Find middle point
 * 3. If target found at middle, return index
 * 4. If target > middle, search right half
 * 5. If target < middle, search left half
 * 6. Repeat until element found or search space exhausted
 *
 * Key implementation details:
 * - Use (left + right)/2 for middle, but beware integer overflow
 * - Loop condition is left <= right
 * - Update left = mid + 1 or right = mid - 1
 */
class IterativeHalvingBinarySearcher
{
public:
    static int search(const vector<int>& arr, int target)
    {
        if (arr.empty()) return -1;

        int left = 0;
        int right = arr.size() - 1;

        while (left <= right)
        {
            // Calculate middle - avoid overflow with this formula
            int mid = left + (right - left) / 2;

            if (arr[mid] == target)
            {
                return mid; // Found the target
            }

            if (arr[mid] < target)
            {
                left = mid + 1; // Target is in right half
            }
            else
            {
                right = mid - 1; // Target is in left half
            }
        }

        return -1; // Target not found
    }

    /**
     * Binary Search Insertion Point Variant
     * ----------------------------------
     * How it works:
     * 1. Start with left = 0, right = array_size (not size-1!)
     * 2. Find middle point
     * 3. If middle element < target, search right half
     * 4. If middle element >= target, search left half
     * 5. When left == right, that's our insertion point
     *
     * Key differences from regular binary search:
     * - Right pointer starts at array_size, not array_size-1
     * - Loop condition is left < right (not left <= right)
     * - Returns position where element should be inserted
     * - Works even when element isn't in array
     *
     * Common uses:
     * - Finding where to insert element in sorted array
     * - Finding lower bound of element in array with duplicates
     * - Finding first position where element should go
     *
     * Example:
     * Array: [1, 3, 5, 7]
     * Target: 4
     * Returns: 2 (inserting at index 2 maintains sort order)
     */
    static int findInsertionPoint(const vector<int>& arr, int target)
    {
        int left = 0;
        int right = arr.size(); // Note: Not size-1!

        while (left < right)
        { // Note: Different condition!
            int mid = left + (right - left) / 2;

            if (arr[mid] < target)
            {
                left = mid + 1;
            }
            else
            {
                right = mid; // Note: Not mid-1!
            }
        }

        return left; // This is our insertion point
    }
};

/**
 * Alternative Binary Search - Jumping Method
 * ---------------------------------------
 * How it works:
 * 1. Start with large jump size (half of array)
 * 2. While current element < target, keep jumping
 * 3. When we overshoot, reduce jump size by half
 * 4. Repeat until jump size becomes 0
 *
 * Key implementation details:
 * - Jump size starts at array_size/2
 * - Each iteration divides jump by 2
 * - Keep track of current position with sum
 * - Check bounds before each jump
 */
class IterativeJumpingBinarySearcher
{
public:
    static int search(const vector<int>& arr, int target)
    {
        if (arr.empty()) return -1;

        int currentPosition = 0; // Keep track of where we are

        // Start with largest jump = array_size/2
        for (int jumpSize = arr.size() / 2; jumpSize >= 1; jumpSize /= 2)
        {
            // Keep jumping while we can and haven't passed target
            while (currentPosition + jumpSize < arr.size() && arr[currentPosition + jumpSize] <= target)
            {
                currentPosition += jumpSize;
            }
        }

        // Check if we found the target
        return (arr[currentPosition] == target) ? currentPosition : -1;
    }
};

/**
 * Test Cases
 * ---------
 * Includes tests for:
 * 1. Empty array
 * 2. Single element
 * 3. Element at start/middle/end
 * 4. Element not in array
 * 5. Duplicate elements
 * 6. Negative numbers
 * 7. Insertion points for various scenarios
 */
void runTests()
{
    // Test case arrays
    vector<vector<int>> testArrays = {
        {},                   // Empty array
        {1},                  // Single element
        {1, 1, 1, 1},         // All same elements
        {1, 3, 5, 7, 9},      // Odd length
        {-10, -5, 0, 5, 10},  // Mixed negative/positive
        {1, 2, 2, 2, 3, 4, 5} // With duplicates
    };

    // Test values to search for in each array
    vector<vector<int>> testValues = {
        {0},                // Test empty array
        {1, 0, 2},          // Test single element array
        {1, 2},             // Test duplicate array
        {1, 5, 9, 0, 4, 8}, // Test odd length array (including insertion points)
        {-10, 0, 10, 3},    // Test negative/positive array
        {2, 1, 5, 6}        // Test duplicates array
    };

    // Run tests for both search implementations and insertion point
    for (size_t i = 0; i < testArrays.size(); i++)
    {
        cout << "\nTesting array: ";
        for (int num : testArrays[i])
        {
            cout << num << " ";
        }
        cout << endl;

        for (int target : testValues[i])
        {
            cout << "\nValue " << target << ":\n";

            // Test regular search methods
            int halvingResult = IterativeHalvingBinarySearcher::search(testArrays[i], target);
            cout << "Search result (halving): " << halvingResult << endl;

            int jumpingResult = IterativeJumpingBinarySearcher::search(testArrays[i], target);
            cout << "Search result (jumping): " << jumpingResult << endl;

            // Test insertion point
            int insertPoint = IterativeHalvingBinarySearcher::findInsertionPoint(testArrays[i], target);
            cout << "Insertion point: " << insertPoint << " (would go between indices " << insertPoint - 1 << " and " << insertPoint << ")" << endl;
        }
    }
}

int main()
{
    cout << "Binary Search Algorithm Testing\n";
    cout << "==============================\n";

    runTests();

    return 0;
}