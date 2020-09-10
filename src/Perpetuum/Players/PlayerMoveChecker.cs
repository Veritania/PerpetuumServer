using Perpetuum.PathFinders;
using Perpetuum.Zones;
using System;

namespace Perpetuum.Players
{
    public class PlayerMoveChecker
    {
        private Position _prev;
        private readonly object _lock = new object();
        public Position GetPrev()
        {
            lock (_lock)
                return _prev;
        }
        public void SetPrev(Position prev)
        {
            lock (_lock)
                _prev = prev;
        }
        private readonly Player _player;
        private readonly AStarLimited _aStar;
        private const int MAX_DIST = 10;

        public PlayerMoveChecker(Player player)
        {
            _player = player;
            _aStar = new AStarLimited(Heuristic.Manhattan, _player.IsWalkable, MAX_DIST);
            SetPrev(player.CurrentPosition);
        }

        public bool IsUpdateValid(Position pos)
        {
            var prev = GetPrev();
            var dx = Math.Abs(prev.intX - pos.intX);
            var dy = Math.Abs(prev.intY - pos.intY);
            if (dx < 2 && dy < 2)
            {
                return true;
            }
            else if (dx > MAX_DIST || dy > MAX_DIST)
            {
                return false;
            }
            else if (_player.Zone.CheckLinearPath(prev, pos, _player.Slope))
            {
                return true;
            }
            else if (_aStar.HasPath(prev.ToPoint(), pos.ToPoint()))
            {
                return true;
            }
            return false;
        }
    }
}
