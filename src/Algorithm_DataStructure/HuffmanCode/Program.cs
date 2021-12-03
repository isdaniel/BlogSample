using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Huffman
{

    sealed class Program
    {
        static void Main(string[] args)
        {
            // string input = "I like like c# elephant lion!! 你好世界";
            // HuffmanCompression huffman = new HuffmanCompression();
            // var compressResult = huffman.Zip(input);  
            // var originalData = huffman.UnZip(compressResult.compressBuffer, compressResult.bitLength);

            // int originalLenght = Encoding.UTF8.GetBytes(input).Length;
            // int compressionLenght = compressResult.compressBuffer.Length;

            // Console.WriteLine($"Compress percentage:{(originalLenght - compressionLenght) / (decimal)originalLenght}");
            // Console.WriteLine($"Data is : {Encoding.UTF8.GetString(originalData)}");

            Compression huffman = new Compression();
            huffman.ZipFile(@"./CompressionSample/Test.bmp",@"./CompressionSample/compressionData");
            huffman.UnZipFile(@"./CompressionSample/compressionData",@"./CompressionSample/new_Test.bmp");
        }
    }
}
