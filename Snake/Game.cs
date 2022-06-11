using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Snake
{
    class Game
    {
        private Board _board;
        private Snake _snake;

        public void Run()
        {
            SetupWindow();
            do
            {
                NewGame();
                while (Settings.Play)
                {
                    Update();
                    Thread.Sleep(Settings.UpdateInterval);
                }
                PlayAgain();
            } while (Settings.PlayAgain);
        }

        private void SetupWindow()
        {
            Console.Title = "Snake";
            Console.CursorVisible = false;
            Console.SetWindowSize(Settings.WindowWidth, Settings.WindowHeight);
            Console.SetBufferSize(Settings.WindowWidth, Settings.WindowHeight);
        }

        private void NewGame()
        {
            Settings.PlayAgain = false;
            Settings.Play = true;
            Settings.DifficultyMultiplierCount = 0;
            Settings.UpdateInterval = 60;
            Settings.Score = 0;

            _snake = new Snake();
            _board = new Board(_snake);

            GenerateFood();
            Draw();
        }

        private void Update()
        {
            SetDirection();
            Move();
            IncreaseDifficulty();
            Draw();
        }

        private void IncreaseDifficulty()
        {
            if (Settings.DifficultyMultiplierCount > 0 && Settings.DifficultyMultiplierCount % Settings.DifficultyMultiplier == 0)
            {
                Settings.UpdateInterval--;
                Settings.DifficultyMultiplierCount = 0;
            }
        }

        private void Move()
        {
            var nextHeadPoint = _snake.NextHeadPoint();
            var nextBoardPoint = _board[nextHeadPoint.X, nextHeadPoint.Y];

            if (IsCollision(nextBoardPoint))
            {
                Settings.Play = false;
                return;
            }

            if (IsFoodCollected(nextBoardPoint))
            {
                Settings.DifficultyMultiplierCount++;
                Settings.Score++;
                GenerateFood();
            }

            else
            {
                _board[_snake.TailPoint.X, _snake.TailPoint.Y].Type = PointType.Air;
                _snake.RemoveTail();
            }

            _board[nextBoardPoint.X, nextBoardPoint.Y].Type = PointType.Snake;
            _snake.AddNewHead(nextBoardPoint);
        }

        private bool IsCollision(Point nextHeadPoint)
        {
            if (_board[_snake.TailPoint.X, _snake.TailPoint.Y] == nextHeadPoint)
                return false;

            return nextHeadPoint.Type == PointType.Border || nextHeadPoint.Type == PointType.Snake;
        }

        private bool IsFoodCollected(Point nextHeadPoint) => nextHeadPoint.Type == PointType.Food;

        private void SetDirection()
        {
            if (Console.KeyAvailable)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        _snake.CurrentDirection = _snake.CurrentDirection == Direction.Down ? Direction.Down : Direction.Up;
                        break;

                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        _snake.CurrentDirection = _snake.CurrentDirection == Direction.Left ? Direction.Left : Direction.Right;
                        break;

                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        _snake.CurrentDirection = _snake.CurrentDirection == Direction.Up ? Direction.Up : Direction.Down;
                        break;

                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        _snake.CurrentDirection = _snake.CurrentDirection == Direction.Right ? Direction.Right : Direction.Left;
                        break;
                }
            }
        }

        private void GenerateFood()
        {
            var airPoints = _board.points.Cast<Point>().Where(x => x.Type == PointType.Air).ToList();

            Random rnd = new Random();
            int randomIndex = rnd.Next(0, airPoints.Count() - 1);

            var foodPoint = airPoints[randomIndex];

            _board[foodPoint.X, foodPoint.Y].Type = PointType.Food;
        }

        private void Draw()
        {
            Console.SetCursorPosition(0, 0);
            StringBuilder sb = new StringBuilder();

            sb.Append($"Score:{Settings.Score}\n");

            for (int y = 0; y < Settings.BoardHeight; y++)
            {
                for (int x = 0; x < Settings.BoardWidth; x++)
                {
                    switch (_board[x, y]?.Type)
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

        private void PlayAgain()
        {
            for (int i = 0; i < Console.WindowWidth / 8 / 2 - 1; i++)
                Console.Write("\t");

            Console.Write("     Play Again? Y/N");
            string option;
            do
            {
                option = Console.ReadKey(true).KeyChar.ToString().ToLower();
                Settings.PlayAgain = option == "y" ? true : false;

            } while ((option != "y") && (option != "n"));
            Console.Clear();
        }
    }
}
