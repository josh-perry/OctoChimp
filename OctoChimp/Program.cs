using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OctoChimp
{
    internal class Program
    {
        [STAThread]
        private static void Main()
        {
            var emulator = new Emulator();

            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                emulator.LoadGame(new FileInfo(dialog.FileName));
                emulator.Run();
            }
            else
            {
                MessageBox.Show("No ROM selected!");
            }
        }
    }
}
