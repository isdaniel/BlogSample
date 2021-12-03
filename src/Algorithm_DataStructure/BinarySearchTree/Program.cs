using System;

namespace BinarySearchTree
{
    public class Node {
        public Node(int val)
        {
            Value = val;
        }
        public Node Left { get; set; }
        public Node Right { get; set; }
        public int Value { get; set; }
    }
    
    public class BST{
        private Node _root;
        public BST(Node root)
        {
            _root = root;
        }

        public void AddNode(Node node){
            if (_root == null)
            {
                _root = node;
                return;
            }
            AddNode(_root,node);
        }

        public Node AddNode(Node pNode,Node node){
            if (pNode == null){
                return node;
                
            }
            else if (node.Value < pNode.Value)
            {
                pNode.Left = AddNode(pNode.Left , node);
            }
            else
            {
                pNode.Right = AddNode(pNode.Right, node);
            }

            return pNode;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            BST bst = new BST(null);
            bst.AddNode(new Node(5));
            bst.AddNode(new Node(3));
            bst.AddNode(new Node(8));
            bst.AddNode(new Node(2));
            bst.AddNode(new Node(8));
            bst.AddNode(new Node(7));
        }
    }
}
