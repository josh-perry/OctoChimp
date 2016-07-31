using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms;
using OctoChimp;
using SFML.Graphics;
using Window = System.Windows.Window;

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

            CreateRenderWindow();

            var refreshRate = new TimeSpan(0, 0, 0, 0, 1000 / 60);
            _timer = new DispatcherTimer { Interval = refreshRate };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            Closed += (sender, args) => Environment.Exit(0);
        }

        private void CreateRenderWindow()
        {
            if (_emulator == null)
                return;

            if (_emulator.Renderer.Window != null)
            {
                _emulator.Renderer.Window.SetActive(false);
                _emulator.Renderer.Window.Dispose();
            }

            _emulator.Renderer.Window = new RenderWindow(DrawSurface.Handle);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _emulator?.Rendering();
        }

        private void DrawSurface_SizeChanged(object sender, EventArgs e)
        {
            if(_emulator == null)
                return;

            CreateRenderWindow();
            ResizeWindow();
        }

        private void ResizeWindow()
        {
            DrawSurface.Width = (int)_emulator.Renderer.ScreenWidth;
            DrawSurface.Height = (int)_emulator.Renderer.ScreenHeight;
        }

        private void OpenRomMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            if (_emulator != null)
            {
                _emulator.Renderer.Window.SetActive(false);
                _emulator.Renderer.Window.Dispose();
            }

            _emulator = new Emulator();
            _emulator.LoadGame(new FileInfo(dialog.FileName));
            new Thread(_emulator.Run).Start();

            CreateRenderWindow();
            ResizeWindow();
        }
    }
}
