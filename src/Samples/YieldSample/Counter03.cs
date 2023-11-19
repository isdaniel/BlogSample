using System.Collections.Generic;

namespace YieldSample
{
    public class Counter03
    {
        public static IEnumerable<int> GetValue(int end)
        {
            int current = 0;
            
            do
            {
                current++;
                if (current % 2 == 0)
                {
                    yield return current;
                }
            } while (current < end);

        }
    }
}