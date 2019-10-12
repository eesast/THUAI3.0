using Google.Protobuf;
using System;
using System.IO;

namespace Communication.Proto
{
    public class Message
    {
        public IMessage Content{ get; set; }
        public int Client { get; set; }
        public int Agent { get; set; }
        public void MergeFrom(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Console.Write(br.PeekChar());
            Agent = br.ReadInt32();
            Client = br.ReadInt32();
            br.ReadInt32();
            string PacketType = br.ReadString();
            Console.WriteLine(PacketType);
            Content = Activator.CreateInstance(Type.GetType(PacketType)) as IMessage;
            Content.MergeFrom(stream);
            Console.WriteLine($"{PacketType} received ({Content.CalculateSize()} bytes)");
        }
        public void WriteTo(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(BitConverter.GetBytes(Client), 0, 4);
            bw.Write(Content.GetType().FullName);
            Content.WriteTo(stream);
            Console.WriteLine($"{Content.GetType().FullName} sent ({Content.CalculateSize()} bytes)");
        }
    }
}
