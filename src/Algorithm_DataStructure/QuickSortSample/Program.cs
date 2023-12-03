int[] arr = new int[] {3,5,1,2,3,5,7,19,8,100,-1};

QuickSort(arr,0,arr.Length - 1);

foreach (var v in arr)
{
    System.Console.WriteLine(v);
}


void QuickSort(int[] arr,int l ,int r){
    if(l >= r){        
        return;
    }
    int p = Partition(arr,l ,r);
    QuickSort(arr,l , p - 1);
    QuickSort(arr,p + 1 , r);
}

int Partition(int[] arr, int l ,int r){
    //todo pivot can be random.
    int pivot = l + (r - l) / 2;
    Swap(arr,pivot,r);
    int j = l;

    for (int i = l; i < r ; i++)
    {
        if (arr[i] <= arr[r])
        {
            Swap(arr,i,j);
            j++;
        }
    }

    Swap(arr,j,r);
    return j;
}

void Swap(int[] arr,int a , int b){
    int tmp = arr[a];
    arr[a] = arr[b];
    arr[b] = tmp;
}