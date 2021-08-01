using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OwnConsumerPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            Store store = new Store(5);
            Factory factory = new Factory(store);

            var seller1 = Task.Run(() => { Customer customer = new Customer(store,"Tom");  customer.Buy();});
            var seller2 = Task.Run(() => { Customer customer = new Customer(store,"Tom");  customer.Buy();});
            var seller3 = Task.Run(() => { Customer customer = new Customer(store,"Tom");  customer.Buy();});
         
            var f1 = Task.Run(() => { factory.CreateProduct(); });
            var f2 = Task.Run(() => { factory.CreateProduct(); });
            var f3 = Task.Run(() => { factory.CreateProduct(); });

            Task.WaitAll(seller1, f3, f1, f2, seller2,seller3);

            Console.WriteLine("End");
            Console.ReadKey();
        }

    }

    public class Store
    {
        private volatile Queue<Product> _productQueue = new Queue<Product>();
        
        private readonly int _maxInStock;

        public Store(int maxInStock)
        {
            _maxInStock = maxInStock;
        }


        public void PurchaseProduct(Product p)
        {
            lock (_productQueue)
            {
                while (_productQueue.Count >= _maxInStock)
                {
                    Monitor.Wait(_productQueue);
                }

                _productQueue.Enqueue(p);
                Monitor.Pulse(_productQueue);
            }
        }

        public Product Sell()
        {
            lock (_productQueue)
            {
                while (_productQueue.Count <= 0)
                {
                    if (Monitor.Wait(_productQueue, 1000)) continue;
                    Console.WriteLine("缺貨 客戶等不及了 離開....");
                    return null;
                }

                var p = _productQueue.Dequeue();
                Monitor.PulseAll(_productQueue);
                return p;
            }
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Factory
    {
        private readonly Store _store;
        public Factory(Store store)
        {
            _store = store;
        }

        public void CreateProduct()
        {

            for (int i = 0; i < 1000; i++)
            {
                _store.PurchaseProduct(new Product()
                {
                    Name = $"[{Thread.CurrentThread.ManagedThreadId}] Buy Computer",
                    Price = i
                });
            }
            
        }
    }

    public class Customer
    {
        private readonly Store _store;
        private readonly string _name;
        public Customer(Store store, string name)
        {
            _store = store;
            _name = name;
        }

        public void Buy()
        {
            //Thread.Sleep(10);
            while (true)
            {
                var p = _store.Sell();
                if (p != null)
                {
                    ShowMessage(p);
                }
                else
                {
                    break;
                }
            }
            
        }


        private void ShowMessage(Product p)
        {
            Console.WriteLine($"Buyer:{_name} ProductName {p.Name} , Price {p.Price}");
        }
    }
}
