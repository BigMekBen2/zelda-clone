using Raylib_cs;
using System.Numerics;

namespace ZeldaClone;

class Editor
{
    List<Room> rooms = [Room.Empty()];
    int current = 0;
    TileType selected = TileType.Floor;
    EnemyType enemySelected = EnemyType.Blob;
    bool enemyMode = false;
    string roomDir;

    const int Scale = 5;
    const int PaletteX = Tiles.Cols * Tiles.Size + 4;
    const int WinW = Tiles.Cols * Tiles.Size + 120;
    const int WinH = Tiles.Rows * Tiles.Size + 24;

    static readonly (TileType t, string label)[] TilePalette = [
        (TileType.Floor, "Floor"),
        (TileType.Wall, "Wall"),
        (TileType.DoorLocked, "DoorLk"),
        (TileType.DoorOpen, "DoorOp"),
        (TileType.Water, "Water"),
        (TileType.Switch, "Switch"),
        (TileType.Void, "Void"),
    ];

    static readonly (EnemyType t, string label)[] EnemyPalette = [
        (EnemyType.Blob, "Blob"),
        (EnemyType.Bat, "Bat"),
        (EnemyType.Skeleton, "Skel"),
    ];

    public Editor(string dir)
    {
        roomDir = dir;
        Directory.CreateDirectory(dir);
        LoadAll();
    }

