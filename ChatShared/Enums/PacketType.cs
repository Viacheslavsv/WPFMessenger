namespace ChatShared.Enums
{
    public enum PacketType : byte
    {
        Connection,
        FailedConnection,
        Message,
        Disconnection,
        Chat,
        Users
    }
}
