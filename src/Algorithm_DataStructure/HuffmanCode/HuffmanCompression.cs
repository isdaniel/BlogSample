using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huffman
{
    public class HuffmanCompression{
        private Dictionary<char,string> _huffmanCodes;
        public Node HuffmanTree { get; }
        public HuffmanCompression(Node huffmanTree)
        {
            this.HuffmanTree = huffmanTree;
            _huffmanCodes = HuffmanTree.ScanTree();
            
        }
        public (byte[] compressBuffer,int bitLength) Zip(string s)
        {
            var bitString = string.Join("", s.Select(x => _huffmanCodes[x]));
            var bitArray = new BitArray(bitString.Select(c => c == '1').Reverse().ToArray());
            var huffmanCodeBytes = new byte[(bitArray.Length + 7)/8];
            bitArray.CopyTo(huffmanCodeBytes,0);
            return (huffmanCodeBytes, bitArray.Length);
        }

        //byte => bit
        private string ByteToBitString(short val,bool isLastByte, int bitLength)
        {
            //判斷需要補位0
            if (isLastByte)
            {
                return Convert.ToString(val,2).PadLeft(bitLength % 8, '0');
            }

            string result = Convert.ToString(val |= 256,2);

            return result.Substring(result.Length - 8);
        }

        public string UnZip(byte[] buffer,int bitLength)
        {
            var bitString = string.Join("",buffer.Select((x,i)=> ByteToBitString((short)x, i == buffer.Length - 1, bitLength)).Reverse());
            var sb = new StringBuilder();
            Node cur = HuffmanTree;

            for (int i = 0; i < bitString.Length; i++)
            {
                if (bitString[i] == '1')
                {
                    cur = cur.Right;
                }else{
                    cur = cur.Left;
                }

                if (cur.c != '\0')
                {
                    sb.Append(cur.c);
                    cur = HuffmanTree;
                }
            }

            return sb.ToString();
        }
    }
}
