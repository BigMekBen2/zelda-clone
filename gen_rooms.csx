// dotnet script or just run with: dotnet-script gen_rooms.csx
// Actually: compile inline. We'll just write JSON directly via logic here.

using System.Text.Json;

const int COLS = 16, ROWS = 10;
const int FLOOR = 0, WALL = 1, DOOR_LOCKED = 2, DOOR_OPEN = 3, WATER = 4, SWITCH = 5, SWITCH_ON = 6, VOID = 7;

int[] MakeGrid(Action<int[], int, int, int> fn)
{
    var g = new int[COLS * ROWS];
    // border walls
    for (int c = 0; c < COLS; c++) { g[0 * COLS + c] = WALL; g[(ROWS-1) * COLS + c] = WALL; }
    for (int r = 0; r < ROWS; r++) { g[r * COLS + 0] = WALL; g[r * COLS + COLS-1] = WALL; }
    fn(g, COLS, ROWS, WALL);
    return g;
}

void Set(int[] g, int c, int r, int t) => g[r * COLS + c] = t;
int Idx(int c, int r) => r * COLS + c;

// Room 0: entrance, south door to room 1, 2 blobs
var r0 = MakeGrid((g, C, R, W) => {
    // south door gap (center)
    Set(g, 7, R-1, FLOOR); Set(g, 8, R-1, FLOOR);
});
var room0 = new {
    Grid = r0,
    Spawns = new[] {
        new { Type = 0, Col = 4, Row = 4 },
        new { Type = 0, Col = 11, Row = 6 },
    },
    Doors = new[] { -1, 1, -1, -1 },
    SwitchDoors = new Dictionary<string,int>(),
};

// Room 1: connects N->room0, E->room2, has a switch that opens east door
var r1 = MakeGrid((g, C, R, W) => {
    Set(g, 7, 0, FLOOR); Set(g, 8, 0, FLOOR); // north door
    Set(g, C-1, 4, FLOOR); Set(g, C-1, 5, FLOOR); // east door (locked until switch)
    // inner wall with switch
    for (int r = 2; r < R-2; r++) Set(g, 10, r, W);
    Set(g, 10, 5, SWITCH); // switch in wall
    // some water
    for (int c = 3; c <= 6; c++) Set(g, c, 5, WATER);
    // east door is locked tile
    Set(g, C-1, 4, DOOR_LOCKED); Set(g, C-1, 5, DOOR_LOCKED);
});
var room1 = new {
    Grid = r1,
    Spawns = new[] {
        new { Type = 0, Col = 3, Row = 3 },
        new { Type = 2, Col = 12, Row = 7 }, // bat on east side
    },
    Doors = new[] { 0, -1, 2, -1 },
    SwitchDoors = new Dictionary<string,int> { { Idx(10,5).ToString(), 2 } },
};

// Room 2: connects W->room1, S->room3, skeleton gauntlet
var r2 = MakeGrid((g, C, R, W) => {
    Set(g, 0, 4, FLOOR); Set(g, 0, 5, FLOOR); // west door
    Set(g, 7, R-1, FLOOR); Set(g, 8, R-1, FLOOR); // south door
    // pillars
    Set(g, 4, 3, W); Set(g, 4, 4, W);
    Set(g, 11, 3, W); Set(g, 11, 4, W);
});
var room2 = new {
    Grid = r2,
    Spawns = new[] {
        new { Type = 2, Col = 7, Row = 2 },
        new { Type = 2, Col = 7, Row = 7 },
        new { Type = 1, Col = 3, Row = 7 },  // blob
    },
    Doors = new[] { -1, 3, -1, 1 },
    SwitchDoors = new Dictionary<string,int>(),
};

// Room 3: connects N->room2, E->room4 (locked door, key dropped by skeleton)
var r3 = MakeGrid((g, C, R, W) => {
    Set(g, 7, 0, FLOOR); Set(g, 8, 0, FLOOR); // north
    Set(g, C-1, 4, DOOR_LOCKED); Set(g, C-1, 5, DOOR_LOCKED); // east locked
    // corridor feel
    for (int c = 2; c <= 13; c++) Set(g, c, 2, W);
    for (int c = 2; c <= 13; c++) Set(g, c, 7, W);
    Set(g, 7, 2, FLOOR); Set(g, 8, 2, FLOOR);
    Set(g, 7, 7, FLOOR); Set(g, 8, 7, FLOOR);
});
var room3 = new {
    Grid = r3,
    Spawns = new[] {
        new { Type = 2, Col = 5, Row = 5 }, // skeleton drops key (handled by game later)
        new { Type = 2, Col = 10, Row = 5 },
        new { Type = 1, Col = 13, Row = 5 }, // blob guards east
    },
    Doors = new[] { 2, -1, 4, -1 },
    SwitchDoors = new Dictionary<string,int>(),
};

// Room 4: boss room — connects W->room3
var r4 = MakeGrid((g, C, R, W) => {
    Set(g, 0, 4, FLOOR); Set(g, 0, 5, FLOOR); // west door
    // open arena
});
var room4 = new {
    Grid = r4,
    Spawns = new[] {
        new { Type = 2, Col = 8, Row = 2 },  // skeleton boss placeholder (more hp via code later)
        new { Type = 2, Col = 8, Row = 7 },
        new { Type = 2, Col = 12, Row = 5 },
        new { Type = 1, Col = 5, Row = 5 },  // bat
        new { Type = 1, Col = 6, Row = 3 },
    },
    Doors = new[] { -1, -1, -1, 3 },
    SwitchDoors = new Dictionary<string,int>(),
};

var opts = new JsonSerializerOptions { WriteIndented = true };
var dir = Path.Combine(args.Length > 0 ? args[0] : "rooms");
Directory.CreateDirectory(dir);
File.WriteAllText(Path.Combine(dir, "room_0.json"), JsonSerializer.Serialize(room0, opts));
File.WriteAllText(Path.Combine(dir, "room_1.json"), JsonSerializer.Serialize(room1, opts));
File.WriteAllText(Path.Combine(dir, "room_2.json"), JsonSerializer.Serialize(room2, opts));
File.WriteAllText(Path.Combine(dir, "room_3.json"), JsonSerializer.Serialize(room3, opts));
File.WriteAllText(Path.Combine(dir, "room_4.json"), JsonSerializer.Serialize(room4, opts));
Console.WriteLine("Rooms generated.");
