using System;
using System.Linq;
using System.Collections.Generic;

namespace HTWGame.Domain
{
    public class HuntTheWumpusGame : IHuntTheWumpus
    {
        private readonly IList<Connection> _connections = new List<Connection>();

        private readonly ISet<string> _caverns = new HashSet<string>();
        private string _playerCavern = "NONE";
        private readonly IHuntTheWumpusMessageReceiver _messageReceiver;
        private readonly ISet<string> _batCaverns = new HashSet<string>();
        private readonly ISet<string> _pitCaverns = new HashSet<string>();
        private string _wumpusCavern = "NONE";
        private int _quiver = 0;
        private readonly IDictionary<string, int> _arrowsIn = new Dictionary<string, int>();

        private static readonly Random _random = new Random();

        public HuntTheWumpusGame(IHuntTheWumpusMessageReceiver receiver)
        {
            _messageReceiver = receiver;
        }

        public void SetPlayerCavern(string playerCavern)
            => _playerCavern = playerCavern;

        public string GetPlayerCavern()
            => _playerCavern;

        private void ReportStatus()
        {
            ReportAvailableDirections();
            if (ReportNearby(c => _batCaverns.Contains(c.to)))
                _messageReceiver.HearBats();
            if (ReportNearby(c => _pitCaverns.Contains(c.to)))
                _messageReceiver.HearPit();
            if (ReportNearby(c => _wumpusCavern.Equals(c.to)))
                _messageReceiver.SmellWumpus();
        }

        private bool ReportNearby(Predicate<Connection> nearTest)
            => _connections.Any(c => _playerCavern.Equals(c.from) && nearTest.Invoke(c));

        private void ReportAvailableDirections()
            => _connections.Where(c => _playerCavern.Equals(c.from))
                           .ToList()
                           .ForEach(c => _messageReceiver.Passage(c.direction));


        public void AddBatCavern(string cavern)
            => _batCaverns.Add(cavern);

        public void AddPitCavern(string cavern)
            => _pitCaverns.Add(cavern);

        public void SetWumpusCavern(string wumpusCavern)
            => _wumpusCavern = wumpusCavern;

        public string GetWumpusCavern()
            => _wumpusCavern;

        private void MoveWumpus()
        {
            IEnumerable<string> wumpusChoices = _connections.Where(c => _wumpusCavern.Equals(c.from))
                                                            .Select(c => c.to)
                                                            .Concat(new string[]{ _wumpusCavern });
            int nChoices = wumpusChoices.Count();
            int choice = _random.Next(nChoices);
            _wumpusCavern = wumpusChoices.ElementAt(choice);
        }

        private void RandomlyTransportPlayer()
        {
            IEnumerable<string> transportChoices = _caverns.Except(new string[] { _playerCavern });
            int nChoices = transportChoices.Count();
            int choice = _random.Next(nChoices);
            _playerCavern = transportChoices.ElementAt(choice);
        }

        public void SetQuiver(int arrows)
            => _quiver = arrows;

        public int GetQuiver()
            => _quiver;

        public int GetArrowsInCavern(string cavern)
            => _arrowsIn.ContainsKey(cavern) ? _arrowsIn[cavern] : 0;

        public void ConnectCavern(string from, string to, Direction direction)
        {
            _connections.Add(new Connection(from, to, direction));
            _caverns.Add(from);
            _caverns.Add(to);
        }

        public string FindDestination(string cavern, Direction direction)
            => _connections.FirstOrDefault(c => c.from.Equals(cavern) && c.direction == direction)?.to;

        public ICommand MakeRestCommand()
            => new RestCommand(this);

        public ICommand MakeShootCommand(Direction direction)
            => new ShootCommand(this, direction);
        public ICommand MakeMoveCommand(Direction direction)
            => new MoveCommand(this, direction);

        public void SetArrowsIn(string cavern, int arrows)
            => _arrowsIn[cavern] = arrows;

        public abstract class GameCommand : ICommand
        {
            protected readonly HuntTheWumpusGame _game;

            protected GameCommand(HuntTheWumpusGame game)
            {
                _game = game;
            }

            public void Execute()
            {
                ProcessCommand();
                _game.MoveWumpus();
                CheckWumpusMovedToPlayer();
                _game.ReportStatus();
            }

            private void CheckWumpusMovedToPlayer()
            {
                if (_game._playerCavern.Equals(_game._wumpusCavern))
                    _game._messageReceiver.WumpusMovesToPlayer();
            }

            protected abstract void ProcessCommand();
        }

