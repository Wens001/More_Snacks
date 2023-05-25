using System.IO;
 
public class MyStream : FileStream
{

    public const byte KEY = 64;
    public MyStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
    {
    }
    public MyStream(string path, FileMode mode) : base(path, mode)
    {
    }
    public override int Read(byte[] array, int offset, int count)
    {
        var tkey = KEY * 3;
        int size = tkey;
        var index = base.Read(array, offset, count);
        for (int i = 0; i < array.Length; i += size)
        {
            array[i] ^= KEY;
            size += tkey;
        }
        return index;
    }
    public override void Write(byte[] array, int offset, int count)
    {
        WriteData(array);
        base.Write(array, offset, count);
    }

    public static void WriteData(byte[] array)
    {
        var tkey = KEY * 3;
        int size = tkey;
        for (int i = 0; i < array.Length; i += size)
        {
            array[i] ^= KEY;
            size += tkey;
        }
    }

}