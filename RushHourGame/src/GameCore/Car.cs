namespace RushHourGame.GameCore
{
    public class Car
    {
        public string Name;
        public int Row;
        public int Col;
        public int Length;
        public bool IsHorizontal;

        public Car(string name, int row, int col, int length, string orientation)
        {
            Name = name;
            Row = row;
            Col = col;
            Length = length;
            IsHorizontal = orientation == "H";
        }

        public bool Occupies(int r, int c)
        {
            for (int i = 0; i < Length; i++)
            {
                int checkRow = Row + (IsHorizontal ? 0 : i);
                int checkCol = Col + (IsHorizontal ? i : 0);
                if (checkRow == r && checkCol == c) return true;
            }
            return false;
        }

        public Car Clone()
        {
            return new Car(Name, Row, Col, Length, IsHorizontal ? "H" : "V");
        }
    }
}