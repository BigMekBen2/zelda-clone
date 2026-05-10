using Raylib_cs;

namespace ZeldaClone;

static class HUD
{
    const int HudY = Tiles.Rows * Tiles.Size + 2;
    static readonly Color Heart = new(200, 40, 40, 255);
    static readonly Color Empty = new(60, 20, 20, 255);
    static readonly Color ArrowColor = new(220, 200, 80, 255);

    public static void Draw(Player p)
    {
        Raylib.DrawRectangle(0, Tiles.Rows * Tiles.Size, Tiles.Cols * Tiles.Size, 24, new Color(10, 8, 6, 255));
        for (int i = 0; i < p.MaxHearts; i++)
        {
            var c = i < p.Hearts ? Heart : Empty;
            Raylib.DrawRectangle(4 + i * 12, HudY, 10, 10, c);
        }
        int arrowX = 4 + p.MaxHearts * 12 + 8;
        Raylib.DrawText($">{p.Arrows}", arrowX, HudY, 10, ArrowColor);
        Raylib.DrawText($"${p.Coins}", arrowX + 36, HudY, 10, new Color(220, 185, 30, 255));
    }
}
