namespace Communication.Proto
{
    internal enum PacketType
    {
        ProtoPacket = 0, //���ݰ�(S2C,C2S)
        IdRequest = 1, //Client����ID(C2S)
        IdAllocate = 2, //Client�������ID(C2S)��Server��Client����ID(S2C)
        Disconnected = 3, //Server�����Ͽ�(S2C)
        Quit=4     // 退出房间
    }
}