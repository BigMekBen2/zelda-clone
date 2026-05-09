using Raylib_cs;
using System.Numerics;

namespace ZeldaClone;

enum TileType : byte
{
    Floor = 0,
    Wall = 1,
    DoorLocked = 2,
    DoorOpen = 3,
    Water = 4,
    Switch = 5,
    SwitchOn = 6,
    Void = 7,
}

static class Tiles
{
    public const int Size = 16;
    public const int Cols = 16;
    public const int Rows = 10;

    static readonly Color[] Palette = [
        new Color(60, 48, 32, 255),   // floor
        new Color(20, 16, 8, 255),    // wall
        new Color(180, 60, 20, 255),  // door locked
        new Color(80, 180, 60, 255),  // door open
        new Color(20, 60, 180, 255),  // water
        new Color(220, 200, 40, 255), // switch off
        new Color(255, 240, 120, 255),// switch on
        new Color(0, 0, 0, 255),      // void
    ];

    public static bool IsSolid(TileType t) => t switch
    {
        TileType.Wall => true,
        TileType.DoorLocked => true,
        TileType.Water => true,
        TileType.Void => true,
        _ => false,
    };

    public static bool IsPassable(TileType t) => !IsSolid(t);

    public static void Draw(TileType t, int col, int row)
    {
        var c = Palette[(int)t];
        int x = col * Size;
        int y = row * Size;
        Raylib.DrawRectangle(x, y, Size, Size, c);

        // grid lines for wall contrast
        if (t == TileType.Floor || t == TileType.Switch || t == TileType.SwitchOn)
            Raylib.DrawRectangleLines(x, y, Size, Size, new Color(40, 32, 20, 80));
    }
}
