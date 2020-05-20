using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Google.Protobuf;

public class PlaybackReader
{
    FileStream FileStream;
    BinaryReader BinaryReader;
    public PlaybackReader(string path)
    {
        FileStream = new FileStream(path, FileMode.Open);
        BinaryReader = new BinaryReader(FileStream);
    }

    public bool Read<T>(ref IMessage<T> message) where T : IMessage<T>
    {
        if (BinaryReader.BaseStream.Position >= BinaryReader.BaseStream.Length)
            return false;
        Int32 length = BinaryReader.ReadInt32();
        if (BinaryReader.BaseStream.Position >= BinaryReader.BaseStream.Length)
            return false;
        message.MergeFrom(BinaryReader.ReadBytes(length));
        return true;
    }

    ~PlaybackReader()
    {
        BinaryReader.Close();
        FileStream.Close();
    }
}