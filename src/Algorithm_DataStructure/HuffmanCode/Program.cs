using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Huffman
{

    class Program
    {
        static void Main(string[] args)
        {
            string input = "I like like c# elephant lion!! 你好世界";
            var nodeList = input.GroupBy(x => x).Select(x => new Node { Weight = x.Count(), c = x.Key }).ToList();
            var huffmanTree = CreateHuffmanTree(nodeList);
            var huffmanCodes = huffmanTree.ScanTree();
            HuffmanCompression huffman = new HuffmanCompression();
            var compressResult = huffman.Zip(input,huffmanCodes);  
            var originalData = huffman.UnZip(compressResult.compressBuffer, huffmanTree, compressResult.bitLength);

            int originalLenght = Encoding.UTF8.GetBytes(input).Length;
            int compressionLenght = compressResult.compressBuffer.Length;

            

            Console.WriteLine($"Compress percentage:{(originalLenght - compressionLenght) / (decimal)originalLenght}");
            Console.WriteLine($"Data is : {originalData}");
        }

        
        

        static Node CreateHuffmanTree(List<Node> nodes){

            if (nodes.Count == 1)
            {
                var right = nodes[0];
                var parentNode = new Node()
                {
                    Right = right,
                    Weight = right.Weight
                };
                nodes.Remove(right);
                nodes.Add(parentNode);
            }

            while (nodes.Count > 1)
            {
                nodes.Sort();
                var right = nodes[0];
                var left = nodes[1];
                var parentNode = new Node(){
                    Left = left,
                    Right = right,
                    Weight = right.Weight + left.Weight
                };
                
                nodes.Remove(left);
                nodes.Remove(right);
                nodes.Add(parentNode);
            }

            return nodes[0];
        }
    }

    public class Node : IComparable<Node> {
        private string code { get; set; } 

        public Node Left { get; set; }

        public char c { get; set; }
        public Node Right { get; set; }
        //權值
        public int Weight { get; set; }

        public Dictionary<char,string> ScanTree(){
            Dictionary<char,string> encodingMapper = new Dictionary<char, string>();
            ScanTree(encodingMapper,this);
            return encodingMapper;
        }

        private void ScanTree(Dictionary<char,string> encodingMapper,Node node){
            if (node.c == '\0')
            {
                if (node.Left != null)
                {
                    node.Left.code = node.code + "0";
                    ScanTree(encodingMapper, node.Left);
                }
                
                node.Right.code = node.code + "1";
                ScanTree(encodingMapper,node.Right);
            }
            else {
                encodingMapper.Add(node.c,node.code);
            }
        }

        public int CompareTo(Node other)
        {
            return this.Weight - other.Weight;
        }
    }
}
