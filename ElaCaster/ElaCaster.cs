using System;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace ElaCaster
{
    public class ElaCaster
    {
        private RenderWindow window;
        private const int WIDTH = 1024;
        private const int HEIGHT = 512;

        Vector2i playerPos;
        RectangleShape playerShape;

        public ElaCaster()
        {
            window = new RenderWindow(new VideoMode(WIDTH, HEIGHT), "ElaCaster");
        }

        private void Run()
        {
            Init();

            while(window.IsOpen)
            {
                window.DispatchEvents();

                Update();

                Draw();
            }
        }

        private void Init()
        {
            SubEvents();
            InitPlayer();
        }

        private void InitPlayer()
        {
            playerPos = new Vector2i(300, 300);
            playerShape = new RectangleShape(new Vector2f(8.0f, 8.0f));
            playerShape.Position = (Vector2f)playerPos;
            playerShape.FillColor = new Color(255, 255, 0);
            playerShape.Origin = new Vector2f(4.0f, 4.0f);
        }

        private void Draw()
        {
            window.Clear(new Color(77, 77, 77));

            DrawPlayer();

            window.Display();
        }

        private void DrawPlayer()
        {
            window.Draw(playerShape);
        }

        private void Update()
        {
            
        }

        private void SubEvents()
        {
            window.Closed += Window_Closed;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            RenderWindow window = sender as RenderWindow;

            window.Close();
        }

        public static void Main(string[] args)
        {
            ElaCaster elaCaster = new ElaCaster();
            elaCaster.Run();
        }
    }
}
