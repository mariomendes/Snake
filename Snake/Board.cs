namespace Snake
{
    class Board
    {
        public Point[,] points;

        public Point this[int x, int y]
        {
            get { return points[y, x]; }
            set { points[y, x] = value; }
        }

        public Board(Snake snake)
        {
            points = new Point[Settings.BoardHeight, Settings.BoardWidth];
            SetupBoard();
            snake.BodyPoints.ForEach(bodyPoint => { this[bodyPoint.X, bodyPoint.Y].Type = PointType.Snake; });
        }

        private void SetupBoard()
        {
            //left border
            for (int y = 0; y < Settings.BoardHeight; y++)
                this[0, y] = new Point { Type = PointType.Border, X = 0, Y = y };

            //top border
            for (int x = 1; x < Settings.BoardWidth; x++)
                this[x, 0] = new Point { Type = PointType.Border, X = x, Y = 0 };

            //bottom border
            for (int x = 1; x < Settings.BoardWidth; x++)
                this[x, Settings.BoardHeight - 1] = new Point { Type = PointType.Border, X = x, Y = Settings.BoardHeight - 1 };

            //right border
            for (int y = 0; y < Settings.BoardHeight; y++)
                this[Settings.BoardWidth - 1, y] = new Point { Type = PointType.Border, X = Settings.BoardWidth - 1, Y = y };

            //fill air blocks
            for (int y = 1; y < Settings.BoardHeight - 1; y++)
                for (int x = 1; x < Settings.BoardWidth - 1; x++)
                    this[x, y] = new Point { Type = PointType.Air, X = x, Y = y };
        }
    }
}
