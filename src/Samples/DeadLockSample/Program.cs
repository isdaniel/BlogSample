using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeadLockSample
{
    class Program
    {
        

        static void Main(string[] args)
        {
            DeadlockObject obj = new DeadlockObject();

            Task.WaitAll(Task.Run(() => obj.LockAThenLockB()), Task.Run(() => obj.LockBThenLockA()));
            Console.WriteLine("complete Job");
            Console.ReadKey();
        }

        
    }

    public class DeadlockObject
    {
        private object _lockA = new object();
        private object _lockB = new object();

        public void LockAThenLockB()
        {
            Console.WriteLine("exec LockAThenLockB!!");
            lock (_lockA)
            {
                //Thread sleeping will help deadlock display easily
                Thread.Sleep(5);
                lock (_lockB)
                {
                    
                }
            }

            Console.WriteLine("finish LockAThenLockB!!");
        }

        public void LockBThenLockA()
        {
            Console.WriteLine("exec LockBThenLockA!!");
            lock (_lockB)
            {
                //Thread sleeping will help deadlock display easily
                Thread.Sleep(5);
                lock (_lockA)
                {
                    
                }
            }

            Console.WriteLine("finish LockBThenLockA!!");
        }
    }
}
