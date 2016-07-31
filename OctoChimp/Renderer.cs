using SFML.Graphics;
using SFML.Window;

namespace OctoChimp
{
    public class Renderer
    {
        private RenderWindow _window;

        private uint _screenWidth;

        private uint _screenHeight;

        public Renderer(uint screenWidth, uint screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;

            _window = new RenderWindow(new VideoMode(screenWidth, screenHeight), "OctoChimp");
            _window.SetActive();
        }

        public void Update(bool[,] pixels)
        {
            _window.Clear();

            for (var x = 0; x < pixels.GetLength(0); x++)
            {
                for (var y = 0; y < pixels.GetLength(1); y++)
                {
                    var pixel = pixels[x, y];

                    if (!pixel)
                    {
                        continue;
                    }

                    var rectangle = new RectangleShape(new Vector2f(8f, 8f));
                    rectangle.Position = new Vector2f(x * 8, y * 8);
                    _window.Draw(rectangle);
                }
            }

            _window.Display();
        }

        public void WindowEvents()
        {
            _window.DispatchEvents();
        }

        public void SetKeys()
        {

        }
    }
}