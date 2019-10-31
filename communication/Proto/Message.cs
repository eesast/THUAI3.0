using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Communication.Proto
{
    internal class Message : IMessage //Communication�ڲ�ʹ�õ�Message
    {
        private static readonly List<Assembly> assemblyList = new List<Assembly> //��Ҫ��������ͳ���
        {
            Assembly.GetExecutingAssembly()
        };
        public int Address; //������/�����ߣ��򻷾�����
        public IMessage Content; //������

        public MessageDescriptor Descriptor => null;

        private static Type getType(string typename)
        {
            foreach (Assembly asm in assemblyList)
            {
                Type t = asm.GetType(typename);
                if (t != null) return t;
            }
            return null;
        }

        public int CalculateSize() //Ŀǰû�����Բ�ʵ��
        {
            return Content.CalculateSize() + 4 + Content.GetType().FullName.Length;
        }

        public void MergeFrom(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Address = br.ReadInt32();
            string PacketType = br.ReadString(); //�����͵�FullName
            Content = Activator.CreateInstance(getType(PacketType)) as IMessage;
            Content.MergeFrom(stream);
            Constants.Debug($"{PacketType} received ({Content.CalculateSize()} bytes)");
        }

        public void MergeFrom(CodedInputStream input)
        {
            Address = input.ReadInt32();
            string PacketType = input.ReadString();
            Content = Activator.CreateInstance(getType(PacketType)) as IMessage;
            Content.MergeFrom(input);
            Constants.Debug($"{PacketType} received ({Content.CalculateSize()} bytes)");
        }

        public void WriteTo(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Address);
            bw.Write(Content.GetType().FullName); //�����͵�FullName
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
