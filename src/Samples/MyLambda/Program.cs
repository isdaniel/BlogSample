using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLambda
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] arr = new[] {1, 2, 3, 4, 5, 6};

            var result = arr.MyWhere(x => x > 3);

            Console.WriteLine();
        }

    }

    public static class MyMyLambdaExtension
    {
        public static IEnumerable<T> MyWhere<T>(this IEnumerable<T> collection,Func<T, bool> selectFunc)
        {
            foreach (var item in collection)
            {
                if (selectFunc(item))
                {
                    yield return item;
                }
            }
        }
    }
}
