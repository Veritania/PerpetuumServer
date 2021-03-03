using Perpetuum.Services.RiftSystem;
using Perpetuum.Zones;

namespace Perpetuum.Services.EventServices.EventMessages
{
    public class SpawnPortalMessage : EventMessage
    {
        public Position SourcePosition { get; private set; }
        public int SourceZone { get; private set; }
        public CustomRiftConfig RiftConfig { get; private set; }
        public SpawnPortalMessage(int sourceZone, Position srcPosition, CustomRiftConfig riftConfig)
        {
            SourcePosition = srcPosition;
            SourceZone = sourceZone;
            RiftConfig = riftConfig;
        }

        public bool IsValid(IZoneManager zoneManager)
        {
            if (RiftConfig == null || !zoneManager.ContainsZone(SourceZone))
                return false;
            if (!zoneManager.GetZone(SourceZone).IsWalkable(SourcePosition))
                return false;
            return true;
        }
    }
}
