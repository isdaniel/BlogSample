using System;

namespace AVL_TreeSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //10,11,7,6,8,9

            // BST bst = new BST(null);
            // bst.AddNode(new Node(10));
            // bst.AddNode(new Node(8));
            // bst.AddNode(new Node(7));
            // bst.AddNode(new Node(6));
            // bst.AddNode(new Node(9));
            // bst.AddNode(new Node(12));
            // bst.RightRotate();

            BST bst = new BST(null);
            int[] arr = new int[]{7,6,10,9,11,8};
            foreach (var item in arr)
            {
                bst.AddNode(new Node(item));
            }
            bst.AddNode(new Node(12));
            bst.AddNode(new Node(13));
            bst.AddNode(new Node(14));
            bst.AddNode(new Node(15));
            bst.AddNode(new Node(16));
            bst.AddNode(new Node(17));
            bst.AddNode(new Node(18));
            // bst.AddNode(new Node(3));
            // bst.AddNode(new Node(2));
            // bst.AddNode(new Node(1));
            Console.WriteLine("Hello World!");
        }
    }

    public class BST{
        private Node _root;

        public Node Root => _root;
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
            _root = Add(_root,node);
        }

        private Node Add(Node pNode,Node node){
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

            if (pNode.RightHeight()-pNode.LeftHeight() > 1)
            {
                if (pNode.Right != null && pNode.Right.RightHeight() < pNode.Right.LeftHeight())
                {
                    pNode.Right = pNode.Right.RightRotate();
                }

                pNode = pNode.LeftRotate();
            } else if (pNode.LeftHeight() - pNode.RightHeight() > 1)
            {
                if (pNode.Left != null && pNode.Left.LeftHeight() < pNode.Left.RightHeight())
                {
                    pNode.Left = pNode.Left.LeftRotate();
                }
                pNode = pNode.RightRotate();
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
                    if (parnetNode != null)
                    {
                        if (parnetNode.Left.Value == targetNode.Value)
                        {
                            parnetNode.Left = targetNode.Left;
                        }else{
                            parnetNode.Right = targetNode.Left;
                        }
                    }else{
                        _root = targetNode.Left;
                    }
                    
                }else{
                    if (parnetNode != null){
                        if (parnetNode.Left.Value == targetNode.Value){
                            parnetNode.Left = targetNode.Right;
                        }else{
                            parnetNode.Right = targetNode.Right;
                        }
                    }else{
                        _root = targetNode.Right;
                    }
                }
            }
        }
    }

    public class Node {
        public Node(int val)
        {
            Value = val;
        }
        public Node Left { get; set; }
        public Node Right { get; set; }
        public int Value { get; set; }


        public int RightHeight(){
            if (Right == null)
                return 0;

            return Right.Height();
        }

        public Node LeftRotate(){
            /*
            1. 建立新節點等於目前根節點的值
            2. 新節點的左子樹指向當前節點左子樹
            3. 新節點的右子樹指向當前節點右子樹的左子樹
            4. 將當前節點的值改成當前節點右子樹的值
            5. 把當前節點右子樹指向右子樹的右子樹
            6. 將當前節點左子樹指向新節點
            */

            // Node newNode = new Node(Value);    
            // newNode.Left = Left;
            // newNode.Right = Right.Left;
            // Value = Right.Value;
            // Right = Right.Right;
            // Left = newNode;
            Node newRoot = this.Right;
            Right = newRoot.Left;
            newRoot.Left = this;
            return newRoot;
        }

        public Node RightRotate(){
            Node newRoot = this.Left;
            Left = newRoot.Right;
            newRoot.Right = this;
            return newRoot;
        }

        public int LeftHeight(){
            if (Left == null)
                return 0;

            return Left.Height();
        }

        public int Height(){
            return Math.Max(Left == null ?  0 : Left.Height() ,Right == null ?  0 : Right.Height()) +1;
        }

        public Node Search(int value){
            if (this.Value == value)
                return this;

            if (this.Value > value)
            {
                if (Left == null)
                    return null;
                return Left.Search(value);
            }else{
                if (Right == null)
                    return null;
                return Right.Search(value);
            }
            
        }

        public Node SearchParent(int value){
            if ((Left != null && Left.Value == value) ||
                (Right != null && Right.Value == value))
            {
                return this;
            }

            if (Left != null && this.Value < value)
            {
                return Left.SearchParent(value);
            }else if(Right != null && this.Value >= value){
                return Right.SearchParent(value);
            }

            return null;
        }
    }
}
