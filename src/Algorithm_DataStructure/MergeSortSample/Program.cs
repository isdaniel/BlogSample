
var arr = new List<int> { 5, 3, 2, 1, 56, 7, 4, 2 };

var lst = MergeSort(arr);

foreach (var n in lst)
{
    System.Console.WriteLine(n);
}

List<int> MergeSort(List<int> arr){
    if (arr.Count <= 1)
    {
        return arr; 
    }

    List<int> right = new List<int>();
    List<int> left = new List<int>();
    int mid = arr.Count / 2;
    for (int i = 0; i < mid; i++)
    {
        right.Add(arr[i]);
    }

    for (int i = mid ; i < arr.Count; i++)
    {
        left.Add(arr[i]);
    }

    left = MergeSort(left);
    right = MergeSort(right);

    return Merge(left, right);
}

List<int> Merge(List<int> left, List<int> right) {
    List<int> result = new List<int>();

    while (left.Count > 0 && right.Count > 0) {
        if (left[0] >= right[0])
        {
            result.Add(right[0]);
            right.RemoveAt(0);
        }
        else {
            result.Add(left[0]);
            left.RemoveAt(0);
        }
    }

    for (int i = 0; i < left.Count; i++) {
        result.Add(left[i]);
    }

    for (int i = 0; i < right.Count; i++) {
        result.Add(right[i]);
    }

    return result;
}