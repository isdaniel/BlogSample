using System.Collections.Generic;
namespace Huffman
{
    public class CompressInfo {
        public Dictionary<byte,string> HuffmanCodes {get;set;}

        public int CompressionBitLength { get; set; }
    }
}
