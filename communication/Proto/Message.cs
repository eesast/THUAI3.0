using Google.Protobuf;
using Google.Protobuf.Reflection;
using Logic.Constant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Communication.Proto
{
    internal class Message : IMessage //Communication内部使用的Message
    {
        public int Address; //发送者/接收者，因环境而定
        public IMessage Content; //包内容
        private static readonly List<Assembly> assemblyList = new List<Assembly>
        {
            Assembly.GetExecutingAssembly(),
            typeof(MessageToServer).Assembly
        };

        public MessageDescriptor Descriptor => null;

        private static Type GetType(string typename)
        {
            foreach (Assembly asm in assemblyList)
            {
                Type t = asm.GetType(typename);
                if (t != null) return t;
            }
            return null;
        }
        public int CalculateSize() //目前没用所以不实现
        {
            return Content.CalculateSize() + 4 + Content.GetType().FullName.Length;
        }

        public void MergeFrom(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Address = br.ReadInt32();
            string PacketType = br.ReadString(); //包类型的FullName
            Content = Activator.CreateInstance(GetType(PacketType)) as IMessage;
            Content.MergeFrom(stream);
            Constants.Debug($"{PacketType} received ({Content.CalculateSize()} bytes)");
        }

        public void MergeFrom(CodedInputStream input)
        {
            Address = input.ReadInt32();
            string PacketType = input.ReadString();
            Content = Activator.CreateInstance(GetType(PacketType)) as IMessage;
            Content.MergeFrom(input);
            Constants.Debug($"{PacketType} received ({Content.CalculateSize()} bytes)");
        }

        public void WriteTo(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Address);
            bw.Write(Content.GetType().FullName); //包类型的FullName
            Content.WriteTo(stream);
            Constants.Debug($"{Content.GetType().FullName} sent ({Content.CalculateSize()} bytes)");
        }

        public void WriteTo(CodedOutputStream output)
        {
            output.WriteInt32(Address);
            output.WriteString(Content.GetType().FullName);
            Content.WriteTo(output);
            Constants.Debug($"{Content.GetType().FullName} sent ({Content.CalculateSize()} bytes)");
        }
    }
}
