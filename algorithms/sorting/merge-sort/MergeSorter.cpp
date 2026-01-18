#include <iomanip>
#include <iostream>
#include <vector>

using namespace std;

/*
    Merge Sort Properties:
    - Divide and Conquer algorithm that splits array into two halves, recursively sorts them, then merges
    - Stable sort: preserves relative order of equal elements
    - Properties:
        * Not in-place: requires extra space proportional to input size
        * External sorting: efficient for sorting large files that don't fit in memory
        * Parallelizable: different parts can be sorted independently
        * Predictable: always O(n log n) regardless of input order
*/

/*
    MergeSorter::sort() - Merge Sort Visualization
    Example with array [64, 34, 25, 12, 22, 11, 90]

    Splitting Phase (Top-Down):
    [64, 34, 25, 12, 22, 11, 90]                 // Initial array
    /                          \
    [64, 34, 25, 12]          [22, 11, 90]       // Split 1
    /            \            /          \
    [64, 34]    [25, 12]    [22, 11]    [90]     // Split 2
    /     \      /    \      /    \       |
    [64]  [34]  [25]  [12]  [22]  [11]   [90]    // Individual elements

    Merging Phase (Bottom-Up):
    [34, 64]  [12, 25]       [11, 22] [90]       // First merge: Compare & sort pairs
       \         /              \      |
    [12, 25, 34, 64]         [11, 22, 90]        // Second merge: Merge sorted halves
              \                    /
            [11, 12, 22, 25, 34, 64, 90]         // Final merge: Complete sorted array

    Key Operations:
    1. Splitting: O(log n) levels of recursion
    2. Merging: O(n) comparisons at each level
    3. Total: O(n log n) comparisons

    Space Complexity Analysis:
    - Temporary arrays: O(n) for merge operation
    - Recursion stack: O(log n) for function calls
    - Total: O(n) extra space

    Advantages:
    1. Stable sorting
    2. Guaranteed O(n log n) performance
    3. Good for linked lists (no random access needed)
    4. Cache-friendly sequential access

    Disadvantages:
    1. Extra space requirement
    2. Overkill for small arrays
    3. Not adaptive (doesn't benefit from partially sorted input)
*/

class MergeSorter
{
public:
    static void sort(vector<int>& arr)
    {
        if (arr.empty()) return;

        sort(arr, 0, arr.size() - 1);
    }

private:
    static void sort(vector<int>& arr, int left, int right)
    {
        if (left >= right) return;

        int mid = left + (right - left) / 2;

        sort(arr, left, mid);
        sort(arr, mid + 1, right);

        merge(arr, left, mid, right);
    }

    static void merge(vector<int>& arr, int left, int mid, int right)
    {
        // Create temporary arrays
        int leftSize = mid - left + 1;
        int rightSize = right - mid;

        vector<int> leftArr(leftSize);
        vector<int> rightArr(rightSize);

        for (int i = 0; i < leftSize; i++) leftArr[i] = arr[left + i];
        for (int i = 0; i < rightSize; i++) rightArr[i] = arr[mid + 1 + i];

        // Merge the temporary arrays back into arr[left..right]
        int leftIdx = 0;
        int rightIdx = 0;
        int mergeIdx = left;

        while (leftIdx < leftSize && rightIdx < rightSize)
        {
            if (leftArr[leftIdx] <= rightArr[rightIdx])
            {
                arr[mergeIdx] = leftArr[leftIdx];
                leftIdx++;
            }
            else
            {
                arr[mergeIdx] = rightArr[rightIdx];
                rightIdx++;
            }

            mergeIdx++;
        }

        // Copy remaining elements
        while (leftIdx < leftSize)
        {
            arr[mergeIdx] = leftArr[leftIdx];

            leftIdx++;
            mergeIdx++;
        }

        while (rightIdx < rightSize)
        {
            arr[mergeIdx] = rightArr[rightIdx];

            rightIdx++;
            mergeIdx++;
        }
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
    MergeSorter::sort(arr1);
    printArray(arr1, "After ");
    cout << endl;

    // Example 2: Already sorted array
    vector<int> arr2 = {1, 2, 3, 4, 5};
    cout << "Example 2 - Already sorted array\n";
    cout << "------------------------------\n";
    printArray(arr2, "Before");
    MergeSorter::sort(arr2);
    printArray(arr2, "After ");
    cout << endl;

    // Example 3: Reverse sorted array
    vector<int> arr3 = {5, 4, 3, 2, 1};
    cout << "Example 3 - Reverse sorted array\n";
    cout << "------------------------------\n";
    printArray(arr3, "Before");
    MergeSorter::sort(arr3);
    printArray(arr3, "After ");
    cout << endl;

    // Example 4: Array with duplicates
    vector<int> arr4 = {3, 1, 4, 1, 5, 9, 2, 6, 5, 3};
    cout << "Example 4 - Array with duplicates\n";
    cout << "-------------------------------\n";
    printArray(arr4, "Before");
    MergeSorter::sort(arr4);
    printArray(arr4, "After ");

    return 0;
}