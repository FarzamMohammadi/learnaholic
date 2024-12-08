#include <iostream>
#include <vector>
#include <climits>

using namespace std;

class MaxSubarraySolutions
{
private:
    void printArray(const vector<int> &nums)
    {
        /*
            Stream Insertion Operator: `<<`
            When used with streams (like `cout`), it becomes the insertion operator. "Inserting" data into the stream.
            Can be thought of as an arrow showing the direction of the data flow.

            The context determines which behavior you get:
                - With numbers in arithmetic expressions: bitwise shift
                    ```
                    int x = 5;          // Binary: 0101
                    int result = x << 1; // Binary: 1010 (value: 10)
                    ```
                - With streams (cout, ofstream, etc.): stream insertion
        */
        cout << "Array: [";

        /*
            size_t = An unsigned integer type in C/C++ that's specifically designed to represent sizes and counts

            1. Guaranteed to be big enough to hold the size of the largest possible object
                - able to represent the maximum size of any object that can be allocated in memory
            2. The "Guaranteed" size varies by platform
                - 32-bit system = 32 bits (4 bytes)
                - 64-bit system = 64 bits (8 bytes)
            3. Commonly used for array indexing & loop counting
            4. Unsigned, hence, cannot be negative
        */
        for (size_t i = 0; i < nums.size(); i++)
        {
            cout << nums[i];

            if (i < nums.size() - 1)
                cout << ", ";
        }

        cout << "]" << endl;
    }

public:
    /*
        Reason for using `int` instead of `size_t` for array indexing:

        While `size_t` is technically the correct choice, using `int` is often more common in algorithm solutions because:
            1. Problem constraints usually guarantee array size fits in `int`
            2. Some algorithms may need negative indices/comparisons
            3. It's more concise

        Best practices:
            - Use `size_t` for general purpose array traversal and size management
            - Use `int` when working with algorithms that may require negative numbers
                OR when problem constraints guarantee int size is sufficient
        */

    // Solution 1: Brute Force O(n^2)
    int bruteForce(vector<int> &nums)
    {
        int maxSum = INT_MIN;

        for (int i = 0; i < nums.size(); i++)
        {
            int currentSum = 0;

            for (int j = i; j < nums.size(); j++)
            {
                currentSum += nums[j];
                maxSum = max(maxSum, currentSum);
            }
        }

        return maxSum;
    }

    // Solution 2: Kadane's Algorithm O(n)
    int kadane(vector<int> &nums)
    {
        int maxSum = 0;
        int sum = 0;

        for (int i = 0; i < nums.size(); i++)
        {
            sum = max(nums[i], (nums[i] + sum));

            maxSum = max(maxSum, sum);
        }

        return maxSum;
    }

public:
    // Test all solutions
    void testAllSolutions(vector<int> &nums)
    {
        cout << "\nMaximum Subarray Problem" << endl;
        cout << "------------------------" << endl;
        printArray(nums);

        cout << "\nResults:" << endl;
        cout << "1. Brute Force: " << bruteForce(nums) << endl;
        cout << "   Time Complexity: O(n^2)" << endl;

        cout << "\n2. Kadane's Algorithm: " << kadane(nums) << endl;
        cout << "   Time Complexity: O(n)" << endl;
    }
};

/*
    Performance comparison (runtime in seconds for different array sizes):
        n     |     BFO(n^2)      |     Kadane O(n)
        10^2  |       0.0         |         0.0
        10^3  |       0.0         |         0.0
        10^4  |       0.1         |         0.0
        10^5  |       5.3         |         0.0
        10^6  |      >10.0        |         0.0
        10^7  |      >10.0        |         0.0
*/

int main()
{
    vector<int> nums = {-2, 1, -3, 4, -1, 2, 1, -5, 4};
    MaxSubarraySolutions solutions;
    solutions.testAllSolutions(nums);
    return 0;
}