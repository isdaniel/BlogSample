using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSample
{
    public class Node{
        public Node Next { get; set; }

        public int Value { get; set; }
    }
    //show Graph
    

    
    class Program
    {
        static Node CreateNode(int[] arr){
            Node dummy = new Node();
            Node cur = dummy;
            foreach(var v in arr){
                cur.Next = new Node(){
                    Value = v
                };
                cur = cur.Next;
            }

            return dummy.Next;
        }

        static void ShowNodeInfo(Node first, string s){
            System.Console.WriteLine(s);
            while (first != null)
            {
                System.Console.WriteLine(first.Value);
                first = first.Next;
            }
        }
        static void Main(string[] args)
        {
            Node root = CreateNode(new[]{1,2,3,4,5,6});
            
            Node dummy = new Node();
            dummy.Next = root;
            Node fast = dummy, slow = dummy;
            //Node cur = dummy;

            while( fast != null &&  fast.Next != null){
                slow = slow.Next;
                fast = fast.Next.Next;
            }

            
            Node second = slow.Next;
            slow.Next = null;
            Node first = dummy.Next;
            ShowNodeInfo(dummy.Next,"first part:");
            ShowNodeInfo(second,"second part:");
            // int removeVal = 2;

            // Node dummy = new Node();
            // Node tail = dummy;
            // while(root != null){
            //     if(root.Value != removeVal){
            //         tail.Next = root;
            //         tail = tail.Next;
            //     }
            //     root = root.Next;
            // }

            // tail.Next = null;

            // dummy = dummy.Next;
            // while(dummy != null){
            //     Console.WriteLine(dummy.Value);
            //     dummy = dummy.Next;
            // }
        }
    }
}
