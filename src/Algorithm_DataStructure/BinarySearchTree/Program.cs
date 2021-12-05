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

        public Node Search(int value){
            if (this.Value == value)
                return this;
            else if (value < this.Value){
                if (this.Left == null)
                    return null;
                return this.Left.Search(value);
            }
            else{
                if (this.Right == null)
                    return null;
                return this.Right.Search(value);
            }
        }

        public Node SearchParent(int value){
            if ((this.Left != null && this.Left.Value == value)||
                (this.Right != null && this.Right.Value == value))
                return this;
            
            if (this.Value > value && this.Left != null)
            {
                return this.Left.SearchParent(value);
            }else if (this.Value <= value && this.Right != null){
                return this.Right.SearchParent(value);
            }

            return null;
        }
    }
    
    public class BST{
        private Node _root;
        public BST(Node root)
        {
            _root = root;
        }

        public Node Search(int value){
            if (_root == null)
            {
                return null;
            }else{
                return _root.Search(value);
            }
        }


        public int DeleteRightMinNode(Node node){
            Node target = node;

            while (target.Left != null)
            {
                target = target.Left;
            }

            Delete(target.Value);

            return target.Value;
        }

        public Node SearchParent(int value){
            if (_root == null)
            {
                return null;
            }else{
                return _root.SearchParent(value);
            }
        }


        public void AddNode(Node node){
            if (_root == null)
            {
                _root = node;
                return;
            }
            Add(_root,node);
        }

        public Node Add(Node pNode,Node node){
            if (pNode == null){
                return node;
                
            }
            else if (node.Value < pNode.Value)
            {
                pNode.Left = Add(pNode.Left , node);
            }
            else
            {
                pNode.Right = Add(pNode.Right, node);
            }

            return pNode;
        }

        public void Delete(int value){
            if (_root == null)
                return;
            
            Node targetNode = Search(value);
            
            //都找尋不到刪除點
            if (targetNode == null)
                return;

            //如果只有 root 節點
            if (_root.Left == null && _root.Right == null)
            {
                _root = null;
                return;
            }
            
            Node parnetNode = SearchParent(value);
            //如果刪除點是 葉子節點
            if (targetNode.Left == null && targetNode.Right == null)
            {
                if (parnetNode.Left.Value == value)
                {
                    parnetNode.Left = null;
                }else if (parnetNode.Right.Value == value)
                {
                    parnetNode.Right = null;
                }
            } 
            //刪除點有兩顆子樹節點
            else if (targetNode.Left != null && targetNode.Right != null){
                //找尋右子樹最小節點
                int minVal = DeleteRightMinNode(targetNode.Right);
                targetNode.Value = minVal;
            }
            //刪除節點只有一個子樹節點
            else {
                if (targetNode.Left != null)
                {
                    if (parnetNode.Left.Value == targetNode.Value)
                    {
                        parnetNode.Left = targetNode.Left;
                    }else{
                        parnetNode.Right = targetNode.Left;
                    }
                    
                }else{
                    if (parnetNode.Left.Value == targetNode.Value){
                        parnetNode.Left = targetNode.Right;
                    }else{
                        parnetNode.Right = targetNode.Right;
                    }
                }
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            BST bst = new BST(null);
            bst.AddNode(new Node(7));
            bst.AddNode(new Node(3));
            bst.AddNode(new Node(10));
            bst.AddNode(new Node(1));
            bst.AddNode(new Node(5));
            bst.AddNode(new Node(9));
            bst.AddNode(new Node(12));
            bst.AddNode(new Node(2));

            bst.Delete(7);
            bst.Delete(3);
        }
    }
}
