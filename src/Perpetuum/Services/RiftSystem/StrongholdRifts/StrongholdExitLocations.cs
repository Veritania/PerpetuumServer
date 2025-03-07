﻿using Perpetuum.Data;
using Perpetuum.Zones;
using System.Collections.Generic;
using System.Linq;

namespace Perpetuum.Services.RiftSystem.StrongholdRifts
{
    /// <summary>
    /// DB query for StrongholdRiftLocation
    /// </summary>
    public static class StrongholdRiftLocationRepository
    {
        /// <summary>
        /// Get all StrongholdRiftLocations for the provided zone
        /// </summary>
        /// <param name="zone">IZone</param>
        /// <returns>IEnumerable of StrongholdRiftLocation</returns>
        public static IEnumerable<StrongholdRiftLocation> GetAllInZone(IZone zone)
        {
            var locations = Db.Query().CommandText("SELECT id, zoneid, x, y FROM strongholdexitconfig WHERE zoneid = @zoneId")
                .SetParameter("@zoneId", zone.Id)
                .Execute()
                .Select((record) =>
                {
                    var x = record.GetValue<int>("x");
                    var y = record.GetValue<int>("y");
                    return new StrongholdRiftLocation(zone, new Position(x, y));
                });

            return locations;
        }
    }

    /// <summary>
    /// Spawn of StrongholdExitRift
    /// </summary>
    public class StrongholdRiftLocation
    {
        public IZone Zone { get; private set; }
        public Position Location { get; private set; }

        public StrongholdRiftLocation(IZone zone, Position location)
        {
            Zone = zone;
            Location = location;
        }
    }
}
