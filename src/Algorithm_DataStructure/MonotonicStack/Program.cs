

using System.Data;
using System.Runtime.CompilerServices;

internal class Program
{
    private static void Main(string[] args){
        foreach (var n in FindRightSamll(new int[]{1,2,4,9,4,0,5}))
        {
            System.Console.WriteLine(n);
        }
    }

    static int[] FindRightSamll(int[] arr){
        int[] res = new int[arr.Length];
        Stack<int> stack = new Stack<int>();

        for(int i = 0; i < arr.Length; i++)
        {
            while (stack.Count > 0 && arr[i] < arr[stack.Peek()])
            {
                res[stack.Pop()] = arr[i];
            }

            stack.Push(i);
        }

        while (stack.Count > 0)
        {
            res[stack.Pop()] = -1;
        }

        return res;
    }
}

