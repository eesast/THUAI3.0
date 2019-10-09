using Google.Protobuf;
using System;
using System.IO;

namespace Communication.Proto
{
    public class Message
    {
        public IMessage Content{ get; set; }
        public int Client { get; set; }
        public void MergeFrom(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Client = br.ReadInt32();
            string PacketType = br.ReadString();
            Content = Activator.CreateInstance(Type.GetType(PacketType)) as IMessage;
            Content.MergeFrom(stream);
            Console.WriteLine($"{PacketType} received ({Content.CalculateSize()} bytes)");
        }
        public void WriteTo(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Client);
            bw.Write(Content.GetType().FullName);
            Content.WriteTo(stream);
            Console.WriteLine($"{Content.GetType().FullName} sent ({Content.CalculateSize()} bytes)");
        }
    }
}
