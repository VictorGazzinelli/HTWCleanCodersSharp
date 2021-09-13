namespace HTWGame.Domain
{
    public class Connection
    {
        internal string from;
        internal string to;
        internal Direction direction;

        public Connection(string from, string to, Direction direction)
        {
            this.from = from;
            this.to = to;
            this.direction = direction;
        }
    }
}
