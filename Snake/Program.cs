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
        static bool playAgain = true;
        static bool snake2 = true;
        static Point[,] board;
        static Direction currentDirection = Direction.Right;
        static List<BodyPoint> bodyPoints = new List<BodyPoint>();
        static BackgroundWorker bw = new BackgroundWorker();

        static void Main(string[] args)
        {
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerAsync();
            while (playAgain)
            {
                NewGame();
                while (play)
                {
                    Update();
                    Thread.Sleep(updateInterval);
                }
                PlayAgain();
            }
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
        }

        private static void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    SetDirection(Console.ReadKey(true).KeyChar);
                }
            }
        }

        static void SetupBoard()
        {
            Console.CursorVisible = false;
            //top border
            for (int i = 0; i <= boardHeight - 1; i++)
            {
                board[i, 0] = new Point { Type = 0 };
            }

            //left boarder
            for (int i = 1; i <= boardLength - 1; i++)
            {
                board[0, i] = new Point { Type = 0 };
            }

            //right boarder
            for (int i = 1; i <= boardLength - 1; i++)
            {
                board[boardHeight - 1, i] = new Point { Type = 0 };
            }

            //bottom border
            for (int i = 0; i <= boardHeight - 1; i++)
            {
                board[i, boardLength - 1] = new Point { Type = 0 };
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
            bool flag = false;
            for (int i = 0; i < boardHeight; i++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    if (board[i, j]?.Type == PointType.Food)
                    {
                        flag = true;
                        continue;
                    }
                }
            }
            return flag;
        }

        static bool IsFoodCollected(int x, int y)
        {
            if (board[bodyPoints[0].Y + y, bodyPoints[0].X + x]?.Type == PointType.Food | board[bodyPoints[0].Y + y, bodyPoints[0].X + x]?.Type == PointType.Food)
            {
                return true;
            }
            return false;
        }

        static void Move()
        {
            switch (currentDirection)
            {
                case Direction.Right:
                    if (CollisionCheck(1, 0))
                    {
                        play = false;
                        break;
                    }

                    if (IsFoodCollected(1, 0))
                    {
                        bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X + 1, Y = bodyPoints[0].Y });
                        score++;
                    }
                    else
                    {
                        if (snake2)
                        {
                            if(board[bodyPoints[0].Y, bodyPoints[0].X + 1]?.Type == PointType.Border)
                            {
                                bodyPoints.Insert(0, new BodyPoint() { X = 1, Y = bodyPoints[0].Y });
                            }
                            else
                            {
                                bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X + 1, Y = bodyPoints[0].Y });
                            }
                        }
                        else
                        {
                            bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X + 1, Y = bodyPoints[0].Y });
                        }
                        board[bodyPoints[bodyPoints.Count - 1].Y, bodyPoints[bodyPoints.Count - 1].X].Type = PointType.Air;
                        bodyPoints.RemoveAt(bodyPoints.Count - 1);
                    }

                    break;

                case Direction.Up:
                    if (CollisionCheck(0, -1))
                    {
                        play = false;
                        break;
                    }

                    if (IsFoodCollected(0, -1))
                    {
                        bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X, Y = bodyPoints[0].Y - 1 });
                        score++;
                    }
                    else
                    {
                        if(snake2)
                        {
                            if (board[bodyPoints[0].Y - 1, bodyPoints[0].X]?.Type == PointType.Border)
                            {
                                bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X, Y = boardHeight - 2 });
                            }
                            else
                            {
                                bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X, Y = bodyPoints[0].Y - 1 });
                            }
                        }
                        else
                        {
                            bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X, Y = bodyPoints[0].Y - 1 });
                        }
                        board[bodyPoints[bodyPoints.Count - 1].Y, bodyPoints[bodyPoints.Count - 1].X].Type = PointType.Air;
                        bodyPoints.RemoveAt(bodyPoints.Count - 1);
                    }
                    break;

                case Direction.Down:
                    if (CollisionCheck(0, 1))
                    {
                        play = false;
                        break;
                    }

                    if (IsFoodCollected(0, 1))
                    {
                        bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X, Y = bodyPoints[0].Y + 1 });
                        score++;
                    }
                    else
                    {
                        if (snake2)
                        {
                            if (board[bodyPoints[0].Y + 1, bodyPoints[0].X]?.Type == PointType.Border)
                            {
                                bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X, Y = 1 });
                            }
                            else
                            {
                                bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X, Y = bodyPoints[0].Y + 1 });
                            }
                        }
                        else
                        {
                            bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X, Y = bodyPoints[0].Y + 1 });
                        }
                        board[bodyPoints[bodyPoints.Count - 1].Y, bodyPoints[bodyPoints.Count - 1].X].Type = PointType.Air;
                        bodyPoints.RemoveAt(bodyPoints.Count - 1);
                    }

                    break;

                case Direction.Left:
                    if (CollisionCheck(-1, 0))
                    {
                        play = false;
                        break;
                    }

                    if (IsFoodCollected(-1, 0))
                    {
                        bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X - 1, Y = bodyPoints[0].Y });
                        score++;
                        break;
                    }
                    else
                    {
                        if(snake2)
                        {
                            if (board[bodyPoints[0].Y, bodyPoints[0].X - 1]?.Type == PointType.Border)
                            {
                                bodyPoints.Insert(0, new BodyPoint() { X = boardLength - 2, Y = bodyPoints[0].Y });
                            }
                            else
                            {
                                bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X - 1, Y = bodyPoints[0].Y });
                            }
                            board[bodyPoints[bodyPoints.Count - 1].Y, bodyPoints[bodyPoints.Count - 1].X].Type = PointType.Air;
                            bodyPoints.RemoveAt(bodyPoints.Count - 1);
                        }
                        else
                        {
                            bodyPoints.Insert(0, new BodyPoint() { X = bodyPoints[0].X - 1, Y = bodyPoints[0].Y });
                            board[bodyPoints[bodyPoints.Count - 1].Y, bodyPoints[bodyPoints.Count - 1].X].Type = PointType.Air;
                            bodyPoints.RemoveAt(bodyPoints.Count - 1);
                        }
                    }

                    break;
            }
            //add snake to the board with new coordinates
            for (int i = 0; i < bodyPoints.Count; i++)
            {
                board[bodyPoints[i].Y, bodyPoints[i].X] = new Point() { Type = PointType.Snake };
            }
        }

        static bool CollisionCheck(int x, int y)
        {
            if (!snake2)
            {
                if (board[bodyPoints[0].Y + y, bodyPoints[0].X + x]?.Type == PointType.Border | board[bodyPoints[0].Y + y, bodyPoints[0].X + x]?.Type == PointType.Snake)
                {
                    return true;
                }
            }
            else
            {
                if (board[bodyPoints[0].Y + y, bodyPoints[0].X + x]?.Type == PointType.Snake)
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
