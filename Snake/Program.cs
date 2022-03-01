using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace Snake
{
    class Program
    {
        static int updateInterval;
        static int score = 0;
        static int boardHeight;
        static int boardLength;
        static bool play = true;
        static bool playAgain = false;
        static bool snake2 = true;
        static Point[,] board;
        static Direction currentDirection = Direction.Right;
        static List<BodyPoint> bodyPoints = new List<BodyPoint>();

        static void Main(string[] args)
        {
            Console.SetWindowSize(40, 20);
            Console.SetBufferSize(40, 20);
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

        static void NewGame()
        {
            boardHeight = Console.WindowHeight - 2;
            boardLength = Console.WindowWidth - 1;

            board = new Point[boardHeight, boardLength];
            currentDirection = Direction.Right;
            playAgain = false;
            play = true;
            score = 0;
            updateInterval = 60;

            SetupBoard();
            GenerateFood();
        }

        static void SetupBoard()
        {
            Console.CursorVisible = false;
            //top border
            for (int i = 0; i <= boardHeight - 1; i++)
            {
                board[i, 0] = new Point { Type = PointType.Border };
            }

            //left boarder
            for (int i = 1; i <= boardLength - 1; i++)
            {
                board[0, i] = new Point { Type = PointType.Border };
            }

            //right border
            for (int i = 1; i <= boardLength - 1; i++)
            {
                board[boardHeight - 1, i] = new Point { Type = PointType.Border };
            }

            //bottom border
            for (int i = 0; i <= boardHeight - 1; i++)
            {
                board[i, boardLength - 1] = new Point { Type = PointType.Border };
            }

            //fill air blocks
            for (int i = 1; i < boardHeight - 1; i++)
            {
                for (int j = 1; j < boardLength - 1; j++)
                {
                    board[i, j] = new Point() { Type = PointType.Air };
                }
            }
            CreateSnake();
        }

        static void CreateSnake()
        {
            bodyPoints.Clear();
            bodyPoints.Add(new BodyPoint() { X = boardLength / 2 + 3, Y = boardHeight / 2 });
            bodyPoints.Add(new BodyPoint() { X = boardLength / 2 + 2, Y = boardHeight / 2 });
            bodyPoints.Add(new BodyPoint() { X = boardLength / 2 + 1, Y = boardHeight / 2 });
            bodyPoints.Add(new BodyPoint() { X = boardLength / 2, Y = boardHeight / 2 });
            bodyPoints.Add(new BodyPoint() { X = boardLength / 2 - 1, Y = boardHeight / 2 });

            for (int i = 0; i < bodyPoints.Count; i++)
            {
                board[bodyPoints[i].Y, bodyPoints[i].X] = new Point() { Type = PointType.Snake };
            }
        }

        static void Update()
        {
            SetDirection();
            Draw();
            GenerateFood();
            Move();
            IncreaseDifficulty();
        }

        static void IncreaseDifficulty()
        {
            if (score > 0 && score % 5 == 0)
            {
                updateInterval--;
                score = 0;
            }
        }

        static void GenerateFood()
        {
            if (BoardHasFood())
                return;
            Random r = new Random();
            int x = r.Next(1, boardLength - 1);
            int y = r.Next(1, boardHeight - 1);

            if (board[y, x]?.Type != PointType.Border | board[y, x]?.Type != PointType.Snake)
            {
                board[y, x] = new Point() { Type = PointType.Food };
            }
            else
            {
                GenerateFood();
            }
        }

        static bool BoardHasFood()
        {
            for (int i = 0; i < boardHeight; i++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    if (board[i, j]?.Type == PointType.Food)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool IsFoodCollected(int x, int y)
        {
            BodyPoint headPoint = GetHeadPoint();
            if (board[headPoint.Y + y, headPoint.X + x]?.Type == PointType.Food | board[headPoint.Y + y, headPoint.X + x]?.Type == PointType.Food)
            {
                return true;
            }
            return false;
        }

        static BodyPoint GetHeadPoint()
        {
            return bodyPoints[0];
        }

        static void Move()
        {
            BodyPoint headPoint = GetHeadPoint();
            int x = 0;
            int y = 0;
            int passX = 0; //if snake 2 and goes through the wall where should the snakes headX position be
            int passY = 0; //same for Y

            switch (currentDirection)
            {
                case Direction.Right:
                    x = 1;
                    y = 0;
                    passX = 1;
                    break;

                case Direction.Left:
                    x = -1;
                    y = 0;
                    passX = boardLength - 2;
                    break;

                case Direction.Up:
                    x = 0;
                    y = -1;
                    passY = boardHeight - 2;
                    break;

                case Direction.Down:
                    x = 0;
                    y = 1;
                    passY = 1;
                    break;
            }

            if (CollisionCheck(x, y))
            {
                play = false;
                return;
            }

            if (IsFoodCollected(x, y))
            {
                bodyPoints.Insert(0, new BodyPoint() { X = headPoint.X + x, Y = headPoint.Y + y });
                score++;
            }

            else //no food was collected and no collision
            {
                //turn tail point to air point for draw
                board[bodyPoints[bodyPoints.Count - 1].Y, bodyPoints[bodyPoints.Count - 1].X].Type = PointType.Air;
                //remove tail point from snake bodyparts
                bodyPoints.RemoveAt(bodyPoints.Count - 1);

                if (snake2)
                {
                    if (board[headPoint.Y + y, headPoint.X + x]?.Type == PointType.Border)
                    {
                        bodyPoints.Insert(0, new BodyPoint() { X = passX == 0 ? headPoint.X : passX, Y = passY == 0 ? headPoint.Y : passY });
                    }
                    else
                    {
                        bodyPoints.Insert(0, new BodyPoint() { X = headPoint.X + x, Y = headPoint.Y + y });
                    }
                }
                else
                {
                    bodyPoints.Insert(0, new BodyPoint() { X = headPoint.X + 1, Y = headPoint.Y });
                }
            }

            //add snake to the board with new coordinates
            foreach (var bodyPoint in bodyPoints)
            {
                board[bodyPoint.Y, bodyPoint.X] = new Point() { Type = PointType.Snake };
            }
        }

        static bool CollisionCheck(int x, int y)
        {
            BodyPoint headPoint = GetHeadPoint();
            if (!snake2)
            {
                if (board[headPoint.Y + y, headPoint.X + x]?.Type == PointType.Border | board[headPoint.Y + y, headPoint.X + x]?.Type == PointType.Snake)
                {
                    return true;
                }
            }
            else
            {
                if (board[headPoint.Y + y, headPoint.X + x]?.Type == PointType.Snake)
                {
                    return true;
                }
            }
            return false;
        }

        static void SetDirection(char key)
        {
            switch (key)
            {
                case 'w':
                    currentDirection = currentDirection == Direction.Down ? Direction.Down : Direction.Up;
                    break;

                case 'd':
                    currentDirection = currentDirection == Direction.Left ? Direction.Left : Direction.Right;
                    break;

                case 's':
                    currentDirection = currentDirection == Direction.Up ? Direction.Up : Direction.Down;
                    break;

                case 'a':
                    currentDirection = currentDirection == Direction.Right ? Direction.Right : Direction.Left;
                    break;
            }
        }

        static void SetDirection()
        {
            char key;
            if (Console.KeyAvailable)
            {
                key = Console.ReadKey(true).KeyChar;

                switch (key)
                {
                    case 'w':
                        currentDirection = currentDirection == Direction.Down ? Direction.Down : Direction.Up;
                        break;

                    case 'd':
                        currentDirection = currentDirection == Direction.Left ? Direction.Left : Direction.Right;
                        break;

                    case 's':
                        currentDirection = currentDirection == Direction.Up ? Direction.Up : Direction.Down;
                        break;

                    case 'a':
                        currentDirection = currentDirection == Direction.Right ? Direction.Right : Direction.Left;
                        break;
                }
            }
        }

        static void Draw()
        {
            Console.SetCursorPosition(0, 0);
            StringBuilder sb = new StringBuilder();

            sb.Append($"Score:{bodyPoints.Count - 5}\n");

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

                        default:
                            sb.Append(" ");
                            break;
                    }
                }
                sb.Append("\n");
            }
            Console.Write(sb);
        }

        static void PlayAgain()
        {
            int numberOfTabs = Console.WindowWidth / 8 / 2 - 1;
            for (int i = 0; i < numberOfTabs; i++)
            {
                Console.Write("\t");
            }
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