        private class RestCommand : GameCommand
        {
            public RestCommand(HuntTheWumpusGame game) : base(game)
            {

            }

            protected override void ProcessCommand()
            {
                
            }
        }

        public abstract class DirectionalGameCommand : GameCommand
        {
            protected readonly Direction _direction;

            protected DirectionalGameCommand(HuntTheWumpusGame game, Direction direction) : base(game)
            {
                _direction = direction;
            }
        }

        private class ShootCommand : DirectionalGameCommand
        {
            public ShootCommand(HuntTheWumpusGame game, Direction direction) : base(game, direction)
            {

            }

            protected override void ProcessCommand()
            {
                if (_game._quiver == 0)
                    _game._messageReceiver.NoArrows();
                else
                {
                    _game._messageReceiver.ArrowShot();
                    _game._quiver--;
                    ArrowTracker arrowTracker = new ArrowTracker(this, _game._playerCavern).TrackArrow(_direction);
                    if (arrowTracker.ArrowHitSomething())
                        return;
                    IncrementArrowsInCavern(arrowTracker.ArrowCavern);
                }
            }

            private void IncrementArrowsInCavern(string cavern)
                => _game._arrowsIn[cavern] = _game._arrowsIn[cavern] + 1;

            private class ArrowTracker
            {
                private readonly ShootCommand _shootCommand;
                private bool _hitSomething;
                private string _arrowCavern;

                public ArrowTracker(ShootCommand shootCommand, string startingCavern)
                {
                    _shootCommand = shootCommand;
                    _arrowCavern = startingCavern;
                }

                internal bool ArrowHitSomething()
                    => _hitSomething;

                internal string ArrowCavern =>
                    _arrowCavern;

                public ArrowTracker TrackArrow(Direction direction)
                {
                    string nextCavern;
                    for (int i = 0; (nextCavern = GetNextCavern(_arrowCavern, direction)) != null; i++)
                    {
                        _arrowCavern = nextCavern;
                        if (ShotSelfInBack() || ShotWumpus() || i > 100)
                            return this;
                    }
                    if (_arrowCavern.Equals(_shootCommand._game._playerCavern))
                        _shootCommand._game._messageReceiver.PlayerShootsWall();
                    return this;
                }

                private string GetNextCavern(string cavern, Direction direction)
                    => _shootCommand._game._connections.FirstOrDefault(c => c.from.Equals(cavern) && c.direction.Equals(direction))?.to;

                private bool ShotSelfInBack()
                {
                    if (_shootCommand._game._playerCavern.Equals(_arrowCavern))
                    {
                        _shootCommand._game._messageReceiver.PlayerShootsSelfInBack();
                        _hitSomething = true;
                        return true;
                    }
                    return false;
                }

                private bool ShotWumpus()
                {
                    if (_shootCommand._game._wumpusCavern.Equals(_arrowCavern))
                    {
                        _shootCommand._game._messageReceiver.PlayerKillsWumpus();
                        _hitSomething = true;
                        return true;
                    }
                    return false;
                }

            }
        }

        private class MoveCommand : DirectionalGameCommand
        {
            public MoveCommand(HuntTheWumpusGame game, Direction direction) : base(game, direction)
            {

            }

            protected override void ProcessCommand()
            {
                if (MovePlayer(_direction))
                {
                    CheckForWumpus();
                    CheckForPit();
                    CheckForBats();
                    CheckForArrows();
                }
                else
                {
                    _game._messageReceiver.NoPassage();
                }
            }

            public bool MovePlayer(Direction direction)
            {
                string destination = _game.FindDestination(_game._playerCavern, direction);
                if (destination != null)
                {
                    _game._playerCavern = destination;
                    return true;
                }
                return false;
            }

            private void CheckForWumpus()
            {
                if (_game._wumpusCavern.Equals(_game._playerCavern))
                    _game._messageReceiver.PlayerMovesToWumpus();
            }

            private void CheckForBats()
            {
                if (_game._batCaverns.Contains(_game._playerCavern))
                {
                    _game._messageReceiver.BatsTransport();
                    _game.RandomlyTransportPlayer();
                }
            }

            private void CheckForPit()
            {
                if (_game._pitCaverns.Contains(_game._playerCavern))
                    _game._messageReceiver.FellInPit();
            }

            private void CheckForArrows()
            {
                int arrowsFound = _game.GetArrowsInCavern(_game._playerCavern);
                if (arrowsFound > 0)
                    _game._messageReceiver.ArrowsFound(arrowsFound);
                _game._quiver += arrowsFound;
                _game._arrowsIn[_game._playerCavern] = 0;
            }
        }
    }
}
