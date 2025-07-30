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

        public void DrawGraphics(Graphics g, Size size)
        {
            int cellWidth = size.Width / Cols;
            int cellHeight = size.Height / Rows;

            g.Clear(Color.White);
            Pen gridPen = Pens.Gray;

            for (int i = 0; i <= Rows; i++)
                g.DrawLine(gridPen, 0, i * cellHeight, size.Width, i * cellHeight);

            for (int i = 0; i <= Cols; i++)
                g.DrawLine(gridPen, i * cellWidth, 0, i * cellWidth, size.Height);

            foreach (var car in Cars)
            {
                Rectangle rect = new(
                    car.Col * cellWidth,
                    car.Row * cellHeight,
                    (car.IsHorizontal ? car.Length : 1) * cellWidth,
                    (car.IsHorizontal ? 1 : car.Length) * cellHeight
                );
                g.FillRectangle(car.Name == "X" ? Brushes.Red : Brushes.Blue, rect);
                g.DrawRectangle(Pens.Black, rect);
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