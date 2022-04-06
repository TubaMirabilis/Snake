namespace Snake
{
    class SnakePoint
    {
        public SnakePoint(int x, int y)
        {
            X = x;
            Y = y;
            this.Direction = Bearing.Left;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public Bearing Direction { get; set; }
        public enum Bearing
        {
            Left,
            Up,
            Right,
            Down
        }
    }
}
