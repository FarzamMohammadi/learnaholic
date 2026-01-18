#include <iomanip>
#include <iostream>
#include <vector>

using namespace std;

/*
    Quick Sort Properties:
    - Divide and Conquer algorithm that selects a pivot and partitions array around it
    - Not stable by default: may change relative order of equal elements
    - Properties:
        * In-place: requires only O(log n) extra space for recursion
        * Internal sorting: all sorting done in memory
        * Parallelizable: different partitions can be sorted independently
        * Adaptive: performance varies based on pivot selection and input order
*/

/*
    QuickSorter::sort() - Quick Sort Visualization
    Example with array [64, 34, 25, 12, 22, 11, 90]

    QuickSort Partitioning Process:
    [64, 34, 25, 12, 22, 11, 90]                    // Initial array, pivot = 90
                    |
    [64, 34, 25, 12, 22, 11] | [90]                 // After first partition
                    |
    [34, 25, 12, 22, 11] | [64] | [90]              // Partition left, pivot = 64
                    |
    [25, 12, 22, 11] | [34] | [64] | [90]           // Continue, pivot = 34
                    |
    [12, 22, 11] | [25] | [34] | [64] | [90]        // Next partition, pivot = 25
                    |
    [11] | [12, 22] | [25] | [34] | [64] | [90]     // Continue, pivot = 22
                    |
    [11] | [12] | [22] | [25] | [34] | [64] | [90]  // Final partitioning

    Final sorted array:
    [11, 12, 22, 25, 34, 64, 90]                    // All elements in position

    Key Operations:
    1. Partitioning: O(n) comparisons at each level
    2. Recursion: O(log n) levels on average
    3. Total: O(n log n) average case comparisons

    Space Complexity Analysis:
    - No extra array needed (in-place)
    - Recursion stack: O(log n) average case
    - Worst case: O(n) recursion stack for unbalanced partitions

    Advantages:
    1. In-place sorting (minimal extra space)
    2. Cache-friendly (good locality of reference)
    3. Very fast in practice, especially on random data
    4. Adaptive to input (works well with partially sorted arrays)

    Disadvantages:
    1. Not stable (equal elements may change order)
    2. O(n²) worst case with poor pivot selection
    3. Performance depends heavily on pivot choice
    4. Deep recursion in worst case
*/

class QuickSorter
{
public:
    static void sort(vector<int>& arr)
    {
        if (arr.empty()) return;

        sort(arr, 0, arr.size() - 1);
    }

private:
    static void sort(vector<int>& arr, int low, int high)
    {
        if (low >= high) return; // Base case: 0 or 1 element

        // Partition array and get pivot position
        int pivot = partition(arr, low, high);

        // Recursively sort sub-arrays
        sort(arr, low, pivot - 1);  // Sort left of pivot
        sort(arr, pivot + 1, high); // Sort right of pivot
    }

    static int partition(vector<int>& arr, int low, int high)
    {
        int pivot = arr[high]; // Choose rightmost element as pivot

        int i = low - 1; // Index of smaller element

        // Place elements smaller than pivot to the left
        for (int j = low; j < high; j++)
        {
            if (arr[j] <= pivot)
            {
                i++;
                swap(arr[i], arr[j]);
            }
        }

        // Place pivot in its final position
        swap(arr[i + 1], arr[high]);

        return i + 1;
    }
};

void printArray(const vector<int>& arr, const string& label)
{
    cout << label << ": ";
    for (int num : arr) cout << setw(3) << num << " ";
    cout << endl;
}

int main()
{
    // Example 1: Basic sorting
    vector<int> arr1 = {64, 34, 25, 12, 22, 11, 90};
    cout << "Example 1 - Basic sorting\n";
    cout << "-----------------------\n";
    printArray(arr1, "Before");
    QuickSorter::sort(arr1);
    printArray(arr1, "After ");
    cout << endl;

    // Example 2: Already sorted array
    vector<int> arr2 = {1, 2, 3, 4, 5};
    cout << "Example 2 - Already sorted array\n";
    cout << "------------------------------\n";
    printArray(arr2, "Before");
    QuickSorter::sort(arr2);
    printArray(arr2, "After ");
    cout << endl;

    // Example 3: Reverse sorted array
    vector<int> arr3 = {5, 4, 3, 2, 1};
    cout << "Example 3 - Reverse sorted array\n";
    cout << "------------------------------\n";
    printArray(arr3, "Before");
    QuickSorter::sort(arr3);
    printArray(arr3, "After ");
    cout << endl;

    // Example 4: Array with duplicates
    vector<int> arr4 = {3, 1, 4, 1, 5, 9, 2, 6, 5, 3};
    cout << "Example 4 - Array with duplicates\n";
    cout << "-------------------------------\n";
    printArray(arr4, "Before");
    QuickSorter::sort(arr4);
    printArray(arr4, "After ");

    return 0;
}