    void LoadAll()
    {
        rooms.Clear();
        var files = Directory.GetFiles(roomDir, "room_*.json")
            .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Replace("room_", "")))
            .ToArray();
        foreach (var f in files) rooms.Add(Room.Load(f));
        if (rooms.Count == 0) rooms.Add(Room.Empty());
    }

    void SaveAll()
    {
        for (int i = 0; i < rooms.Count; i++)
            rooms[i].Save(Path.Combine(roomDir, $"room_{i}.json"));
    }

    public void Run()
    {
        Raylib.InitWindow(WinW * Scale, WinH * Scale, "Zelda Editor");
        Raylib.SetTargetFPS(60);

        var rt = Raylib.LoadRenderTexture(WinW, WinH);

        while (!Raylib.WindowShouldClose())
        {
            UpdateInput();
            Raylib.BeginTextureMode(rt);
            Raylib.ClearBackground(new Color(15, 12, 8, 255));
            Draw();
            Raylib.EndTextureMode();

            Raylib.BeginDrawing();
            Raylib.DrawTexturePro(
                rt.Texture,
                new Rectangle(0, 0, WinW, -WinH),
                new Rectangle(0, 0, WinW * Scale, WinH * Scale),
                System.Numerics.Vector2.Zero, 0, Color.White);
            Raylib.EndDrawing();
        }

        Raylib.UnloadRenderTexture(rt);
        Raylib.CloseWindow();
    }

    void UpdateInput()
    {
        // switch mode
        if (Raylib.IsKeyPressed(KeyboardKey.E)) enemyMode = !enemyMode;

        // navigate rooms
        if (Raylib.IsKeyPressed(KeyboardKey.Right))
        {
            if (current < rooms.Count - 1) current++;
            else { rooms.Add(Room.Empty()); current = rooms.Count - 1; }
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Left) && current > 0) current--;

        // save
        if (Raylib.IsKeyPressed(KeyboardKey.S) && Raylib.IsKeyDown(KeyboardKey.LeftControl))
            SaveAll();

        var rawMouse = Raylib.GetMousePosition();
        var mouse = rawMouse / Scale;
        int mc = (int)(mouse.X / Tiles.Size);
        int mr = (int)(mouse.Y / Tiles.Size);
        bool inGrid = mc >= 0 && mc < Tiles.Cols && mr >= 0 && mr < Tiles.Rows;

        if (!enemyMode)
        {
            // palette selection
            for (int i = 0; i < TilePalette.Length; i++)
            {
                int py = 4 + i * 18;
                if (Raylib.IsMouseButtonPressed(MouseButton.Left)
                    && mouse.X >= PaletteX && mouse.Y >= py && mouse.Y < py + 16)
                    selected = TilePalette[i].t;
            }

            // paint
            if (inGrid)
            {
                if (Raylib.IsMouseButtonDown(MouseButton.Left))
                    rooms[current].Set(mc, mr, selected);
                if (Raylib.IsMouseButtonDown(MouseButton.Right))
                {
                    rooms[current].Set(mc, mr, TileType.Floor);
                    rooms[current].Spawns.RemoveAll(s => s.Col == mc && s.Row == mr);
                }
            }
        }
        else
        {
            // enemy palette
            for (int i = 0; i < EnemyPalette.Length; i++)
            {
                int py = 4 + i * 18;
                if (Raylib.IsMouseButtonPressed(MouseButton.Left)
                    && mouse.X >= PaletteX && mouse.Y >= py && mouse.Y < py + 16)
                    enemySelected = EnemyPalette[i].t;
            }

            if (inGrid && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                // remove existing spawn at cell
                rooms[current].Spawns.RemoveAll(s => s.Col == mc && s.Row == mr);
                rooms[current].Spawns.Add(new EnemySpawn { Type = enemySelected, Col = mc, Row = mr });
            }
            if (inGrid && Raylib.IsMouseButtonPressed(MouseButton.Right))
                rooms[current].Spawns.RemoveAll(s => s.Col == mc && s.Row == mr);
        }
    }

    void Draw()
    {
        rooms[current].Draw();

        // enemy spawns overlay
        foreach (var s in rooms[current].Spawns)
        {
            int x = s.Col * Tiles.Size + 2;
            int y = s.Row * Tiles.Size + 2;
            var c = s.Type switch
            {
                EnemyType.Blob => new Color(80, 200, 80, 200),
                EnemyType.Bat => new Color(120, 80, 180, 200),
                EnemyType.Skeleton => new Color(200, 200, 180, 200),
                _ => new Color(255, 255, 255, 200),
            };
            Raylib.DrawRectangle(x, y, 12, 12, c);
        }

        // palette panel
        Raylib.DrawRectangle(PaletteX - 2, 0, 120, Tiles.Rows * Tiles.Size, new Color(20, 16, 10, 255));
        if (!enemyMode)
        {
            for (int i = 0; i < TilePalette.Length; i++)
            {
                var (t, label) = TilePalette[i];
                int py = 4 + i * 18;
                bool sel = t == selected;
                Raylib.DrawRectangle(PaletteX, py, 14, 14, sel ? new Color(255, 255, 255, 255) : new Color(80, 80, 80, 255));
                Tiles.Draw(t, 0, 0); // just for color reference drawn in palette inline
                Raylib.DrawText(label, PaletteX + 18, py + 2, 8, sel ? new Color(255, 220, 80, 255) : new Color(180, 160, 120, 255));
            }
        }
        else
        {
            Raylib.DrawText("ENEMIES", PaletteX + 2, 0, 8, new Color(220, 180, 80, 255));
            for (int i = 0; i < EnemyPalette.Length; i++)
            {
                var (t, label) = EnemyPalette[i];
                int py = 12 + i * 18;
                bool sel = t == enemySelected;
                Raylib.DrawText(label, PaletteX + 4, py, 8, sel ? new Color(255, 220, 80, 255) : new Color(180, 160, 120, 255));
            }
        }

        // HUD bar
        int hudY = Tiles.Rows * Tiles.Size;
        Raylib.DrawRectangle(0, hudY, Tiles.Cols * Tiles.Size, 20, new Color(10, 8, 6, 255));
        Raylib.DrawText($"Room {current + 1}/{rooms.Count}  [</>] nav  [E] toggle  [Ctrl+S] save  mode:{(enemyMode ? "ENEMY" : "TILE")}", 4, hudY + 4, 8, new Color(180, 160, 120, 255));
    }
}
