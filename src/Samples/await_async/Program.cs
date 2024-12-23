using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Transactions;

public class MyTask
{

    private bool _completed;
    private Exception? _exception;
    private Action? _continuation;
    private ExecutionContext? _context;
    private object _sycn = new object();
    public bool IsCompleted
    {
        get
        {
            lock (_sycn)
            {
                return _completed;
            }
        }
    }

    public void SetResult() => Complete(null);
    public void SetException(Exception exception) { }
    private void Complete(Exception exception)
    {
        lock (_sycn)
        {
            if (_completed)
            {
                throw new InvalidOperationException("Task has done!");
            }

            _completed = true;
            _exception = exception;

            if (_continuation != null)
            {
                if (_context == null)
                {
                    _continuation();
                }
                else
                {
                    ExecutionContext.Run(_context, (obj) => _continuation(), null);
                }
            }
        }
    }
    public void Wait()
    {
        ManualResetEventSlim mres = null;

        lock (_sycn)
        {
            if (!_completed)
            {
                mres = new ManualResetEventSlim();
                ContinueWith(mres.Set);
            }
        }

        mres?.Wait();

        if (_exception != null)
        {
            throw new AggregateException(_exception);
        }
    }

    public MyTask ContinueWith(Action action)
    {
        MyTask t = new();

        Action callback = () =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                t.SetException(ex);
                return;
            }

            t.SetResult();
        };

        lock (_sycn)
        {
            if (_completed)
            {
                MyThreadPool.QueueUserWorkItem(callback);
            }
            else
            {
                _continuation = callback;
                _context = ExecutionContext.Capture();
            }
        }

        return t;
    }

    public MyTask ContinueWith(Func<MyTask> action)
    {
        MyTask t = new();

        Action callback = () =>
        {
            try
            {
                MyTask next = action();
                next.ContinueWith(() => {
                    if (next._exception != null)
                    {
                        t.SetException(next._exception);
                    } else {
                        t.SetResult();
                    }
                });
            }
            catch (Exception ex)
            {
                t.SetException(ex);
                return;
            }
        };

        lock (_sycn)
        {
            if (_completed)
            {
                MyThreadPool.QueueUserWorkItem(callback);
            }
            else
            {
                _continuation = callback;
                _context = ExecutionContext.Capture();
            }
        }

        return t;
    }


    public static MyTask Run(Action action)
    {

        MyTask t = new MyTask();

        MyThreadPool.QueueUserWorkItem(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                t.SetException(ex);
                return;
            }
            t.SetResult();
        });

        return t;
    }

    public static MyTask Delay(int timeout)
    {
        MyTask t = new();

        new Timer(_ => t.SetResult()).Change(timeout, -1);

        return t;
    }

    //await and async impl by ourselves
    public struct Awaiter : INotifyCompletion
    {

        MyTask _t;
        public Awaiter(MyTask t)
        {
            this._t = t;
        }

        public Awaiter GetAwaiter() => this;

        public void GetResult() => _t.Wait();

        public bool IsCompleted => _t.IsCompleted;

        public void OnCompleted(Action continuation)
        {
            _t.ContinueWith(continuation);
        }
    }

    public static MyTask WaitAll(List<MyTask> tasks)
    {
        MyTask t = new();

        if (tasks.Count == 0)
        {
            t.SetResult();
        }
        else
        {
            int remaining = tasks.Count;
            Action continuation = () =>
            {
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    t.SetResult();
                }
            };

            foreach (MyTask task in tasks)
            {
                task.ContinueWith(continuation);
            }
        }

        return t;
    }

    public static MyTask Iterate(IEnumerable<MyTask> tasks) {
        MyTask t = new();
        IEnumerator<MyTask> e = tasks.GetEnumerator();

        void MoveNext() {
            try
            {
                if (e.MoveNext())
                {
                    var next = e.Current;
                    next.ContinueWith(MoveNext);
                    return;
                }
            }
            catch (Exception ex)
            {
                t.SetException(ex);
                return;
            }
            t.SetResult();
        }

        MoveNext();
        
        return t;
    }

    public Awaiter GetAwaiter() => new(this);
}

public class MyThreadPool
{
    private static readonly BlockingCollection<(Action action, ExecutionContext context)> s_workerItems = new BlockingCollection<(Action action, ExecutionContext context)>();
    public static void QueueUserWorkItem(Action action) => s_workerItems.Add((action, ExecutionContext.Capture()));

    static MyThreadPool()
    {
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            new Thread(() =>
            {
                while (true)
                {
                    (Action workItem, ExecutionContext context) = s_workerItems.Take();

                    if (context == null)
                    {
                        workItem();
                    }
                    else
                    {
                        ExecutionContext.Run(context, (obj) => workItem(), null);
                    }

                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }

}

class Program
{

    static async Task Main(string[] args)
    {
        //MyTask.Iterate(Count()).Wait();
        await CountAsync();

        static async Task CountAsync() {
            for (int i = 0; ; i++)
            {
                //yield return MyTask.Delay(1000);
                await MyTask.Delay(1000);
                Console.WriteLine(i);
            }
        }

        //Console.Write("Hello, ");
        //MyTask.Delay(2000).ContinueWith(() =>
        //{
        //    Console.Write("World");
        //    return MyTask.Delay(1000);
        //}).ContinueWith(() =>
        //{
        //    Console.Write(", Daniel");
        //}).Wait();

        //Console.ReadLine();

        //AsyncLocal<int> asyncLocal = new AsyncLocal<int>();
        //List<MyTask> tasks = new List<MyTask>();
        //for (int i = 0; i < 50; i++)
        //{
        //    asyncLocal.Value = i;
        //    tasks.Add(MyTask.Run(() =>
        //    {
        //        Console.WriteLine(asyncLocal.Value);
        //        Thread.Sleep(1000);
        //    }));
        //}

        //MyTask.WaitAll(tasks).Wait();

    }
}