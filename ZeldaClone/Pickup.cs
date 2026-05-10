using Raylib_cs;
using System.Numerics;

namespace ZeldaClone;

enum PickupType { Heart, Arrow, Coin }

class Pickup
{
    public Vector2 Pos;
    public PickupType Type;
    public bool Collected;
    float animTimer;

    public Pickup(Vector2 pos, PickupType type) { Pos = pos; Type = type; }

    public void Update(float dt, Player player)
    {
        animTimer += dt;
        if (Vector2.Distance(Pos, player.Pos) < 10)
        {
            Collected = true;
            switch (Type)
            {
                case PickupType.Heart:
                    player.Hearts = Math.Min(player.Hearts + 2, player.MaxHearts);
                    break;
                case PickupType.Arrow:
                    player.Arrows += 5;
                    break;
                case PickupType.Coin:
                    player.Coins += 1;
                    break;
            }
        }
    }

    public void Draw()
    {
        if (Collected) return;
        float bob = MathF.Sin(animTimer * 4f) * 1.5f;
        int x = (int)Pos.X;
        int y = (int)(Pos.Y + bob);

        switch (Type)
        {
            case PickupType.Heart:
                DrawHeart(x, y);
                break;
            case PickupType.Arrow:
                DrawArrowPickup(x, y);
                break;
            case PickupType.Coin:
                DrawCoin(x, y);
                break;
        }
    }

    static void DrawHeart(int x, int y)
    {
        var red = new Color(220, 40, 40, 255);
        // two top bumps + triangle bottom
        Raylib.DrawRectangle(x - 4, y - 3, 3, 2, red);
        Raylib.DrawRectangle(x + 1, y - 3, 3, 2, red);
        Raylib.DrawRectangle(x - 5, y - 1, 11, 3, red);
        Raylib.DrawRectangle(x - 3, y + 2, 7, 2, red);
        Raylib.DrawRectangle(x - 1, y + 4, 3, 2, red);
    }

    static void DrawArrowPickup(int x, int y)
    {
        var shaft = new Color(180, 130, 60, 255);
        var tip   = new Color(200, 200, 180, 255);
        // shaft
        Raylib.DrawRectangle(x - 4, y - 1, 9, 2, shaft);
        // tip
        Raylib.DrawRectangle(x + 4, y - 2, 2, 5, tip);
        Raylib.DrawRectangle(x + 6, y - 1, 1, 3, tip);
        // fletching
        Raylib.DrawRectangle(x - 5, y - 3, 2, 2, shaft);
        Raylib.DrawRectangle(x - 5, y + 1, 2, 2, shaft);
    }

    static void DrawCoin(int x, int y)
    {
        var gold  = new Color(220, 185, 30, 255);
        var shine = new Color(255, 230, 100, 255);
        Raylib.DrawRectangle(x - 3, y - 4, 6, 8, gold);
        Raylib.DrawRectangle(x - 4, y - 2, 8, 4, gold);
        // shine
        Raylib.DrawRectangle(x - 1, y - 3, 2, 3, shine);
    }
}
