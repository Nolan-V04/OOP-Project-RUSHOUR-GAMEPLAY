using System;
using System.Drawing;

namespace RushHourGame.GameCore
{
    public class Car
    {
        public string Name { get; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int Length { get; }
        public bool IsHorizontal { get; }
        public Image? CarImage { get; }

        public Car(string name, int row, int col, int length, string orientation)
        {
            Name = name;
            Row = row;
            Col = col;
            Length = length;
            IsHorizontal = orientation == "H";

            CarImage = LoadCarImage(Name, IsHorizontal);
        }

        /// Kiểm tra xem xe có chiếm ô (r, c) không
        public bool Occupies(int r, int c)
        {
            for (int i = 0; i < Length; i++)
            {
                int checkRow = Row + (IsHorizontal ? 0 : i);
                int checkCol = Col + (IsHorizontal ? i : 0);
                if (checkRow == r && checkCol == c)
                    return true;
            }
            return false;
        }

        /// Tạo bản sao xe (phục vụ undo)
        public Car Clone()
        {
            return new Car(Name, Row, Col, Length, IsHorizontal ? "H" : "V");
        }

        /// Tải ảnh xe dựa vào tên và hướng
        private Image? LoadCarImage(string name, bool isHorizontal)
        {
            string direction = isHorizontal ? "H" : "V";
            string path = $"img/Car/{name}{direction}.png";

            try
            {
                return Image.FromFile(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
