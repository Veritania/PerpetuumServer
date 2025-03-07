﻿using Perpetuum.StateMachines;
using Perpetuum.Units;
using Perpetuum.Zones.NpcSystem.Flocks;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Perpetuum.Zones.NpcSystem.Presences.PathFinders
{

    public class SpawnState : IState
    {
        private readonly IRoamingPresence _presence;
        private TimeSpan _delay = TimeSpan.Zero;

        private bool _spawning;
        private bool _spawned;
        private double _repawnDelayModifier = 0.0;

        public SpawnState(IRoamingPresence presence)
        {
            _presence = presence;
        }

        public void Enter()
        {
            _spawning = false;
            _spawned = false;

            _elapsed = TimeSpan.Zero;

            _delay = TimeSpan.FromSeconds(_presence.Configuration.RoamingRespawnSeconds * _repawnDelayModifier);
            _repawnDelayModifier = FastRandom.NextDouble(1.0, 2.0);
        }

        public void Exit()
        {

        }

        private TimeSpan _elapsed;

        //updated
        public void Update(TimeSpan time)
        {
            if (_spawning)
                return;

            if (_spawned)
            {
                _presence.StackFSM.Push(new RoamingState(_presence));
                return;
            }

            _elapsed += time;

            if (_elapsed < _delay)
                return;

            _spawning = true;
            Task.Run(() => SpawnFlocks()).ContinueWith(t =>
            {
                _spawned = true;
                _spawning = false;
            });
        }

        private void SpawnFlocks()
        {
            Position spawnPosition;
            bool anyPlayersAround;

            do
            {
                spawnPosition = _presence.PathFinder.FindSpawnPosition(_presence).ToPosition();
                anyPlayersAround = _presence.Zone.Players.WithinRange(spawnPosition, 200).Any();

            } while (anyPlayersAround);

            _presence.SpawnOrigin = spawnPosition;
            _presence.CurrentRoamingPosition = spawnPosition;
            _presence.Log("spawn position: " + spawnPosition);

            //spawn all flocks
            foreach (var flock in _presence.Flocks)
            {
                flock.SpawnAllMembers();
            }
        }
    }

    public class RoamingState : IState
    {
        private readonly IRoamingPresence _presence;

        public RoamingState(IRoamingPresence presence)
        {
            _presence = presence;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        private bool _finding;

        public void Update(TimeSpan time)
        {
            if (_finding)
                return;

            var members = _presence.Flocks.GetMembers().ToArray();
            if (members.Length <= 0)
            {
                _presence.StackFSM.Pop();
                return;
            }

            var idleMembersCount = members.Select(m => m.AI.Current).OfType<IdleAI>().Count();
            if (idleMembersCount < members.Length)
                return;

            _finding = true;
            Task.Run(() => FindNextRoamingPosition()).ContinueWith(t => _finding = false);
        }

        private void FindNextRoamingPosition()
        {
#if DEBUG
            _presence.Log("finding new roaming position. current: " + _presence.CurrentRoamingPosition);
#endif
            var nextRoamingPosition = _presence.PathFinder.FindNextRoamingPosition(_presence);
#if DEBUG
            _presence.Log("next roaming position: " + nextRoamingPosition + " dist:" + _presence.CurrentRoamingPosition.Distance(nextRoamingPosition));
#endif
            _presence.CurrentRoamingPosition = nextRoamingPosition;

            foreach (var npc in _presence.Flocks.GetMembers())
            {
                npc.HomePosition = _presence.CurrentRoamingPosition.ToPosition();
            }
        }
    }
}
