using System.IO;

namespace OctoChimp
{
    class Program
    {
        static void Main(string[] args)
        {
            var emulator = new Emulator();
            emulator.LoadGame(new FileInfo(@"C:\Users\Josh\Desktop\c8games\MAZE"));
            emulator.Run();
        }
    }
}
