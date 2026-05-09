using Raylib_cs;
using System.Numerics;

namespace ZeldaClone;

enum EnemyType { Blob, Bat, Skeleton }

abstract class Enemy
{
    public Vector2 Pos;
    public int Health;
    public bool Dead;
    protected float invTimer;

    protected Enemy(int col, int row, int hp)
    {
        Pos = new Vector2(col * Tiles.Size + Tiles.Size / 2f, row * Tiles.Size + Tiles.Size / 2f);
        Health = hp;
    }

    public abstract void Update(float dt, Room room, Player player, List<Arrow> arrows);
    public abstract void Draw();

    protected bool CanOccupy(Vector2 p, Room room, bool ignoreWalls = false)
    {
        if (ignoreWalls) return true;
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

    protected void DrawRect(int w, int h, Color color)
    {
        bool flash = invTimer > 0 && (int)(invTimer * 20) % 2 == 0;
        var c = flash ? new Color(255, 255, 255, 255) : color;
        Raylib.DrawRectangle((int)Pos.X - w / 2, (int)Pos.Y - h / 2, w, h, c);
    }

    public System.Drawing.Rectangle Hitbox(int r = 6)
        => new((int)Pos.X - r, (int)Pos.Y - r, r * 2, r * 2);
}

class Blob : Enemy
{
    Vector2 vel;
    float changeTimer;
    static readonly Random Rng = new();

    public Blob(int col, int row) : base(col, row, 2)
    {
        RandomDir();
    }

    void RandomDir()
    {
        var dirs = new[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
        vel = dirs[Rng.Next(dirs.Length)] * 40f;
        changeTimer = 0.8f + (float)Rng.NextDouble() * 0.8f;
    }

    public override void Update(float dt, Room room, Player player, List<Arrow> arrows)
    {
        if (invTimer > 0) invTimer -= dt;
        changeTimer -= dt;
        if (changeTimer <= 0) RandomDir();

        var next = Pos + vel * dt;
        if (CanOccupy(next, room)) Pos = next;
        else RandomDir();

        ContactDamage(player);
    }

    void ContactDamage(Player p)
    {
        if (Vector2.Distance(Pos, p.Pos) < 12) p.TakeDamage(1);
    }

    public override void Draw() => DrawRect(12, 12, new Color(80, 200, 80, 255));
}

class Bat : Enemy
{
    float speed = 55f;
    static readonly Random Rng = new();

    public Bat(int col, int row) : base(col, row, 1) { }

    public override void Update(float dt, Room room, Player player, List<Arrow> arrows)
    {
        if (invTimer > 0) invTimer -= dt;
        var dir = player.Pos - Pos;
        if (dir != Vector2.Zero) dir = Vector2.Normalize(dir);
        // bats ignore walls
        Pos += dir * speed * dt;
        if (Vector2.Distance(Pos, player.Pos) < 10) player.TakeDamage(1);
    }

    public override void Draw() => DrawRect(10, 8, new Color(120, 80, 180, 255));
}

class Skeleton : Enemy
{
    float shootTimer = 2f;
    static readonly Random Rng = new();

    public Skeleton(int col, int row) : base(col, row, 3) { }

    public override void Update(float dt, Room room, Player player, List<Arrow> arrows)
    {
        if (invTimer > 0) invTimer -= dt;
        shootTimer -= dt;
        if (shootTimer <= 0)
        {
            shootTimer = 2f + (float)Rng.NextDouble();
            var d = BestDir(player);
            arrows.Add(new Arrow(new Vector2(Pos.X, Pos.Y), d, false));
        }
        if (Vector2.Distance(Pos, player.Pos) < 10) player.TakeDamage(1);
    }

    Dir BestDir(Player p)
    {
        var diff = p.Pos - Pos;
        if (Math.Abs(diff.X) > Math.Abs(diff.Y))
            return diff.X > 0 ? Dir.E : Dir.W;
        return diff.Y > 0 ? Dir.S : Dir.N;
    }

    public override void Draw()
    {
        DrawRect(12, 14, new Color(200, 200, 180, 255));
        // eye dots
        Raylib.DrawRectangle((int)Pos.X - 3, (int)Pos.Y - 3, 2, 2, new Color(20, 20, 20, 255));
        Raylib.DrawRectangle((int)Pos.X + 1, (int)Pos.Y - 3, 2, 2, new Color(20, 20, 20, 255));
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
