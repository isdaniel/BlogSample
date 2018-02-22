using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YearDiffSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now.GetDiffYear(DateTime.Now.AddYears(20)));

            Console.ReadKey();
        }
    }

    public static class DateTimeExt
    {
        public static int GetDiffYear(this DateTime startDate,DateTime endDate)
        {
            DateTime zoneTime = new DateTime(1, 1, 1);

            TimeSpan diffTime = TimeSpan.MinValue;

            if (endDate > startDate)
            {
                diffTime = endDate - startDate;
            }
            else
            {
                diffTime = startDate - endDate;
            }

            return (zoneTime + diffTime).Year - 1;
        }
    }
    
}
