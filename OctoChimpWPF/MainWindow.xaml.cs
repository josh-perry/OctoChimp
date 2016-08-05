using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OctoChimp;

namespace OctoChimpWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Emulator _emulator;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            var refreshRate = new TimeSpan(0, 0, 0, 0, 1000 / 60);
            _timer = new DispatcherTimer { Interval = refreshRate };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            Closed += (sender, args) => Environment.Exit(0);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_emulator == null)
                return;

            if (!_emulator.DrawFlag)
                return;

            var frame = BitmapFactory.New(64, 48);

            var pixels = _emulator.Screen;

            for (var x = 0; x < pixels.GetLength(0); x++)
            {
                for (var y = 0; y < pixels.GetLength(1); y++)
                {
                    if (pixels[x, y])
                        frame.SetPixel(x, y, Properties.Settings.Default.ForegroundR, Properties.Settings.Default.ForegroundG, Properties.Settings.Default.ForegroundB);
                    else
                        frame.SetPixel(x, y, Properties.Settings.Default.BackgroundR, Properties.Settings.Default.BackgroundG, Properties.Settings.Default.BackgroundB);
                }
            }

            FrameImage.Source = frame;
        }

        private void OpenRomMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            NewEmulator(new FileInfo(dialog.FileName));
        }

        private void NewEmulator(FileInfo file)
        {
            _emulator = new Emulator();
            _emulator.LoadGame(file);

            new Thread(_emulator.Run).Start();
        }

        private void ColourSettingsMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            var window = new ColourSettings();
            window.ShowDialog();
        }
    }
}
