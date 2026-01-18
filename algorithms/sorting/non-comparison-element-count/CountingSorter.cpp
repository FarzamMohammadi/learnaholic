#include <iomanip>
#include <iostream>
#include <map>
#include <vector>

using namespace std;

/*
    Counting Sort Algorithm:
    1. Count frequencies using hash map (handles negative & positive integers)
    2. Calculate cumulative frequencies to determine positions
    3. Build output array by placing elements in their sorted positions

    Time Complexity: O(n + k) where k is number of unique elements
    Space Complexity: O(k) where k is number of unique elements

    Features:
    - Handles negative and positive integers
    - Maintains stability (preserves order of equal elements)
    - Works with duplicates
    - Memory efficient (only stores unique elements)
*/

class CountingSorter
{
public:
    static vector<int> sort(vector<int>& arr)
    {
        if (arr.empty()) return arr;

        map<int, int> frequency;

        for (int num : arr) frequency[num]++;

        vector<int> output(arr.size());

        size_t sum = 0;

        for (auto& pair : frequency)
        {
            int count = pair.second;
            pair.second = sum;
            sum += count;
        }

        for (size_t i = arr.size() - 1; i >= 0; i--)
        {
            int currentNum = arr[i];
            int position = frequency[currentNum];
            output[position] = currentNum;
            frequency[currentNum]++;
        }

        return output;
    }
};

void printArray(const vector<int>& arr)
{
    cout << "\033[33m[";
    for (size_t i = 0; i < arr.size(); i++)
    {
        cout << setw(3) << arr[i];
        if (i < arr.size() - 1) cout << ",";
    }
    cout << " ]\033[0m" << endl;
}

int main()
{
    cout << "\n\033[1;36mCounting Sort Algorithm\033[0m" << endl;
    cout << "\033[1;36m---------------------\033[0m" << endl;

    vector<pair<string, vector<int>>> test_cases = {{"Random Array", {4, 2, 2, 8, 3, 3, 1}},
                                                    {"Already Sorted", {1, 2, 3, 4, 5}},
                                                    {"Reverse Sorted", {5, 4, 3, 2, 1}},
                                                    {"Array with Duplicates", {4, 4, 2, 2, 2, 8, 3, 3, 1}},
                                                    {"Negative Numbers", {-3, -1, -4, -1, -5}},
                                                    {"Mixed Numbers", {-5, 2, -3, 7, 0, -1}}};

    for (size_t i = 0; i < test_cases.size(); i++)
    {
        cout << "\n\033[1;32mTest Case " << i + 1 << " - " << test_cases[i].first << ":\033[0m" << endl;
        cout << "Before sorting: ";
        printArray(test_cases[i].second);

        vector<int> sorted = CountingSorter::sort(test_cases[i].second);

        cout << "After sorting:  ";
        printArray(sorted);
    }

    return 0;
}