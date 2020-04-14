using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Communication.Proto
{
    internal class Message : IMessage //Communication�ڲ�ʹ�õ�Message 
    {
        public int Address;
        public IMessage Content;

        public MessageDescriptor Descriptor => null;

        public int CalculateSize()
        {
            return Content.CalculateSize() + 4 + Content.GetType().FullName.Length;
        }

        public void MergeFrom(Stream stream)
        {
            try
            {
                MergeFrom(new CodedInputStream(stream));
            }
            catch (Exception e)
            {
                Constants.Error($"Unhandled exception while trying to deserialize packet: {e}");
                //stream should be instance of MemoryStream only.
                Constants.Error($"Packet: {string.Concat((stream as MemoryStream).ToArray().Select((b) => $"{b:X2} "))}");
            }
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
