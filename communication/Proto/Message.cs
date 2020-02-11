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
        public int Address; //������/�����ߣ��򻷾�����
        public IMessage Content; //������

        public MessageDescriptor Descriptor => null;

        public int CalculateSize() //Ŀǰû�����Բ�ʵ��
        {
            return Content.CalculateSize() + 4 + Content.GetType().FullName.Length;
        }

        public void MergeFrom(Stream stream)
        {
            MergeFrom(new CodedInputStream(stream));
        }

        public void MergeFrom(CodedInputStream input)
        {
            Address = input.ReadInt32();
            string PacketType = input.ReadString();

            Content = Activator.CreateInstance(Type.GetType(PacketType)) as IMessage;

            Content.MergeFrom(input);
            Constants.Debug($"{PacketType} received ({Content.CalculateSize()} bytes)");
        }

        public void WriteTo(Stream stream)
        {
            using (CodedOutputStream costream = new CodedOutputStream(stream, true))
            {
                WriteTo(costream);
                costream.Flush();
            }
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
