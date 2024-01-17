namespace MusicX.Core.Models;

public class ListenTogetherMethods
{
    public const string ChangeTrack = "ChangeTrack";

    public const string ChangePlayState = "ChangePlayState";

    public const string StartPlaySession = "StartPlaySession";

    public const string StopPlaySession = "StopPlaySession";

    public const string JoinPlaySession = "JoinPlaySession";

    public const string LeavePlaySession = "LeavePlaySession";

    public const string GetCurrentTrack = "GetCurrentTrack";

    public const string GetSessionOwnerInfo = "GetSessionOwnerInfo";

    public const string GetListenersInSession = "GetListenersInSession";
}

public enum PlayerMode
{
    None,

    Owner,

    Listener
}