using System;
using System.Collections.Generic;

namespace LRU_Algorithm
{
    public class LRUCache {

        private Dictionary<int,Node<int,int>> _map;
        private DoubleLinkedList<int, int> _linkedList;
        private readonly int _capacity;

        public LRUCache(int capacity) {
            this._capacity = capacity;
            _map = new Dictionary<int, Node<int, int>>();
            _linkedList = new DoubleLinkedList<int,int>();
        }
        
        public int Get(int key) {
            if (_map.TryGetValue(key,out var node))
            {
                _linkedList.RemoveNode(node);
                _linkedList.AddHeader(node);			
                return node.Value;
            }

            return -1;
        }
        
        public void Put(int key, int value) {

            var newNode = new Node<int,int>(){
                Value = value,
                Key = key
            };

            if (_map.TryGetValue(key,out var node))
            {
                _linkedList.RemoveNode(node);
                _linkedList.AddHeader(newNode);
                _map[key] = newNode;
            }else{
                if (_map.Count == _capacity)
                {
                    var lastNode = _linkedList.GetLastNode();
                    _linkedList.RemoveNode(lastNode);
                    _map.Remove(lastNode.Key);
                }

                _map[key] = newNode;
                _linkedList.AddHeader(newNode);
            }
        }

        public void PrintAll(){
            _linkedList.PrintAll();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LRUCache lruCache = new LRUCache(5);

            lruCache.Put(1,1);
            lruCache.Put(2,2);
            lruCache.Put(3,3);
            lruCache.Put(4,4);
            lruCache.Put(5,5);
            lruCache.Put(6,6);

            lruCache.PrintAll();

            System.Console.WriteLine("=============================");

            lruCache.Put(1,1);
            lruCache.Put(3,3);
            lruCache.PrintAll();
        }
    }
}
