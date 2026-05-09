# Zelda NES Clone — Plan

## Stack
- C# / raylib-cs
- .NET 8+
- Aesthetic: Atari 2600/NES/MS-DOS hybrid

---

## Phases

### Phase 1 — Project Setup
- [ ] `dotnet new console`, add raylib-cs NuGet
- [ ] 60fps game loop, fixed 256x224 internal resolution scaled to window
- [ ] 4-color NES-style palette system
- [ ] Sprite/tile sheet loader (PNG, 8x8 or 16x16 tiles)

### Phase 2 — Tile System
- [ ] `Room` struct: 16x10 grid of tile indices (16x16px tiles → 256x160px play area + HUD strip)
- [ ] Tile types: Floor, Wall, Door (locked/open), Water, Void
- [ ] Render room from tile array
- [ ] Collision: solid tiles block movement

### Phase 3 — Room Editor
- [ ] Separate `editor` mode (flag or separate entry point)
- [ ] Render room grid with tile palette panel on the right
- [ ] Left-click to paint tile, right-click to erase (set to floor)
- [ ] Number keys or palette click to select tile type
- [ ] Place enemy spawn markers (type + position)
- [ ] Save/load room to JSON (`rooms/room_N.json`)
- [ ] Navigate between rooms (prev/next), wire door connections between rooms

### Phase 4 — Player
- [ ] Top-down 4-directional movement (grid-aligned feel, smooth pixels)
- [ ] Sword swing: 1-tile hitbox in facing direction, short animation
- [ ] Bow: fire arrow projectile in facing direction; arrow travels until it hits wall or target
- [ ] Arrow hits switch → triggers door open in same room
- [ ] Health (hearts), invincibility frames on hit
- [ ] HUD: hearts, key count, arrow count

### Phase 5 — Dungeon & Room Transitions
- [ ] Dungeon map: list of rooms with N/S/E/W door connections
- [ ] Screen-wipe transition when player walks through open door
- [ ] Locked door requires key item (dropped by enemy or in chest)
- [ ] Switch door: opens when arrow hits switch tile

### Phase 6 — Enemies
- [ ] Base `Enemy`: position, health, contact damage to player
- [ ] **Blob** (walker): bounces between walls, random direction change on collision
- [ ] **Bat**: ignores walls, flutters toward player, faster
- [ ] **Skeleton**: faces player, shoots arrow projectile on timer; arrow same collision as player arrow
- [ ] Enemy death: flash + disappear, chance to drop heart/arrow

### Phase 7 — Boss
- [ ] Boss room (large or single room)
- [ ] Boss: multi-phase or single phase with weak point
- [ ] Weak point only vulnerable when hit by arrow (not sword)
- [ ] Boss death → door opens to exit / win screen

### Phase 8 — Dungeon Layout (Hand-Authored)
- [ ] ~10 rooms authored in the editor
- [ ] Room 1: entrance, blobs
- [ ] Rooms 2-4: exploration, mix of blobs/bats, find arrows/keys
- [ ] Room 5: switch puzzle room (use bow to open door)
- [ ] Rooms 6-8: skeleton gauntlet
- [ ] Room 9: locked door (key from earlier)
- [ ] Room 10: boss room

### Phase 9 — Polish
- [ ] Sound effects (raylib audio): footstep, sword, arrow, hit, door open, boss death
- [ ] Screen flash on damage
- [ ] Simple title screen and game-over/win screen
- [ ] Tile art pass (NES palette, dithering for MS-DOS feel)

---

## File Structure
```
zelda-clone/
  src/
    Game.cs          # entry point, game loop
    Player.cs
    Enemy.cs         # base + subclasses or enum-switched
    Room.cs
    Dungeon.cs
    Editor.cs
    Tiles.cs
    Combat.cs
    HUD.cs
  assets/
    tiles.png
    sprites.png
  rooms/
    room_0.json ... room_9.json
  plan.md
```

---

## Missed Requirements

- **Switches**: arrow (and sword) can trigger switches; switch opens a door in the same room.
- **Item**: only the bow. No other items. Sword is always available.
- **Room editor**: built as a mode within the same binary (or separate entry point), saves to JSON.
- **Enemies**: Blob (walker), Bat (wall-phasing), Skeleton (arrow shooter). No others at start.
- **Boss weak point**: only vulnerable to arrows, not sword.
