using System;
using System.IO;
using Google.Protobuf;

namespace PlayBack
{
    public class Writer
    {
        FileStream FileStream;
        BinaryWriter BinaryWriter;
        object privateLock = new object();

        public Writer(string path)
        {
            if (File.Exists(path))
                FileStream = new FileStream(path, FileMode.Truncate);
            else
                FileStream = new FileStream(path, FileMode.OpenOrCreate);
            BinaryWriter = new BinaryWriter(FileStream);
        }

        public void Write(IMessage message)
        {
            MemoryStream ostream = new MemoryStream();
            message.WriteTo(ostream);
            byte[] bytes = ostream.ToArray();
            BinaryWriter.Write(bytes.Length);
            BinaryWriter.Write(bytes);
            BinaryWriter.Flush();
        }

        ~Writer()
        {
            BinaryWriter.Close();
            BinaryWriter.Dispose();
            FileStream.Close();
            FileStream.Dispose();
        }
    }
}
