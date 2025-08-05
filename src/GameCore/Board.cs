using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RushHourGame.GameCore
{
    public class Board
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public List<Car> Cars { get; set; } = new();

        public Board(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
        }

        public void AddCar(Car car) => Cars.Add(car);

        public Car? GetCarAt(int row, int col)
        {
            return Cars.FirstOrDefault(car =>
            {
                for (int i = 0; i < car.Length; i++)
                {
                    int r = car.Row + (car.IsHorizontal ? 0 : i);
                    int c = car.Col + (car.IsHorizontal ? i : 0);
                    if (r == row && c == col) return true;
                }
                return false;
            });
        }

        public bool TryMoveCar(Car car, int dx, int dy)
        {
            int move = car.IsHorizontal ? dx : dy;
            if (move == 0) return false;

            int newRow = car.Row + (car.IsHorizontal ? 0 : move);
            int newCol = car.Col + (car.IsHorizontal ? move : 0);

            for (int i = 0; i < car.Length; i++)
            {
                int r = newRow + (car.IsHorizontal ? 0 : i);
                int c = newCol + (car.IsHorizontal ? i : 0);
                if (r < 0 || r >= Rows || c < 0 || c >= Cols ||
                    Cars.Any(other => other != car && other.Occupies(r, c)))
                    return false;
            }

            car.Row = newRow;
            car.Col = newCol;
            return true;
        }

        public void DrawGraphics(Graphics g, Size panelSize)
        {
            int cellWidth = 100;
            int cellHeight = 100;

            // Vẽ lưới
            using Pen gridPen = new Pen(Color.LightGray);
            for (int i = 0; i <= Rows; i++)
                g.DrawLine(gridPen, 0, i * cellHeight, panelSize.Width, i * cellHeight);
            for (int j = 0; j <= Cols; j++)
                g.DrawLine(gridPen, j * cellWidth, 0, j * cellWidth, panelSize.Height);

            // Vẽ từng xe
            foreach (var car in Cars)
            {
                int x = car.Col * cellWidth;
                int y = car.Row * cellHeight;
                int w = car.IsHorizontal ? car.Length * cellWidth : cellWidth;
                int h = car.IsHorizontal ? cellHeight : car.Length * cellHeight;

                if (car.CarImage != null)
                {
                    g.DrawImage(car.CarImage, new Rectangle(x, y, w, h));
                }
                else
                {
                    // fallback nếu không có ảnh
                    Brush fill = car.Name == "X" ? Brushes.Red : Brushes.Gray;
                    g.FillRectangle(fill, x, y, w, h);
                    g.DrawRectangle(Pens.Black, x, y, w, h);
                    g.DrawString(car.Name, SystemFonts.DefaultFont, Brushes.White, x + 5, y + 5);
                }
            }
        }


        public List<Car> CloneCars() => Cars.Select(c => c.Clone()).ToList();

        public bool IsWin()
        {
            Car? main = Cars.FirstOrDefault(c => c.Name == "X");
            return main != null && main.Col + main.Length == Cols;
        }
    }
}