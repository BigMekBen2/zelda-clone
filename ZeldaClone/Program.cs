using ZeldaClone;

try
{
    string roomDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "rooms");
    Directory.CreateDirectory(roomDir);
    Console.WriteLine($"Room dir: {Path.GetFullPath(roomDir)}");

    if (args.Length > 0 && args[0] == "--editor")
        new Editor(roomDir).Run();
    else
        new Game().Run(roomDir);
}
catch (Exception ex)
{
    Console.WriteLine($"CRASH: {ex}");
    Console.ReadLine();
}
