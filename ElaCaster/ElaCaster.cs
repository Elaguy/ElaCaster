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

        private float playerX, playerY;
        private float playerDeltaX, playerDeltaY, playerAngle; // playerAngle is in rad
        private RectangleShape playerShape;

        private int mapX = 8, mapY = 8, tileSize = 64;

        private int[] map = {
            1,1,1,1,1,1,1,1,
            1,0,1,0,0,0,0,1,
            1,0,1,0,0,0,0,1,
            1,0,1,0,0,0,0,1,
            1,0,0,0,0,0,0,1,
            1,0,0,0,0,1,0,1,
            1,0,0,0,0,0,0,1,
            1,1,1,1,1,1,1,1
        };

        public ElaCaster()
        {
            window = new RenderWindow(new VideoMode(WIDTH, HEIGHT), "ElaCaster");
            window.SetFramerateLimit(60);
        }

        private void Run()
        {
            Init();

            while(window.IsOpen)
            {
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
            playerX = 300.0f; playerY = 300.0f;
            playerDeltaX = (float)Math.Cos(playerAngle);
            playerDeltaY = (float)Math.Sin(playerAngle);

            playerShape = new RectangleShape(new Vector2f(8.0f, 8.0f));
            playerShape.Position = new Vector2f(playerX, playerY);
            playerShape.FillColor = new Color(255, 255, 0);

            // This is relative to the position!
            // For some reason +X is left and +Y is up, hence (4.0f, 4.0f)
            playerShape.Origin = new Vector2f(4.0f, 4.0f);
        }

        private void Draw()
        {
            window.Clear(new Color(77, 77, 77));

            DrawMap();
            DrawPlayer();

            window.Display();
        }

        private void DrawMap()
        {
            for(int y = 0; y < mapY; y++)
            {
                for(int x = 0; x < mapX; x++)
                {
                    int currentTile = map[y * mapX + x];
                    Color currentTileColor = currentTile == 1 ? Color.White : Color.Black;
                    int scaledX = x * tileSize, scaledY = y * tileSize;

                    Vertex[] tile =
                    {
                        new Vertex(new Vector2f(scaledX + 1, scaledY + 1), currentTileColor),
                        new Vertex(new Vector2f(scaledX + 1, scaledY + tileSize - 1), currentTileColor),
                        new Vertex(new Vector2f(scaledX + tileSize - 1, scaledY + tileSize - 1), currentTileColor),
                        new Vertex(new Vector2f(scaledX + tileSize - 1, scaledY + 1), currentTileColor)
                    };

                    window.Draw(tile, 0, 4, PrimitiveType.Quads, RenderStates.Default);
                }
            }
        }

        private void DrawPlayer()
        {
            window.Draw(playerShape);

            // Draw the direction/angle line originating from the player
            int lineThickness = 4, lineLength = 20;
            Color lineColor = new Color(255, 255, 0);
            RectangleShape dirLine = new RectangleShape
            {
                Size = new Vector2f(lineThickness, lineLength),
                Position = new Vector2f(playerX, playerY),
                Origin = new Vector2f(lineThickness / 2, lineThickness / 2),
                Rotation = (float)((playerAngle * (180 / Math.PI)) - 90),
                FillColor = lineColor
            };
            window.Draw(dirLine);
        }

        private void DrawRays()
        {
            float rayX, rayY;
            float rayAngle = playerAngle;
            float xOffset, yOffset;
            float negCotan = (float)(-1 / Math.Tan(rayAngle));

            for(int r = 0; r < 1; r++)
            {
                //// Check horizontal lines ////
                if(rayAngle > Math.PI) // ray is looking up
                {
                    rayY = ((int) playerY >> 6) << 6;
                    rayX = (float)((playerY - rayY) * negCotan + playerX);
                    yOffset = -64;
                    xOffset = (float)(-yOffset * negCotan);
                }

                if(rayAngle < Math.PI) // ray is looking down
                {
                    rayY = (((int)playerY >> 6) << 6) + 64;
                    rayX = (float)((playerY - rayY) * negCotan + playerX);
                    yOffset = 64;
                    xOffset = (float)(-yOffset * negCotan);
                }
            }
        }

        private void Update()
        {
            window.DispatchEvents();
            CheckKeys();
        }
        
        private void CheckKeys()
        {
            if(Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                playerX += playerDeltaX * 5;
                playerY += playerDeltaY * 5;

                // Update player position in the playerShape obj.
                // Kind of annoying though and need to see if there's a
                // more elegant way
                playerShape.Position = new Vector2f(playerX, playerY);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                playerAngle -= 0.1f;
                playerDeltaX = (float)Math.Cos(playerAngle);
                playerDeltaY = (float)Math.Sin(playerAngle);

                if (playerAngle < 0)
                    playerAngle += (float)(2 * Math.PI);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                playerX -= playerDeltaX * 5;
                playerY -= playerDeltaY * 5;

                playerShape.Position = new Vector2f(playerX, playerY);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                playerAngle += 0.1f;
                playerDeltaX = (float)Math.Cos(playerAngle);
                playerDeltaY = (float)Math.Sin(playerAngle);

                if (playerAngle > 2 * Math.PI)
                    playerAngle -= (float)(2 * Math.PI);
            }
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
