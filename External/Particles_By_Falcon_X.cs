using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FXX;

namespace FXX
{
    public class Particles
    {
        #region variables
        public static Form _Instance;
        public static (int, int)[] AvailableSpeeds;
        public static int _CircleNum;
        public static int _ConnectLines;
        public static Random rand = new Random();
        public static List<Circles> _Particles = new List<Circles>();
        public static List<Lines> _Lines = new List<Lines>();
        private static Bitmap _bufferBitmap;
        public static Color _LineColor;
        #endregion
        public static void Setup(Form Instance, int CircleNum, Color CircleColor, int ConnectLines, Color LineColor) //start
        {
            _Instance = Instance;
            _CircleNum = CircleNum;
            _ConnectLines = ConnectLines;
            Instance.Paint += DrawCirclesAndLines;
            _LineColor = LineColor;
            _bufferBitmap = new Bitmap(_Instance.Width, _Instance.Height);
            List<(int, int)> speedsList = new List<(int, int)>();

            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (j != 0 || k != 0)
                    {
                        speedsList.Add((j, k));
                    }
                }
            }

            Shuffle(speedsList);

            while (speedsList.Count < CircleNum * 2)
            {
                speedsList.AddRange(speedsList);
                Shuffle(speedsList);
            }

            AvailableSpeeds = speedsList.Take(CircleNum * 2).ToArray();

            for (int i = 0; i < CircleNum; i++)
            {
                var speedIndex = i % AvailableSpeeds.Length;
                var (speedX, speedY) = AvailableSpeeds[speedIndex];
                _Particles.Add(new Circles()
                {
                    Size = rand.Next(2, 2),
                    Pos = new Point(rand.Next(Instance.Width), rand.Next(Instance.Height)),
                    Speed = new Point(speedX, speedY),
                    color = CircleColor,
                    Opacity = 255,
                });
            }

            for (int i = 0; i < _Particles.Count; i++)
            {
                for (int j = i + 1; j < _Particles.Count; j++)
                {
                    int x1 = _Particles[i].Pos.X;
                    int y1 = _Particles[i].Pos.Y;
                    int x2 = _Particles[j].Pos.X;
                    int y2 = _Particles[j].Pos.Y;

                    Lines newLine = new Lines
                    {
                        Size = 2,
                        Pos = new Point(x1, y1),
                        Pos2 = new Point(x2, y2),
                        Color = LineColor,
                        Opacity = 50
                    };
                    _Lines.Add(newLine);
                }
            }

        }

        public static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void MoveCircles(List<Circles> circles)
        {
            Random random = new Random();

            for (int i = 0; i < circles.Count; i++)
            {
                Circles circle = circles[i];

                int newX = circle.Pos.X + circle.Speed.X;
                int newY = circle.Pos.Y + circle.Speed.Y;

                if (newX - circle.Size < 0 || newX + circle.Size > _Instance.ClientSize.Width)
                {
                    circle.Speed = new Point(-circle.Speed.X, circle.Speed.Y);
                }

                if (newY - circle.Size < 0 || newY + circle.Size > _Instance.ClientSize.Height)
                {
                    circle.Speed = new Point(circle.Speed.X, -circle.Speed.Y);
                }

                circle.Pos = new Point(newX, newY);
                circles[i] = circle;
            }

        }



        public static void DrawCirclesAndLines(object sender, PaintEventArgs e)
        {
            if (_Instance.WindowState != FormWindowState.Minimized)
            {

                using (Graphics bufferGraphics = Graphics.FromImage(_bufferBitmap))
                {
                    bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    bufferGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    bufferGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    bufferGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    bufferGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    bufferGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    bufferGraphics.Clear(_Instance.BackColor);
                    DrawCircles(bufferGraphics);
                    DrawLines(bufferGraphics);
                }

                e.Graphics.DrawImage(_bufferBitmap, Point.Empty);

            }
        }
        public static void DrawCircles(Graphics g)
        {
            foreach (var circle in _Particles)
            {
                g.FillEllipse(new SolidBrush(Color.FromArgb(circle.Opacity, circle.color)),
                              circle.Pos.X - circle.Size, circle.Pos.Y - circle.Size,
                              circle.Size * 2, circle.Size * 2);
            }
        }
        public static void DrawLines(Graphics g)
        {
            _Lines.Clear();
            for (int i = 0; i < _Particles.Count; i++)
            {
                for (int j = i + 1; j < _Particles.Count; j++)
                {
                    int x1 = _Particles[i].Pos.X;
                    int y1 = _Particles[i].Pos.Y;

                    int x2 = _Particles[j].Pos.X;
                    int y2 = _Particles[j].Pos.Y;

                    Lines newLine = new Lines
                    {
                        Size = 2,
                        Pos = new Point(x1, y1),
                        Pos2 = new Point(x2, y2),
                        Color = _LineColor,
                        Opacity = 25
                    };
                    _Lines.Add(newLine);
                }
            }
            foreach (var line in _Lines)
            {
                double distance = CalculateDistance(line.Pos, line.Pos2);

                if (distance <= 45.0f)
                {
                    using (Pen pen = new Pen(Color.FromArgb(line.Opacity, line.Color), line.Size))
                    {
                        g.DrawLine(pen, line.Pos, line.Pos2);
                    }
                }
            }
        }
        public static double CalculateDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

    }
    public class Circles
    {
        public int Size { get; set; }
        public Point Pos { get; set; }
        public Point Speed { get; set; }
        public Color color { get; set; }
        public int Opacity { get; set; }
    }
    public class Lines
    {
        public int Size { get; set; }
        public Point Pos { get; set; }
        public Point Pos2 { get; set; }
        public Color Color { get; set; }
        public int Opacity { get; set; }
    }
}
