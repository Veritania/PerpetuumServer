using Perpetuum.Collections;
using Perpetuum.Data;
using Perpetuum.Zones;
using Perpetuum.Zones.Finders.PositionFinders;
using Perpetuum.Zones.Terrains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Perpetuum.Services.RiftSystem
{
    public interface ICustomRiftConfigReader
    {
        IEnumerable<CustomRiftConfig> RiftConfigs { get; }
        CustomRiftConfig GetById(int id);
    }

    public class CustomRiftConfigReader : ICustomRiftConfigReader
    {
        public IEnumerable<CustomRiftConfig> RiftConfigs
        {
            get { return _riftconfigs.Values; }
        }
        private readonly IDictionary<int, CustomRiftConfig> _riftconfigs;

        public CustomRiftConfigReader()
        {
            _riftconfigs = Database.CreateCache<int, CustomRiftConfig>("riftconfigs", "id", r =>
            {
                var id = r.GetValue<int>("id");
                var name = r.GetValue<string>("name");
                var destinationGroupId = r.GetValue<int>("destinationGroupId");
                var lifetimeSeconds = r.GetValue<int?>("lifespanSeconds") ?? 0;
                var maxUses = r.GetValue<int?>("maxUses") ?? -1;
                var onlyOne = r.GetValue<bool>("onlyOne");

                var group = Db.Query().CommandText(
                    @"SELECT id, groupId, zoneId, x, y, weight FROM riftdestinations WHERE groupId=@groupId;")
                    .SetParameter("@groupId", destinationGroupId)
                    .Execute()
                    .Select((record) =>
                    {
                        var groupId = record.GetValue<int>("groupId");
                        var zoneId = record.GetValue<int>("zoneId");
                        var x = record.GetValue<int?>("x");
                        var y = record.GetValue<int?>("y");
                        var weight = record.GetValue<int>("weight");

                        return new Destination(groupId, zoneId, x, y, weight);
                    });
                var collection = new WeightedCollection<Destination>();
                foreach (var destination in group)
                {
                    collection.Add(destination, destination.Weight);
                }
                return new CustomRiftConfig(id, name, collection, maxUses, onlyOne, TimeSpan.FromSeconds(lifetimeSeconds));
            });
        }

        public CustomRiftConfig GetById(int id)
        {
            return RiftConfigs.FirstOrDefault(r => r.Id == id);
        }
    }

    public class CustomRiftConfig
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool OnlyOne { get; private set; }
        public TimeSpan Lifespan { get; private set; }
        public int MaxUses { get; private set; }
        private readonly WeightedCollection<Destination> _destinations;

        public CustomRiftConfig(int id, string name, WeightedCollection<Destination> destinations, int maxUses, bool oneOnly, TimeSpan lifespan)
        {
            Id = id;
            Name = name;
            _destinations = destinations;
            MaxUses = maxUses;
            OnlyOne = oneOnly;
            Lifespan = lifespan;
        }

        public bool IsDespawning { get { return !Lifespan.Equals(TimeSpan.Zero); } }
        public bool InfiniteUses { get { return MaxUses < 0; } }

        public Destination GetDestination()
        {
            return _destinations.GetRandom();
        }
    }


    public class Destination
    {
        private Position Location { get; set; }
        public bool IsRandomLocation { get { return Location.Equals(Position.Empty); } }
        public int ZoneId { get; private set; }
        public int Weight { get; private set; }
        public int Group { get; private set; }

        public Destination(int groupId, int zoneId, int? x, int? y, int weight = 1)
        {
            Group = groupId;
            ZoneId = zoneId;
            Location = x == null || y == null ? Position.Empty : new Position(x ?? 0, y ?? 0);
            Weight = weight;
        }

        public Position GetPosition(IZone zone)
        {
            if (IsRandomLocation)
            {
                var randomFinder = new RandomPassablePositionFinder(zone);
                if (randomFinder.Find(out Position random))
                {
                    return random;
                }
            }
            var closestFinder = new ClosestWalkablePositionFinder(zone, Location);
            if (closestFinder.Find(out Position closest))
            {
                return closest;
            }
            return Location;
        }
    }
}
