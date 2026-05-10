using Raylib_cs;
using System.Numerics;

namespace ZeldaClone;

enum EnemyType { Blob, Bat, Skeleton }

abstract class Enemy
{
    public Vector2 Pos;
    public int Health;
    public bool Dead;
    public PickupType? Drop;
    protected float invTimer;
    protected float animTimer;
    static readonly Random Rng = new();

    protected Enemy(int col, int row, int hp, PickupType? drop = null)
    {
        Pos = new Vector2(col * Tiles.Size + Tiles.Size / 2f, row * Tiles.Size + Tiles.Size / 2f);
        Health = hp;
        Drop = drop ?? (Rng.NextDouble() < 0.4 ? (PickupType?)RandomDrop() : null);
    }

    static PickupType RandomDrop()
    {
        double r = new Random().NextDouble();
        if (r < 0.4) return PickupType.Heart;
        if (r < 0.7) return PickupType.Arrow;
        return PickupType.Coin;
    }

    public abstract void Update(float dt, Room room, Player player, List<Arrow> arrows);
    public abstract void Draw();

    protected bool CanOccupy(Vector2 p, Room room)
    {
        int col = (int)(p.X / Tiles.Size);
        int row = (int)(p.Y / Tiles.Size);
        if (col < 0 || col >= Tiles.Cols || row < 0 || row >= Tiles.Rows) return false;
        return !Tiles.IsSolid(room.Get(col, row));
    }

    public void Hit(int dmg)
    {
        if (invTimer > 0) return;
        Health -= dmg;
        invTimer = 0.3f;
        if (Health <= 0) Dead = true;
    }

    protected bool Flashing => invTimer > 0 && (int)(invTimer * 20) % 2 == 0;

    protected Color Flash(Color c) => Flashing ? new Color(255, 255, 255, 255) : c;

    protected void Rect(int x, int y, int w, int h, Color c)
        => Raylib.DrawRectangle((int)Pos.X + x, (int)Pos.Y + y, w, h, Flash(c));

    public System.Drawing.Rectangle Hitbox(int r = 6)
        => new((int)Pos.X - r, (int)Pos.Y - r, r * 2, r * 2);
}

class Blob : Enemy
{
    Vector2 vel;
    float changeTimer;
    static readonly Random Rng = new();

    public Blob(int col, int row) : base(col, row, 2) { RandomDir(); }

    void RandomDir()
    {
        var dirs = new[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
        vel = dirs[Rng.Next(dirs.Length)] * 40f;
        changeTimer = 0.8f + (float)Rng.NextDouble() * 0.8f;
    }

    public override void Update(float dt, Room room, Player player, List<Arrow> arrows)
    {
        if (invTimer > 0) invTimer -= dt;
        animTimer += dt;
        changeTimer -= dt;
        if (changeTimer <= 0) RandomDir();

        var next = Pos + vel * dt;
        if (CanOccupy(next, room)) Pos = next;
        else RandomDir();

        if (Vector2.Distance(Pos, player.Pos) < 12) player.TakeDamage(1);
    }

    public override void Draw()
    {
        // outer square pulses on X
        float px = MathF.Sin(animTimer * 4f) * 2f;
        // inner square pulses on Y (offset phase)
        float py = MathF.Sin(animTimer * 4f + 1.5f) * 2f;

        int ow = (int)(12 + px), oh = 10;
        int iw = 7, ih = (int)(7 + py);

        var outer = new Color(60, 180, 60, 255);
        var inner = new Color(120, 230, 100, 255);

        Rect(-ow / 2, -oh / 2, ow, oh, outer);
        Rect(-iw / 2, -ih / 2, iw, ih, inner);
    }
}

class Bat : Enemy
{
    const float Speed = 55f;

    public Bat(int col, int row) : base(col, row, 1) { }

