using Perpetuum.EntityFramework;
using Perpetuum.ExportedTypes;
using Perpetuum.Log;
using Perpetuum.Services.EventServices.EventMessages;
using Perpetuum.Services.RiftSystem;
using Perpetuum.Zones;
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

        public override void HandleMessage(EventMessage value)
        {
            if (value is SpawnPortalMessage msg)
            {
                if(!_zoneManager.ContainsZone(msg.SourceZone) || !_zoneManager.ContainsZone(msg.TargetZone))
                {
                    Logger.DebugWarning($"{msg.SourceZone} or {msg.TargetZone} not a valid zone");
                    return;
                }
                //TODO
                var zone = _zoneManager.GetZone(msg.SourceZone);
                var zoneTarget = _zoneManager.GetZone(msg.TargetZone);
                var rift = (TargettedPortal)_entityServices.Factory.CreateWithRandomEID(DefinitionNames.TARGETTED_RIFT);
                rift.AddToZone(zone, msg.SourcePosition, ZoneEnterType.NpcSpawn);
                rift.SetTarget(zoneTarget, msg.TargetPosition);
                rift.SetDespawnTime(TimeSpan.FromMinutes(2.5));
                Logger.Info(string.Format("Rift spawned on zone {0} {1} ({2})", zone.Id, rift.ED.Name, rift.CurrentPosition));
            }
        }

    }
}
