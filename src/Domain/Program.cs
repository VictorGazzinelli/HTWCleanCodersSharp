using System;
using System.Collections.Generic;

namespace HTWGame.Domain
{
    public class Program : IHuntTheWumpusMessageReceiver
    {
        private static int _hitPoints = 10;
        private static IHuntTheWumpus _game;
        private static readonly Random _random = new Random();
        private static readonly IList<string> _caverns = new List<string>();
        private static readonly string[] _environments = new string[] { 
            "bright",
            "humid",
            "dry",
            "creepy",
            "ugly",
            "foggy",
            "hot",
            "cold",
            "drafty",
            "dreadful" 
        };
        private static readonly string[] _shapes = new string[] {
            "round",
            "square",
            "oval",
            "irregular",
            "long",
            "craggy",
            "rough",
            "tall",
            "narrow" 
        };
        private static readonly string[] _cavernTypes = new string[] { 
            "cavern",
            "room",
            "chamber",
            "catacomb",
            "crevasse",
            "cell",
            "tunnel",
            "passageway",
            "hall",
            "expanse" 
        };
        private static readonly string[] _adornments = new string[] { 
            "smelling of sulphur",
            "with engravings on the walls",
            "with a bumpy floor",
            "",
            "littered with garbage",
            "spattered with guano",
            "with piles of Wumpus droppings",
            "with bones scattered around",
            "with a corpse on the floor",
            "that seems to vibrate",
            "that feels stuffy",
            "that fills you with dread" 
        };

        public static void Main(string [] args)
        {
            _game = HuntTheWumpusFactory.MakeGame("HTWGame.Domain.HuntTheWumpusGame", new Program());
            CreateMap();
            _game.MakeRestCommand().Execute();
            while (true)
            {
                Console.WriteLine(_game.GetPlayerCavern());
                Console.WriteLine($"Health: {_hitPoints} arrows: {_game.GetQuiver()}");
                ICommand command = _game.MakeRestCommand();
                Console.Write(">");
                string input = Console.ReadLine();
                if (input.Equals("w"))
                    command = _game.MakeMoveCommand(Direction.Up);
                else if (input.Equals("a"))
                    command = _game.MakeMoveCommand(Direction.Left);
                else if (input.Equals("d"))
                    command = _game.MakeMoveCommand(Direction.Right);
                else if (input.Equals("s"))
                    command = _game.MakeMoveCommand(Direction.Down);
                else if (input.Equals("r"))
                    command = _game.MakeRestCommand();
                else if (input.Equals("sw"))
                    command = _game.MakeShootCommand(Direction.Up);
                else if (input.Equals("sa"))
                    command = _game.MakeShootCommand(Direction.Left);
                else if (input.Equals("ss"))
                    command = _game.MakeShootCommand(Direction.Down);
                else if (input.Equals("sd"))
                    command = _game.MakeShootCommand(Direction.Right);
                else if (input.Equals("q"))
                    return;

                command.Execute();
            }
        }

        private static void CreateMap()
        {
            int ncaverns = _random.Next(31) + 10;
            while (ncaverns-- > 0)
            {
                string cavern = MakeName();
                _caverns.Add(cavern);
                _game.SetArrowsIn(cavern, 0);
            }

            foreach (string cavern in _caverns)
            {
                MaybeConnectCavern(cavern, Direction.Up);
                MaybeConnectCavern(cavern, Direction.Down);
                MaybeConnectCavern(cavern, Direction.Left);
                MaybeConnectCavern(cavern, Direction.Right);
            }

            string playerCavern = AnyCavern();
            _game.SetPlayerCavern(playerCavern);
            _game.SetWumpusCavern(AnyOther(playerCavern));
            _game.AddBatCavern(AnyOther(playerCavern));
            _game.AddBatCavern(AnyOther(playerCavern));
            _game.AddBatCavern(AnyOther(playerCavern));

            _game.AddPitCavern(AnyOther(playerCavern));
            _game.AddPitCavern(AnyOther(playerCavern));
            _game.AddPitCavern(AnyOther(playerCavern));

            _game.SetQuiver(5);
        }

        private static string MakeName()
            => $"A {ChooseName(_environments)} {ChooseName(_shapes)} {ChooseName(_cavernTypes)} {ChooseName(_adornments)}.";

        private static string ChooseName(string[] names)
        {
            int n = names.Length;
            int choice = _random.Next(n);
            return names[choice];
        }

        private static void MaybeConnectCavern(string cavern, Direction direction)
        {
            if (_random.NextDouble() > .2)
            {
                string other = AnyOther(cavern);
                ConnectIfAvailable(cavern, direction, other);
                ConnectIfAvailable(other, direction.Opposite(), cavern);
            }
        }

        private static void ConnectIfAvailable(string from, Direction direction, string to)
        {
            if (_game.FindDestination(from, direction) == null)
                _game.ConnectCavern(from, to, direction);
        }

        private static string AnyOther(string cavern)
        {
            string otherCavern = cavern;
            while (cavern.Equals(otherCavern))
                otherCavern = AnyCavern();
            return otherCavern;
        }

        private static string AnyCavern()
            => _caverns[_random.Next(_caverns.Count)];

        public void NoPassage()
            => Console.WriteLine("No Passage.");

        public void HearBats()
            => Console.WriteLine("You hear chirping.");
        
        public void HearPit()
            => Console.WriteLine("You hear wind.");

        public void SmellWumpus()
            => Console.WriteLine("There is a terrible smell");

        public void Passage(Direction direction)
            => Console.WriteLine($"You can go {direction.Name}.");

        public void NoArrows()
            => Console.WriteLine("You have no arrows.");

        public void ArrowShot()
            => Console.WriteLine("Thwang");

        public void PlayerShootsSelfInBack()
        {
            Console.WriteLine("Ow!  You shot yourself in the back.");
            Hit(3);
        }

        public void PlayerKillsWumpus()
        {
            Console.WriteLine("You killed the Wumpus.");
            Environment.Exit(0);
        }

        public void PlayerShootsWall()
            => Console.WriteLine("You shot a wall.");


        public void ArrowsFound(int arrowsFound)
            => Console.WriteLine($"You found {arrowsFound} arrow{GetPlural(arrowsFound)}.");

        public void FellInPit()
        {
            Console.WriteLine("You fell in a pit and hurt yourself.");
            Hit(4);
        }

        public void PlayerMovesToWumpus()
        {
            Console.WriteLine("You walked into the waiting arms of the Wumpus.");
            Environment.Exit(0);
        }

        public void WumpusMovesToPlayer()
        {
            Console.WriteLine("The Wumpus has found you.");
            Environment.Exit(0);
        }

        public void BatsTransport()
            => Console.WriteLine("Some bats carried you away.");

        private string GetPlural(int arrowsFound)
            => arrowsFound == 1 ? "" : "s";

        private void Hit(int points)
        {
            _hitPoints -= points;
            if (_hitPoints <= 0)
            {
                Console.WriteLine("You have died of your wounds.");
                Environment.Exit(0);
            }
        }
    }
}