    public override void Update(float dt, Room room, Player player, List<Arrow> arrows)
    {
        if (invTimer > 0) invTimer -= dt;
        animTimer += dt;
        var dir = player.Pos - Pos;
        if (dir != Vector2.Zero) dir = Vector2.Normalize(dir);
        Pos += dir * Speed * dt;
        if (Vector2.Distance(Pos, player.Pos) < 10) player.TakeDamage(1);
    }

    public override void Draw()
    {
        // body: flat center rect
        var body = new Color(100, 60, 160, 255);
        var wing = new Color(140, 90, 200, 255);

        // wing flap: outer rects move on Y
        float flap = MathF.Sin(animTimer * 10f) * 3f;

        // left wing
        Rect(-10, (int)(-2 + flap), 5, 3, wing);
        // right wing
        Rect(5, (int)(-2 + flap), 5, 3, wing);
        // body
        Rect(-4, -2, 8, 4, body);
        // head dot
        Rect(-1, -4, 3, 3, body);
    }
}

class Skeleton : Enemy
{
    float shootTimer = 2f;
    Vector2 vel;
    float moveChangeTimer;
    static readonly Random Rng = new();
    Dir facing = Dir.S;

    public Skeleton(int col, int row) : base(col, row, 3) { RandomMove(); }

    void RandomMove()
    {
        var dirs = new[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
        var d = dirs[Rng.Next(dirs.Length)];
        vel = d * 28f;
        moveChangeTimer = 1f + (float)Rng.NextDouble();
    }

    public override void Update(float dt, Room room, Player player, List<Arrow> arrows)
    {
        if (invTimer > 0) invTimer -= dt;
        animTimer += dt;

        // move
        moveChangeTimer -= dt;
        if (moveChangeTimer <= 0) RandomMove();
        var next = Pos + vel * dt;
        if (CanOccupy(next, room)) Pos = next;
        else RandomMove();

        // face player
        var diff = player.Pos - Pos;
        facing = Math.Abs(diff.X) > Math.Abs(diff.Y)
            ? (diff.X > 0 ? Dir.E : Dir.W)
            : (diff.Y > 0 ? Dir.S : Dir.N);

        // shoot
        shootTimer -= dt;
        if (shootTimer <= 0)
        {
            shootTimer = 1.8f + (float)Rng.NextDouble();
            arrows.Add(new Arrow(new Vector2(Pos.X, Pos.Y), facing, false));
        }

        if (Vector2.Distance(Pos, player.Pos) < 10) player.TakeDamage(1);
    }

    public override void Draw()
    {
        var bone = new Color(210, 205, 185, 255);
        var dark = new Color(30, 25, 20, 255);

        // skull
        Rect(-4, -7, 8, 7, bone);
        // eye sockets
        Rect(-3, -6, 2, 2, dark);
        Rect(1, -6, 2, 2, dark);
        // jaw gap
        Rect(-3, -2, 2, 1, dark);
        Rect(1, -2, 2, 1, dark);

        // ribcage
        Rect(-3, 0, 6, 5, bone);
        Rect(-1, 1, 2, 4, new Color(30, 25, 20, 120)); // center gap

        // legs (bob with walk cycle)
        float bob = MathF.Sin(animTimer * 6f) * 1.5f;
        Rect(-4, 5, 3, (int)(5 + bob), bone);  // left leg
        Rect(1, 5, 3, (int)(5 - bob), bone);   // right leg

        // arms
        Rect(-6, 0, 3, 2, bone);
        Rect(3, 0, 3, 2, bone);
    }
}

static class EnemyFactory
{
    public static Enemy Create(EnemySpawn s) => s.Type switch
    {
        EnemyType.Blob => new Blob(s.Col, s.Row),
        EnemyType.Bat => new Bat(s.Col, s.Row),
        EnemyType.Skeleton => new Skeleton(s.Col, s.Row),
        _ => throw new ArgumentException($"Unknown enemy type {s.Type}"),
    };
}
