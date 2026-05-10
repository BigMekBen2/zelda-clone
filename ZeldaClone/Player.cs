using Raylib_cs;
using System.Numerics;

namespace ZeldaClone;

class Player
{
    public Vector2 Pos;
    public Dir Facing = Dir.S;
    public int Hearts = 6;
    public int MaxHearts = 6;
    public int Arrows = 10;
    public int Coins = 0;
    public bool HasBow = true;

    float invTimer = 0f;
    float swordTimer = 0f;
    const float SwordDur = 0.15f;
    const float InvDur = 0.8f;
    const float Speed = 80f;

    public SwordHit? ActiveSword;
    public List<Arrow> Arrows_ = [];

    public Player(int col, int row)
    {
        Pos = new Vector2(col * Tiles.Size + Tiles.Size / 2f, row * Tiles.Size + Tiles.Size / 2f);
    }

    public void Update(float dt, Room room, List<Arrow> arrows)
    {
        // movement
        var move = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))    { move.Y -= 1; Facing = Dir.N; }
        if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))  { move.Y += 1; Facing = Dir.S; }
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  { move.X -= 1; Facing = Dir.W; }
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) { move.X += 1; Facing = Dir.E; }

        if (move != Vector2.Zero)
        {
            move = Vector2.Normalize(move) * Speed * dt;
            TryMove(move, room);
        }

        // sword
        if (swordTimer > 0) swordTimer -= dt;
        if (Raylib.IsKeyPressed(KeyboardKey.Z) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            if (swordTimer <= 0)
            {
                swordTimer = SwordDur;
                ActiveSword = new SwordHit(Pos, Facing);
                // sword can also trigger switch
                CheckSwordSwitch(room);
            }
        }
        else if (swordTimer <= 0) ActiveSword = null;

        // bow
        if ((Raylib.IsKeyPressed(KeyboardKey.X) || Raylib.IsKeyPressed(KeyboardKey.LeftShift))
            && HasBow && Arrows > 0)
        {
            Arrows--;
            arrows.Add(new Arrow(new Vector2(Pos.X, Pos.Y), Facing, true));
        }

        if (invTimer > 0) invTimer -= dt;
    }

    void TryMove(Vector2 delta, Room room)
    {
        // try X then Y separately for wall sliding
        var next = Pos + new Vector2(delta.X, 0);
        if (CanOccupy(next, room)) Pos = next;
        next = Pos + new Vector2(0, delta.Y);
        if (CanOccupy(next, room)) Pos = next;
    }

    bool CanOccupy(Vector2 p, Room room)
    {
        int r = 4; // half-size of player hitbox
        foreach (var corner in Corners(p, r))
        {
            int col = (int)(corner.X / Tiles.Size);
            int row = (int)(corner.Y / Tiles.Size);
            if (col < 0 || col >= Tiles.Cols || row < 0 || row >= Tiles.Rows) return false;
            if (Tiles.IsSolid(room.Get(col, row))) return false;
        }
        return true;
    }

    static Vector2[] Corners(Vector2 p, int r) => [
        new(p.X - r, p.Y - r), new(p.X + r, p.Y - r),
        new(p.X - r, p.Y + r), new(p.X + r, p.Y + r),
    ];

    void CheckSwordSwitch(Room room)
    {
        var v = Facing.ToVec();
        int col = (int)((Pos.X + v.X * Tiles.Size) / Tiles.Size);
        int row = (int)((Pos.Y + v.Y * Tiles.Size) / Tiles.Size);
        if (col < 0 || col >= Tiles.Cols || row < 0 || row >= Tiles.Rows) return;
        if (room.Get(col, row) == TileType.Switch)
        {
            room.Set(col, row, TileType.SwitchOn);
            int idx = row * Tiles.Cols + col;
            if (room.SwitchDoors.TryGetValue(idx, out int doorDir))
                room.Doors[doorDir] = -2;
        }
    }

    public void TakeDamage(int dmg)
    {
        if (invTimer > 0) return;
        Hearts -= dmg;
        invTimer = InvDur;
    }

    public bool IsInvincible => invTimer > 0;

    public void Draw()
    {
        bool visible = invTimer <= 0 || (int)(invTimer * 10) % 2 == 0;
        if (!visible) return;
        int x = (int)Pos.X - 7;
        int y = (int)Pos.Y - 7;
        Raylib.DrawRectangle(x, y, 14, 14, new Color(80, 160, 80, 255));
        // direction indicator
        var v = Facing.ToVec();
        Raylib.DrawRectangle((int)(Pos.X + v.X * 5) - 2, (int)(Pos.Y + v.Y * 5) - 2, 4, 4,
            new Color(200, 240, 200, 255));
        ActiveSword?.Draw();
    }
}
