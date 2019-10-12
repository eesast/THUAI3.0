namespace Communication.Proto
{
    public enum PacketType
    {
        ProtoPacket = 0,
        PlayerReady = 1,
        GameOver = 2,
        RequestInfo = 3,
        IdRequest=4,
        IdAllocate=5,
    }
}