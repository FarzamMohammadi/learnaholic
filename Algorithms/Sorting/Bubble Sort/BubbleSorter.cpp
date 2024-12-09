#include <cassert>
#include <iostream>
#include <vector>

using namespace std;

/*
    Inversions in Array Sorting:
    - An inversion is a pair (array[a],array[b]) where a < b and array[a] > array[b] (in short, in incorrect order)
    - Example: in [1,2,2,6,3,5,9,8] there are 3 inversions: (6,3), (6,5), (9,8)
    - Properties:
        * A sorted array has zero inversions
        * Each swap of conse elements removes exactly one inversion
        * Bubble sort works by repeatedly swapping conse elements
        * Number of inversions indicates how "out of order" an array is
*/

/*
    BubbleSorter::sort() - Bubble Sort Visualization
    Example with array [5, 3, 8, 4, 2] - containing inversions (5,3), (5,4), (5,2), (8,4), (8,2), (3,2), (4,2)

    Initial array: 5 3 8 4 2

    Pass 1: (looking at whole array)
    (5,3) → 3 5 8 4 2    // j=0: swap needed - removes (5,3) inversion
    (5,8) → 3 5 8 4 2    // j=1: no swap - no inversion
    (8,4) → 3 5 4 8 2    // j=2: swap needed - removes (8,4) inversion
    (8,2) → 3 5 4 2 8    // j=3: swap needed - removes (8,2) inversion

    Pass 2: (ignore last element - it's sorted)
    (3,5) → 3 5 4 2 8    // j=0: no swap - no inversion
    (5,4) → 3 4 5 2 8    // j=1: swap needed - removes (5,4) inversion
    (5,2) → 3 4 2 5 8    // j=2: swap needed - removes (5,2) inversion

    Pass 3: (ignore last two elements)
    (3,4) → 3 4 2 5 8    // j=0: no swap - no inversion
    (4,2) → 3 2 4 5 8    // j=1: swap needed - removes (4,2) inversion

    Pass 4: (ignore last three elements)
    (3,2) → 2 3 4 5 8    // j=0: swap needed - removes final (3,2) inversion

    Final array: 2 3 4 5 8 (zero inversions - array is sorted)

    Time Complexity: O(n^2)
    Space Complexity: O(1) - in-place sorting
*/

class BubbleSorter
{
public:
    static void sort(vector<int>& arr)
    {
        int n = arr.size();
        bool swapped;

        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;

            for (int j = 0; j < n - i - 1; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    swap(arr[j], arr[j + 1]);
                    swapped = true;
                }
            }

            if (!swapped)
            {
                break;
            }
        }
    }
};

int main()
{
    vector<int> arr = {5, 3, 8, 4, 2};

    cout << "Original array: ";
    for (int num : arr)
    {
        cout << num << " ";
    }
    cout << endl;

    BubbleSorter::sort(arr);

    cout << "Sorted array: ";
    for (int num : arr)
    {
        cout << num << " ";
    }
    cout << endl;

    vector<int> expected = vector<int>{2, 3, 4, 5, 8};
    assert(expected == arr);

    cout << "Assertion passed: Array sorted correctly!" << endl;

    return 0;
}