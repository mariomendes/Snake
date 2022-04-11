using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace Snake
{
    class Program
    {
        static int updateInterval;
        static int boardHeight;
        static int boardLength;
        static bool play = true;
        static bool playAgain = false;
        static bool wallTravel = true;
        static Point[,] board;
        static Direction currentDirection = Direction.Right;
        static List<Point> snakebodyPoints = new List<Point>();
        static int width = 40;
        static int height = 20;
        static int snakeStartLength = 5;
        static int difficultyMultiplierCount = 0;
        static int difficultyMultiplier = 5;
        static List<char> keysPressed = new List<char>();

        static void Main(string[] args)
        {
            SetupWindow();
            do
            {
                NewGame();
                while (play)
                {
                    Update();
                    Thread.Sleep(updateInterval);
                }
                PlayAgain();
            } while (playAgain);
        }

        static void SetupWindow()
        {
            Console.CursorVisible = false;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
        }

        static void NewGame()
        {
            boardHeight = Console.WindowHeight - 2;
            boardLength = Console.WindowWidth - 1;

            board = new Point[boardHeight, boardLength];
            currentDirection = Direction.Right;
            playAgain = false;
            play = true;
            difficultyMultiplierCount = 0;
            updateInterval = 60;

            SetupBoard();
            CreateSnake();
            GenerateFood();
            Draw();
        }

        static void SetupBoard()
        {
            //top border
            for (int i = 0; i <= boardHeight - 1; i++)
                board[i, 0] = new Point { Type = PointType.Border, X = 0, Y = i };

            //left border
            for (int i = 1; i <= boardLength - 1; i++)
                board[0, i] = new Point { Type = PointType.Border, X = i, Y = 0 };

            //right border
            for (int i = 1; i <= boardLength - 1; i++)
                board[boardHeight - 1, i] = new Point { Type = PointType.Border, X = i, Y = boardHeight - 1 };

            //bottom border
            for (int i = 0; i <= boardHeight - 1; i++)
                board[i, boardLength - 1] = new Point { Type = PointType.Border, X = boardLength - 1, Y = i };

            //fill air blocks
            for (int i = 1; i < boardHeight - 1; i++)
                for (int j = 1; j < boardLength - 1; j++)
                    board[i, j] = new Point() { Type = PointType.Air, X = j, Y = i };
        }

        static void CreateSnake()
        {
            snakebodyPoints.Clear();

            for (int i = 3; i > 3 - snakeStartLength; i--)
                snakebodyPoints.Add(new Point() { X = boardLength / 2 + i, Y = boardHeight / 2, Type = PointType.Snake });

            foreach (var bodyPoint in snakebodyPoints)
                GetBoardPoint(bodyPoint.X, bodyPoint.Y).Type = PointType.Snake;
        }

        static void Update()
        {
            SetDirection();
            Move();
            IncreaseDifficulty();
            Draw();
        }

        static void IncreaseDifficulty()
        {
            if (difficultyMultiplierCount > 0 && difficultyMultiplierCount % difficultyMultiplier == 0)
            {
                updateInterval--;
                difficultyMultiplierCount = 0;
            }
        }

        static void GenerateFood()
        {
            var airPoints = board.Cast<Point>().Where(x => x.Type == PointType.Air).ToList();

            Random rnd = new Random();
            int randomIndex = rnd.Next(0, airPoints.Count() - 1);

            var foodPoint = airPoints[randomIndex];

            GetBoardPoint(foodPoint.X, foodPoint.Y).Type = PointType.Food;
        }

        static bool IsFoodCollected(Point point)
        {
            return point.Type == PointType.Food ? true : false;
        }

        static Point GetHeadPoint()
        {
            return GetBoardPoint(snakebodyPoints.First().X, snakebodyPoints.First().Y);
        }

        static void Move()
        {
            var nextHeadPoint = NextHeadPoint();

            if (IsCollision(nextHeadPoint))
            {
                play = false;
                return;
            }

            if (IsFoodCollected(nextHeadPoint))
            {
                difficultyMultiplierCount++;
                GenerateFood();
            }

            else
            {
                RemoveTail();
            }

            snakebodyPoints.Insert(0, new Point() { X = nextHeadPoint.X, Y = nextHeadPoint.Y });
            GetBoardPoint(nextHeadPoint.X, nextHeadPoint.Y).Type = PointType.Snake;
        }

        static void RemoveTail()
        {
            GetTailPoint().Type = PointType.Air;
            snakebodyPoints.RemoveAt(snakebodyPoints.Count - 1);
        }

        static Point GetBoardPoint(int x, int y)
        {
            return board[y, x];
        }

        static Point GetTailPoint()
        {
            return GetBoardPoint(snakebodyPoints.Last().X, snakebodyPoints.Last().Y);
        }

        static Point NextHeadPoint()
        {
            int x = 0;
            int y = 0;
            int wallPassX = 0; //if goes through the wall where should the snakes head X position be
            int wallPassY = 0; //same for Y

            switch (currentDirection)
            {
                case Direction.Right:
                    x = 1;
                    y = 0;
                    wallPassX = 1;
                    break;

                case Direction.Left:
                    x = -1;
                    y = 0;
                    wallPassX = boardLength - 2;
                    break;

                case Direction.Up:
                    x = 0;
                    y = -1;
                    wallPassY = boardHeight - 2;
                    break;

                case Direction.Down:
                    x = 0;
                    y = 1;
                    wallPassY = 1;
                    break;
            }

            var currentHeadPoint = GetHeadPoint();

            if (wallTravel && GetBoardPoint(currentHeadPoint.X + x, currentHeadPoint.Y + y).Type == PointType.Border)
            {
                var xPoint = wallPassX == 0 ? currentHeadPoint.X : wallPassX;
                var yPoint = wallPassY == 0 ? currentHeadPoint.Y : wallPassY;
                return GetBoardPoint(xPoint, yPoint);
            }

            return GetBoardPoint(currentHeadPoint.X + x, currentHeadPoint.Y + y);
        }

        static bool IsCollision(Point point)
        {
            var tailPoint = GetTailPoint();

            if (point == tailPoint)
                return false;

            if (point.Type == PointType.Snake || point.Type == PointType.Border)
                return true;

            return false;
        }

        static void SetDirection()
        {
            if (Console.KeyAvailable)
            {
                char c = Console.ReadKey(true).KeyChar;
                if(keysPressed.Count > 0 && c == keysPressed.Last())
                    return;
                switch (c)
                {
                    case 'w':
                        currentDirection = currentDirection == Direction.Down ? Direction.Down : Direction.Up;
                        keysPressed.Add('w');
                        break;

                    case 'd':
                        currentDirection = currentDirection == Direction.Left ? Direction.Left : Direction.Right;
                        keysPressed.Add('d');
                        break;

                    case 's':
                        currentDirection = currentDirection == Direction.Up ? Direction.Up : Direction.Down;
                        keysPressed.Add('s');
                        break;

                    case 'a':
                        currentDirection = currentDirection == Direction.Right ? Direction.Right : Direction.Left;
                        keysPressed.Add('a');
                        break;
                }
            }
        }

        static void Draw()
        {
            Console.SetCursorPosition(0, 0);
            StringBuilder sb = new StringBuilder();

            sb.Append($"Score:{snakebodyPoints.Count - snakeStartLength}\n");

            for (int i = 0; i < boardHeight; i++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    switch (board[i, j]?.Type)
                    {
                        case PointType.Border:
                            sb.Append("0");
                            break;

                        case PointType.Air:
                            sb.Append(" ");
                            break;

                        case PointType.Snake:
                            sb.Append("%");
                            break;

                        case PointType.Food:
                            sb.Append("*");
                            break;
                    }
                }
                sb.Append("\n");
            }
            Console.Write(sb);
        }

        static void PlayAgain()
        {
            for (int i = 0; i < Console.WindowWidth / 8 / 2 - 1; i++)
                Console.Write("\t");

            Console.Write("      Play Again? y/n");
            char option;
            do
            {
                option = Console.ReadKey(true).KeyChar;
                playAgain = option == 'y' ? true : false;

            } while ((option != 'y') && (option != 'n'));
            Console.Clear();
        }
    }
}