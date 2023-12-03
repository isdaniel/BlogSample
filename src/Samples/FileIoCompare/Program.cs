using System;
using System.IO;
using System.IO.MemoryMappedFiles;

public class FileIoCompare
{
    public const string source = "C:/bk-bloating.sql";
    
    public static void Main(string[] args)
    {
        SendFileCopy("C:/bk-bloating1.sql");
        OldFileCopy("C:/bk-bloating2.sql");
        MmapFileCopy("C:/bk-bloating3.sql");
    }

    //DMA
    private static void SendFileCopy(string dest){
        try
        {
            using (FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            using (FileStream destStream = new FileStream(dest, FileMode.Create, FileAccess.Write))
            {
                long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                sourceStream.CopyTo(destStream);
                Console.WriteLine("SendFileCopy time cost : " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void OldFileCopy(string dest){
        try
        {
            using (FileStream inputStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            using (FileStream outputStream = new FileStream(dest, FileMode.Create, FileAccess.Write))
            {
                long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                byte[] buffer = new byte[4096];
                int read;
                long total = 0;

                while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    total += read;
                    outputStream.Write(buffer, 0, read);
                }

                outputStream.Flush();
                Console.WriteLine("OldFileCopy time cost：" + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    //mmap
    static void MmapFileCopy(string dest){
        try
        {
            using (FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            using (FileStream destStream = new FileStream(dest, FileMode.Create, FileAccess.Write))
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateNew(null, sourceStream.Length))
                {
                    using (MemoryMappedViewStream mmfStream = mmf.CreateViewStream())
                    {
                        long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        sourceStream.CopyTo(mmfStream);
                        mmfStream.Seek(0, SeekOrigin.Begin);
                        mmfStream.CopyTo(destStream);
                        Console.WriteLine("MmapFileCopy time cost：" + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}