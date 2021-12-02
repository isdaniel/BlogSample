using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
namespace Huffman
{
    [Serializable]  
    public class CompressInfo {
        public Dictionary<byte,string> HuffmanCodes {get;set;}

        public int CompressionBitLength { get; set; }
        //public int CompressionBitLength { get; set; }
    }

    public class HuffmanCompression{
        private Dictionary<byte,string> _huffmanCodes;
        public Node HuffmanTree { get; private set; }
        public (byte[] compressBuffer,int bitLength) Zip(string input)
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            return Zip(buffer);
        }

        public (byte[] compressBuffer,int bitLength) Zip(byte[] buffer)
        {
            var nodeList = buffer.GroupBy(x => x).Select(x => new Node { Weight = x.Count(), c = x.Key }).ToList();
            var huffmanTree = CreateHuffmanTree(nodeList);
            this.HuffmanTree = huffmanTree;
            _huffmanCodes = HuffmanTree.ScanTree();
            var bitString = string.Join("", buffer.Select(x => _huffmanCodes[x]));
            var bitArray = new BitArray(bitString.Select(c => c == '1').Reverse().ToArray());
            var huffmanCodeBytes = new byte[(bitArray.Length + 7)/8];
            bitArray.CopyTo(huffmanCodeBytes,0);
            return (huffmanCodeBytes, bitArray.Length);
        }

        public void ZipFile(string srcFile,string desFile){
            var buffer = File.ReadAllBytes(srcFile);
            var compressResult = Zip(buffer);
            CompressInfo compressInfo = new CompressInfo(){
                HuffmanCodes = _huffmanCodes,
                CompressionBitLength = compressResult.bitLength
            };

            using (Stream stream = new FileStream(desFile , FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write(JsonConvert.SerializeObject(compressInfo));
                bw.Write(compressResult.compressBuffer);
            }
        }

        
        public void UnZipFile(string srcFile,string desFile){
            using (Stream stream = new FileStream(srcFile , FileMode.Open, FileAccess.Read, FileShare.None))
            using (BinaryReader br = new BinaryReader(stream))
            {          
                
                var json = br.ReadString();
                var compressInfo = JsonConvert.DeserializeObject<CompressInfo>(json);
                var buffer = br.ReadBytes((int)(stream.Length - stream.Position));
                var bytes = UnZip(buffer,compressInfo.CompressionBitLength);
                File.WriteAllBytes(desFile,bytes);
            }
        }

        //byte => bit
        private string ByteToBitString(short b,bool isLastByte, int bitLength)
        {
            //判斷需要補位0
            if (isLastByte)
            {
                return Convert.ToString(b,2).PadLeft(bitLength % 8, '0');
            }

            string result = Convert.ToString(b |= 256,2);

            return result.Substring(result.Length - 8);
        }

        public byte[] UnZip(byte[] buffer,int orginalBitLength)
        {
            var bitString = string.Join("",buffer.Select((x,i)=> ByteToBitString((short)x, i == buffer.Length - 1, orginalBitLength)).Reverse());
            List<byte> bitArray = new List<byte>();
            Node cur = HuffmanTree;

            for (int i = 0; i < bitString.Length; i++)
            {
                if (bitString[i] == '1')
                {
                    cur = cur.Right;
                }else{
                    cur = cur.Left;
                }

                if (cur.c.HasValue)
                {
                    bitArray.Add(cur.c.Value);
                    cur = HuffmanTree;
                }
            }

            return bitArray.ToArray();
        }

        private Node CreateHuffmanTree(List<Node> nodes){

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
}
