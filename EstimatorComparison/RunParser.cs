using System.Text.Json;

namespace EstimatorComparison;

public record struct Floor(bool IsCombat, bool IsElite);

public static class RunParser
{
    public static List<Floor> Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException();
        
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(filePath));
        JsonElement root = doc.RootElement;
        JsonElement mapPointHistory = root.GetProperty("map_point_history");

        List<Floor> floorData = new List<Floor>();
        foreach (JsonElement act in mapPointHistory.EnumerateArray())
        {
            foreach (JsonElement floor in act.EnumerateArray())
            {
                string? roomType = floor
                    .GetProperty("rooms")[0]
                    .GetProperty("room_type")
                    .GetString();
                
                bool isCombat = roomType switch
                {
                    "monster" => true,
                    "elite" => true,
                    "boss" => true,
                    _ => false
                };
                
                bool isElite = roomType == "elite";
                
                floorData.Add(new Floor(isCombat, isElite));
            }
        }

        return floorData;
    }
}
