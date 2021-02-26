namespace Perpetuum.Services.EventServices.EventMessages
{
    public class SpawnPortalMessage : EventMessage
    {
        public Position SourcePosition { get; private set; }
        public Position TargetPosition { get; private set; }
        public int SourceZone { get; private set; }
        public int TargetZone { get; private set; }
        public SpawnPortalMessage(int sourceZone, Position srcPosition, int targetZone, Position targetPosition)
        {
            SourcePosition = srcPosition;
            SourceZone = sourceZone;
            TargetPosition = targetPosition;
            TargetZone = targetZone;
        }
    }
}
