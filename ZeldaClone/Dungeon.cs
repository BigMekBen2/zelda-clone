using System.Numerics;

namespace ZeldaClone;

class Dungeon
{
    public List<Room> Rooms = [];
    public int Current = 0;
    public List<Enemy> Enemies = [];
    public List<Arrow> Arrows = [];
    public List<Pickup> Pickups = [];

    float transTimer = 0;
    const float TransDur = 0.3f;
    int transDir = -1;

    public Room ActiveRoom => Rooms[Current];

    public void LoadRooms(string dir)
    {
        Rooms.Clear();
        var files = Directory.GetFiles(dir, "room_*.json")
            .OrderBy(f => int.Parse(System.IO.Path.GetFileNameWithoutExtension(f).Replace("room_", "")))
            .ToArray();
        foreach (var f in files) Rooms.Add(Room.Load(f));
        if (Rooms.Count == 0) Rooms.Add(Room.Empty());
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        Enemies.Clear();
        foreach (var s in ActiveRoom.Spawns) Enemies.Add(EnemyFactory.Create(s));
    }

    public void Update(float dt, Player player)
    {
        if (transTimer > 0) { transTimer -= dt; return; }

        foreach (var e in Enemies) e.Update(dt, ActiveRoom, player, Arrows);

        foreach (var a in Arrows) a.Update(dt, ActiveRoom);
        Arrows.RemoveAll(a => a.Dead);

        // arrow-enemy collision
        foreach (var a in Arrows)
        {
            if (!a.FromPlayer) continue;
            foreach (var e in Enemies)
            {
                var hb = e.Hitbox();
                if (hb.Contains((int)a.Pos.X, (int)a.Pos.Y)) { e.Hit(1); a.Dead = true; }
            }
        }

        // enemy arrow -> player
        foreach (var a in Arrows)
        {
            if (a.FromPlayer) continue;
            if (Vector2.Distance(a.Pos, player.Pos) < 8) { player.TakeDamage(1); a.Dead = true; }
        }

        // sword -> enemy
        if (player.ActiveSword != null)
        {
            var sr = player.ActiveSword.Rect;
            foreach (var e in Enemies)
            {
                var hb = e.Hitbox();
                if (hb.IntersectsWith(sr)) e.Hit(2);
            }
        }

        // drop pickups after all combat resolved
        foreach (var e in Enemies.Where(e => e.Dead && e.Drop.HasValue))
            Pickups.Add(new Pickup(e.Pos, e.Drop!.Value));
        Enemies.RemoveAll(e => e.Dead);

        foreach (var p in Pickups) p.Update(dt, player);
        Pickups.RemoveAll(p => p.Collected);

        CheckTransition(player);
    }

    void CheckTransition(Player player)
    {
        // Check if player walks off edge — map to door direction
        int px = (int)(player.Pos.X / Tiles.Size);
        int py = (int)(player.Pos.Y / Tiles.Size);

        (int dir, int target)[] checks = [
            (0, py == 0 ? ActiveRoom.Doors[0] : -1),          // N
            (1, py >= Tiles.Rows - 1 ? ActiveRoom.Doors[1] : -1), // S
            (2, px >= Tiles.Cols - 1 ? ActiveRoom.Doors[2] : -1), // E
            (3, px == 0 ? ActiveRoom.Doors[3] : -1),           // W
        ];

        foreach (var (dir, target) in checks)
        {
            if (target < 0) continue; // -1 = no door, -2 would mean locked but -2 is switch-open
            if (target == -2) continue; // actually open via switch but no next room wired
            if (target >= Rooms.Count) continue;
            GoTo(target, dir);
            // reposition player on other side
            player.Pos = dir switch
            {
                0 => new Vector2(player.Pos.X, (Tiles.Rows - 2) * Tiles.Size),
                1 => new Vector2(player.Pos.X, 1 * Tiles.Size + Tiles.Size / 2f),
                2 => new Vector2(1 * Tiles.Size + Tiles.Size / 2f, player.Pos.Y),
                3 => new Vector2((Tiles.Cols - 2) * Tiles.Size, player.Pos.Y),
                _ => player.Pos,
            };
            return;
        }
    }

    void GoTo(int idx, int fromDir)
    {
        Current = idx;
        SpawnEnemies();
        Arrows.Clear();
        Pickups.Clear();
        transTimer = TransDur;
        transDir = fromDir;
    }

    public void Draw(Player player)
    {
        ActiveRoom.Draw();
        foreach (var p in Pickups) p.Draw();
        foreach (var e in Enemies) e.Draw();
        foreach (var a in Arrows) a.Draw();
        player.Draw();

        if (transTimer > 0)
        {
            float alpha = transTimer / TransDur;
            Raylib_cs.Raylib.DrawRectangle(0, 0,
                Tiles.Cols * Tiles.Size, Tiles.Rows * Tiles.Size,
                new Raylib_cs.Color(0, 0, 0, (int)(alpha * 255)));
        }
    }
}
