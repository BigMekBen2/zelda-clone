using ZeldaClone;

string roomDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "rooms");
Directory.CreateDirectory(roomDir);

if (args.Length > 0 && args[0] == "--editor")
{
    new Editor(roomDir).Run();
}
else
{
    new Game().Run(roomDir);
}
