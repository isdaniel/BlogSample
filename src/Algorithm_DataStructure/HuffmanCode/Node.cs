using System;
using System.Collections.Generic;

namespace Huffman
{
    public class Node : IComparable<Node> {
        private string code { get; set; } 

        public Node Left { get; set; }

        public byte? c { get; set; }
        public Node Right { get; set; }
        //權值
        public int Weight { get; set; }

        public Dictionary<byte,string> ScanTree(){
            Dictionary<byte,string> haffmanCodes = new Dictionary<byte, string>();
            ScanTree(haffmanCodes,this);
            return haffmanCodes;
        }

        private void ScanTree(Dictionary<byte,string> haffmanCodes,Node node){
            if (!node.c.HasValue)
            {
                if (node.Left != null)
                {
                    node.Left.code = node.code + "0";
                    ScanTree(haffmanCodes, node.Left);
                }
                if (node.Right != null)
                {
                    node.Right.code = node.code + "1";
                    ScanTree(haffmanCodes,node.Right);
                }
            }
            else {
                haffmanCodes.Add(node.c.Value,node.code);
            }
        }

        public int CompareTo(Node other)
        {
            return this.Weight - other.Weight;
        }
    }
}
