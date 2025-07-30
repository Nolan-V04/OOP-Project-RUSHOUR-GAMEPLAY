using System.Collections.Generic;

namespace RushHourGame.Models
{
    public class LevelMap
    {
        public int[] Size { get; set; } = new int[2];
        public List<Vehicle> Vehicles { get; set; } = new();
    }

}