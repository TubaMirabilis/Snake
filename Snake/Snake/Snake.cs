using System.Collections.Generic;

namespace Snake
{
    class Snake
    {
        public Snake()
        {
            this.snakePoints = new List<SnakePoint>{
                new SnakePoint(8, 11),
                new SnakePoint(9,11),
                new SnakePoint(10,11),
                new SnakePoint(11,11),
                new SnakePoint(12,11)};
        }
        public List<SnakePoint> snakePoints { get; set; }
    }
}
