using Ardalis.SmartEnum;

namespace HTWGame.Domain
{
    public abstract class Direction : SmartEnum<Direction>
    {
        public static readonly Direction Left = new LeftDirection();
        public static readonly Direction Up = new UpDirection();
        public static readonly Direction Right = new RightDirection();
        public static readonly Direction Down = new DownDirection();

        protected Direction(string name, int value) : base(name, value)
        {
        }

        public abstract Direction Opposite();

        private class LeftDirection : Direction
        {
            public LeftDirection() : base(nameof(Left), 0)
            {
            }

            public override Direction Opposite()
                => Right;
        }

        private class UpDirection : Direction
        {
            public UpDirection() : base(nameof(Up), 1)
            {
            }

            public override Direction Opposite()
                => Down;
        }

        private class RightDirection : Direction
        {
            public RightDirection() : base(nameof(Right), 2)
            {
            }

            public override Direction Opposite()
                => Left;
        }


        private class DownDirection : Direction
        {
            public DownDirection() : base(nameof(Down), 3)
            {
            }

            public override Direction Opposite()
                => Up;
        }
    }
}
