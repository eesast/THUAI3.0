namespace Communication.Proto
{
    internal enum PacketType
    {
        ProtoPacket = 0, //内容包(S2C,C2S)
        IdRequest = 1, //Client请求ID(C2S)
        IdAllocate = 2, //Client请求分配ID(C2S)，Server给Client分配ID(S2C)
        Disconnected = 3 //Server主动断开(S2C)
    }
}