
using System.Text;
int sum = 0;
BackTracking(new StringBuilder(),3);
Console.WriteLine(sum);

void BackTracking(StringBuilder sb,int n){
    if (sb.Length == n)
    {
        sum++;
        Console.WriteLine(sb.ToString());
        return;
    }

    for (int i = 0; i < 2; i++)
    {
        sb.Append(i);
        BackTracking(sb,n);
        sb.Remove(sb.Length - 1,1);
    }
}