public class Vehicle
{
    public string Name { get; set; } = "";
    public int Row { get; set; }
    public int Col { get; set; }
    public int Length { get; set; }
    public string Orientation { get; set; } = "";

    public bool IsHorizontal => Orientation == "H";
    public bool IsMain => Name == "X";
}
