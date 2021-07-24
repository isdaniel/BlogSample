using System;
using System.Threading;

namespace VolatileSample
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Member member = new Member();
            new Thread(()=>{
                
                System.Console.WriteLine($"Sleep 前~ 餘額剩下:{member.balance}");
                member.UpdateBalance();
                System.Console.WriteLine($"Sleep 結束! 餘額剩下:{member.balance}");
            }).Start();
            
            while (member.balance > 0)
            {
               
            }   
            Thread.Sleep(50);
            Console.WriteLine("執行結束!");
            Console.ReadKey();
        }
    }

    public class Member
    {
        public int balance = 100;
        public void UpdateBalance()
        {
             balance = 0;  
        }
    }
}
