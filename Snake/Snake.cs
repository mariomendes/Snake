using System.Collections.Generic;
using System.Linq;

namespace Snake
{
    class Snake
    {
        public List<Point> BodyPoints;
        public Direction CurrentDirection { get; set; } = Direction.Right;
        public Point HeadPoint { get { return BodyPoints.First(); } }
        public Point TailPoint { get { return BodyPoints.Last(); } }

        public Snake()
        {
            BodyPoints = new List<Point>();

            for (int i = 3; i > 3 - Settings.SnakeStartLength; i--)
                BodyPoints.Add(new Point() { X = Settings.BoardWidth / 2 + i, Y = Settings.BoardHeight / 2, Type = PointType.Snake });
        }

        public Point NextHeadPoint()
        {
            int x = 0;
            int y = 0;
            int wallPassX = 0;
            int wallPassY = 0;

            switch (CurrentDirection)
            {
                case Direction.Right:
                    x = 1;
                    y = 0;
                    wallPassX = 1;
                    break;

                case Direction.Left:
                    x = -1;
                    y = 0;
                    wallPassX = Settings.BoardWidth - 2;
                    break;

                case Direction.Up:
                    x = 0;
                    y = -1;
                    wallPassY = Settings.BoardHeight - 2;
                    break;

                case Direction.Down:
                    x = 0;
                    y = 1;
                    wallPassY = 1;
                    break;
            }

            var nextHeadPoint = new Point { X = HeadPoint.X + x, Y = HeadPoint.Y + y, Type = PointType.Snake };

            if (Settings.WallTravel)
            {
                nextHeadPoint.X = nextHeadPoint.X == Settings.BoardWidth - 1 || nextHeadPoint.X == 0 ? wallPassX : nextHeadPoint.X;
                nextHeadPoint.Y = nextHeadPoint.Y == Settings.BoardHeight - 1 || nextHeadPoint.Y == 0 ? wallPassY : nextHeadPoint.Y;
            }

            return nextHeadPoint;
        }

        public void RemoveTail()
        {
            BodyPoints.Remove(this.TailPoint);
        }

        public void AddNewHead(Point nextHeadPoint)
        {
            BodyPoints.Insert(0, new Point() { X = nextHeadPoint.X, Y = nextHeadPoint.Y, Type = PointType.Snake });
        }
    }
}
