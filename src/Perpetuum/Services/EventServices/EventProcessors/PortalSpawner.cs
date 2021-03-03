using Perpetuum.EntityFramework;
using Perpetuum.ExportedTypes;
using Perpetuum.Log;
using Perpetuum.Services.EventServices.EventMessages;
using Perpetuum.Services.RiftSystem;
using Perpetuum.Zones;
using Perpetuum.Zones.Terrains;
using System;

namespace Perpetuum.Services.EventServices.EventProcessors
{
    public class PortalSpawner : EventProcessor
    {
        private readonly IEntityServices _entityServices;
        private readonly IZoneManager _zoneManager;

        public PortalSpawner(IEntityServices entityServices, IZoneManager zoneManager)
        {
            _entityServices = entityServices;
            _zoneManager = zoneManager;
        }

        private bool IsValid(SpawnPortalMessage msg)
        {
            if (msg.RiftConfig == null || !_zoneManager.ContainsZone(msg.SourceZone))
                return false;
            if (!_zoneManager.GetZone(msg.SourceZone).IsWalkable(msg.SourcePosition))
                return false;
            return true;
        }

        private Destination TryGetValidDestination(CustomRiftConfig riftConfig)
        {
            var attempts = 100;
            while (attempts > 0)
            {
                attempts--;
                var dest = riftConfig.GetDestination();
                if (dest != null && _zoneManager.ContainsZone(dest.ZoneId))
                {
                    return dest;
                }
            }
            return null;
        }

        public override void HandleMessage(EventMessage value)
        {
            if (value is SpawnPortalMessage msg)
            {
                if (!msg.IsValid(_zoneManager))
                    return;

                var zone = _zoneManager.GetZone(msg.SourceZone);
                var targetDestination = TryGetValidDestination(msg.RiftConfig);
                if (targetDestination == null)
                    return;

                var zoneTarget = _zoneManager.GetZone(targetDestination.ZoneId);
                var targetPos = targetDestination.GetPosition(zoneTarget);
                var rift = (TargettedPortal)_entityServices.Factory.CreateWithRandomEID(DefinitionNames.TARGETTED_RIFT);
                rift.AddToZone(zone, msg.SourcePosition, ZoneEnterType.NpcSpawn);
                rift.SetTarget(zoneTarget, targetPos);
                rift.SetConfig(msg.RiftConfig);

                Logger.Info(string.Format("Rift spawned on zone {0} {1} ({2})", zone.Id, rift.ED.Name, rift.CurrentPosition));
            }
        }

    }
}
