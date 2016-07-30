using System.IO;

namespace OctoChimp
{
    internal class Program
    {
        private static void Main()
        {
            var emulator = new Emulator();
            emulator.LoadGame(new FileInfo(@"C:\Users\Josh\Desktop\c8games\MAZE"));
            emulator.Run();
        }
    }
}
