using System.Numerics;

namespace ZeldaClone;

enum Dir { N, S, E, W }

static class DirExt
{
    public static Vector2 ToVec(this Dir d) => d switch
    {
        Dir.N => new Vector2(0, -1),
        Dir.S => new Vector2(0, 1),
        Dir.E => new Vector2(1, 0),
        Dir.W => new Vector2(-1, 0),
        _ => Vector2.Zero,
    };
}

class Arrow
{
    public Vector2 Pos;
    public Vector2 Vel;
    public bool FromPlayer;
    public bool Dead;
    const float Speed = 180f;

    public Arrow(Vector2 pos, Dir dir, bool fromPlayer)
    {
        Pos = pos;
        Vel = dir.ToVec() * Speed;
        FromPlayer = fromPlayer;
    }

    public void Update(float dt, Room room)
    {
        if (Dead) return;
        Pos += Vel * dt;
        int col = (int)(Pos.X / Tiles.Size);
        int row = (int)(Pos.Y / Tiles.Size);
        if (col < 0 || col >= Tiles.Cols || row < 0 || row >= Tiles.Rows)
        { Dead = true; return; }
        var tile = room.Get(col, row);
        if (Tiles.IsSolid(tile)) { Dead = true; return; }
        if (tile == TileType.Switch)
        {
            room.Set(col, row, TileType.SwitchOn);
            int idx = row * Tiles.Cols + col;
            if (room.SwitchDoors.TryGetValue(idx, out int doorDir))
                room.Doors[doorDir] = -2; // -2 = switch-opened (passable)
            Dead = true;
        }
    }

    public void Draw()
    {
        if (Dead) return;
        Raylib_cs.Raylib.DrawRectangle((int)Pos.X - 2, (int)Pos.Y - 2, 4, 4,
            new Raylib_cs.Color(220, 200, 80, 255));
    }
}

class SwordHit
{
    public System.Drawing.Rectangle Rect;
    public float Timer;
    const float Duration = 0.15f;

    public SwordHit(Vector2 playerPos, Dir dir)
    {
        var v = dir.ToVec();
        int x = (int)(playerPos.X + v.X * Tiles.Size) - 6;
        int y = (int)(playerPos.Y + v.Y * Tiles.Size) - 6;
        Rect = new System.Drawing.Rectangle(x, y, 12, 12);
        Timer = Duration;
    }

    public bool Update(float dt) { Timer -= dt; return Timer <= 0; }

    public void Draw()
    {
        Raylib_cs.Raylib.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height,
            new Raylib_cs.Color(200, 220, 255, 180));
    }
}
