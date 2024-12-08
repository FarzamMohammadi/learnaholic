#include <cassert>
#include <iostream>
#include <vector>

/*
ArraySorter::sort() - Bubble Sort Visualization
Example with array [5, 3, 8, 4, 2]:

Initial array: 5 3 8 4 2

Pass 1: (looking at whole array)
(5,3) → 3 5 8 4 2    // j=0: swap needed
(5,8) → 3 5 8 4 2    // j=1: no swap
(8,4) → 3 5 4 8 2    // j=2: swap needed
(8,2) → 3 5 4 2 8    // j=3: swap needed (8 bubbles to end)

Pass 2: (ignore last element - it's sorted)
(3,5) → 3 5 4 2 8    // j=0: no swap
(5,4) → 3 4 5 2 8    // j=1: swap needed
(5,2) → 3 4 2 5 8    // j=2: swap needed (5 bubbles up)

Pass 3: (ignore last two elements)
(3,4) → 3 4 2 5 8    // j=0: no swap
(4,2) → 3 2 4 5 8    // j=1: swap needed

Pass 4: (ignore last three elements)
(3,2) → 2 3 4 5 8    // j=0: swap needed

Final array: 2 3 4 5 8

Time Complexity: O(n^2)
Space Complexity: O(1) - in-place sorting
*/

using namespace std;

class ArraySorter
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

    ArraySorter::sort(arr);

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