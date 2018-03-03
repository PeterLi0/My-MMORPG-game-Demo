namespace LunaNav
{
    public enum MoveRequestState
    {
        TargetNone,
        TargetFailed,
        TargetValid,
        TargetRequesting,
        TargetWaitingForQueue,
        TargetWaitingForPath,
        TargetVelocity
    }
}