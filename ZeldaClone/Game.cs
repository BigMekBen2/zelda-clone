using Raylib_cs;

namespace ZeldaClone;

class Game
{
    const int Scale = 3;
    const int GameW = Tiles.Cols * Tiles.Size;
    const int GameH = Tiles.Rows * Tiles.Size + 24;

    Dungeon dungeon = new();
    Player player = new(1, 1);

    public void Run(string roomDir)
    {
        dungeon.LoadRooms(roomDir);

        Raylib.InitWindow(GameW * Scale, GameH * Scale, "Zelda Clone");
        Raylib.SetTargetFPS(60);

        var rt = Raylib.LoadRenderTexture(GameW, GameH);

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            player.Update(dt, dungeon.ActiveRoom, dungeon.Arrows);
            dungeon.Update(dt, player);

            Raylib.BeginTextureMode(rt);
            Raylib.ClearBackground(new Color(15, 12, 8, 255));
            dungeon.Draw(player);
            HUD.Draw(player);
            Raylib.EndTextureMode();

            Raylib.BeginDrawing();
            Raylib.DrawTexturePro(
                rt.Texture,
                new Rectangle(0, 0, GameW, -GameH),
                new Rectangle(0, 0, GameW * Scale, GameH * Scale),
                System.Numerics.Vector2.Zero, 0, Color.White);
            Raylib.EndDrawing();
        }

        Raylib.UnloadRenderTexture(rt);
        Raylib.CloseWindow();
    }
}
