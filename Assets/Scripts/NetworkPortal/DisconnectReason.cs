public class DisconnectReason
{
    public ConnectionStatus Reason { get; private set; } = ConnectionStatus.Undefined;

    public void SetDisconnectReason(ConnectionStatus reason)
    {
        Reason = reason;
    }

    public void Clear()
    {
        Reason = ConnectionStatus.Undefined;
    }

    public bool HasTransitionReason => Reason != ConnectionStatus.Undefined;
}
