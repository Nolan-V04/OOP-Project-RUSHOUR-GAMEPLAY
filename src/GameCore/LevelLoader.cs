using System.IO;
using System.Text.Json;
using RushHourGame.Models;
using RushHourGame.GameCore;

namespace RushHourGame.GameCore
{
    public class LevelLoader
    {
        public static LevelMap LoadLevel(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Không tìm thấy file: {path}");

            try
            {
                string json = File.ReadAllText(path);
                var level = JsonSerializer.Deserialize<LevelMap>(json);

                if (level == null || level.Size == null || level.Vehicles == null)
                    throw new InvalidDataException("Dữ liệu JSON không hợp lệ hoặc thiếu thuộc tính.");

                return level;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("Không thể phân tích nội dung JSON: " + ex.Message, ex);
            }
        }
        
    }
}
