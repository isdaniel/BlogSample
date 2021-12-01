using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huffman
{
    public class HuffmanCompression{
        public (byte[] compressBuffer,int bitLength) Zip(string s,Dictionary<char,string> huffmanCodes)
        {
            var bitString = string.Join("", s.Select(x => huffmanCodes[x]));
            var bitArray = new BitArray(bitString.Select(c => c == '1').Reverse().ToArray());
            var huffmanCodeBytes = new byte[(bitArray.Length + 7)/8];
            bitArray.CopyTo(huffmanCodeBytes,0);
            return (huffmanCodeBytes, bitArray.Length);
        }

        private string ByteToBitString(byte b,bool flag, int bitLength)
        {
            short temp = b;
            if (flag)
            {
                temp |= 256;
            }

            string result = Convert.ToString(temp,2);

            if (flag)
            {
                return result.Substring(result.Length - 8);
            }
            
            //判斷需要補位0
            return result.PadLeft(bitLength % 8, '0');
        }

        public string UnZip(byte[] buffer,Node huffmanTree,int bitLength)
        {
            var bitString = string.Join("",buffer.Select((x,i)=> ByteToBitString(x, i != buffer.Length - 1, bitLength)).Reverse());
            var sb = new StringBuilder();
            Node cur = huffmanTree;

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
                    cur = huffmanTree;
                }
            }

            return sb.ToString();
        }
    }
}
