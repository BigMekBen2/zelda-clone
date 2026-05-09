using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZeldaClone;

class Room
{
    public TileType[] Grid = new TileType[Tiles.Cols * Tiles.Rows];
    public List<EnemySpawn> Spawns = [];
    // door connections: N, S, E, W -> room index or -1
    public int[] Doors = [-1, -1, -1, -1];
    // switch->door mappings: switch tile index -> door direction index (0=N,1=S,2=E,3=W)
    public Dictionary<int, int> SwitchDoors = [];

    public TileType Get(int col, int row) => Grid[row * Tiles.Cols + col];
    public void Set(int col, int row, TileType t) => Grid[row * Tiles.Cols + col] = t;

    public void Draw()
    {
        for (int r = 0; r < Tiles.Rows; r++)
            for (int c = 0; c < Tiles.Cols; c++)
                Tiles.Draw(Get(c, r), c, r);
    }

    public static Room Empty()
    {
        var room = new Room();
        // border walls
        for (int c = 0; c < Tiles.Cols; c++)
        {
            room.Set(c, 0, TileType.Wall);
            room.Set(c, Tiles.Rows - 1, TileType.Wall);
        }
        for (int r = 0; r < Tiles.Rows; r++)
        {
            room.Set(0, r, TileType.Wall);
            room.Set(Tiles.Cols - 1, r, TileType.Wall);
        }
        return room;
    }

    public void Save(string path)
    {
        var dto = new RoomDto
        {
            Grid = Grid.Select(t => (int)t).ToArray(),
            Spawns = Spawns,
            Doors = Doors,
            SwitchDoors = SwitchDoors.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
        };
        File.WriteAllText(path, JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static Room Load(string path)
    {
        var dto = JsonSerializer.Deserialize<RoomDto>(File.ReadAllText(path))!;
        var room = new Room
        {
            Grid = dto.Grid.Select(i => (TileType)i).ToArray(),
            Spawns = dto.Spawns,
            Doors = dto.Doors,
            SwitchDoors = dto.SwitchDoors.ToDictionary(kv => int.Parse(kv.Key), kv => kv.Value),
        };
        return room;
    }
}

class RoomDto
{
    public int[] Grid { get; set; } = [];
    public List<EnemySpawn> Spawns { get; set; } = [];
    public int[] Doors { get; set; } = [-1, -1, -1, -1];
    public Dictionary<string, int> SwitchDoors { get; set; } = [];
}

class EnemySpawn
{
    public EnemyType Type { get; set; }
    public int Col { get; set; }
    public int Row { get; set; }
}
