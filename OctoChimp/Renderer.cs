using System;
using SFML.Graphics;
using SFML.Window;

namespace OctoChimp
{
    public class Renderer
    {
        public RenderWindow Window;

        public uint ScreenWidth { get; set; }

        public uint ScreenHeight { get; set; }

        public Color BackgroundColour { get; set; }

        public Color ForegroundColour { get; set; }

        public Renderer(uint screenWidth, uint screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }

        public void Update(bool[,] pixels)
        {
            if (Window == null)
                return;

            Window.Clear(BackgroundColour);

            for (var x = 0; x < pixels.GetLength(0); x++)
            {
                for (var y = 0; y < pixels.GetLength(1); y++)
                {
                    var pixel = pixels[x, y];

                    if (!pixel)
                    {
                        continue;
                    }

                    var rectangle = new RectangleShape(new Vector2f(8f, 8f)) {Position = new Vector2f(x*8, y*8)};
                    rectangle.FillColor = ForegroundColour;
                    rectangle.OutlineColor = ForegroundColour;
                    Window.Draw(rectangle);
                }
            }

            Window.Display();
        }

        public void WindowEvents()
        {
            Window?.DispatchEvents();
        }

        public void RecreateWindow(IntPtr handle)
        {
            var context = new ContextSettings { DepthBits = 24 };
            Window = new RenderWindow(handle, context);
            Window.SetActive();
        }
    }
}