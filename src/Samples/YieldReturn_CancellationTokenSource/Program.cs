
var arr = Enumerable.Range(1,1000);

foreach (var item in GetData(arr,0))
{
    System.Console.WriteLine(item);
}

foreach (var item in GetData(arr,20))
{
    System.Console.WriteLine(item);
}


IEnumerable<int> GetData(IEnumerable<int> arr,int skipNumber){
    var cts = new CancellationTokenSource();
    int cnt = 0;
    foreach (var item in arr.Skip(skipNumber)) {
        cnt++;
        if(item % 20 == 0){
            cts.Cancel();
        }
        System.Console.WriteLine($"cnt {cnt}");
        yield return item;
        if (cts.IsCancellationRequested) {
            System.Console.WriteLine("yield break!!");
            yield break;
        }
    }

    yield return int.MaxValue;
} 