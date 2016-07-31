using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using OctoChimp;
using SFML.Graphics;
using SFML.Window;
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

            _emulator = new Emulator();
            _emulator.LoadGame(new FileInfo(@"C:\Users\Josh\Desktop\c8games\MAZE"));
            new Thread(_emulator.Run).Start();

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
            DrawSurface.Width = (int)_emulator.Renderer.ScreenWidth;
            DrawSurface.Height = (int)_emulator.Renderer.ScreenHeight;

            CreateRenderWindow();
        }
    }
}
