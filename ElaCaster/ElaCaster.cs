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

        private const float OneDegAsRad = 0.017453f;

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
            DrawRays();
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
            float rayAngle;
            float xOffset, yOffset;
            float nCotan;
            float nTan;
            int rayMapX, rayMapY, rayMapIndex;
            int depthOfField;
            float distH, distV, finalHX, finalHY, finalVX, finalVY;
            float finalDist;
            Color wallColor;

            rayAngle = playerAngle - OneDegAsRad * 30; // first ray is 30 deg left of player's angle
            if (rayAngle < 0)
                rayAngle += (float)(2 * Math.PI);
            else if (rayAngle > (2 * Math.PI))
                rayAngle -= (float)(2 * Math.PI);

            for(int r = 0; r < 60; r++)
            {
                ////// Check horizontal lines ////

                rayX = 0;  rayY = 0;
                xOffset = 0; yOffset = 0;
                nCotan = (float)(-1 / Math.Tan(rayAngle));
                depthOfField = 0; // determines how many grid lines (either horiz/vert) to check
                distH = 1000000; finalHX = playerX; finalHY = playerY;

                if (rayAngle > Math.PI) // ray is looking up
                {
                    // - 0.0001 is to ensure the tile we're looking at is not the one the player is already in.
                    // Basically prevents the map indexing from lagging behind by 1 tile, leading to the ray cutting
                    // off/hitting wall 1 tile too late
                    rayY = (float)((((int)playerY >> 6) << 6) - 0.0001);
                    rayX = (float)((playerY - rayY) * nCotan + playerX);
                    yOffset = -64;
                    xOffset = (float)(-yOffset * nCotan);
                }

                if (rayAngle < Math.PI) // ray is looking down
                {
                    rayY = (((int)playerY >> 6) << 6) + 64;
                    rayX = (float)((playerY - rayY) * nCotan + playerX);
                    yOffset = 64;
                    xOffset = (float)(-yOffset * nCotan);
                }

                if (rayAngle == 0 || rayAngle == Math.PI) // ray is looking straight to left or right
                {
                    rayX = playerX;
                    rayY = playerY;
                    depthOfField = 8;
                }

                while (depthOfField < 8)
                {
                    // convert ray x,y to map tile number/index
                    rayMapX = (int)rayX >> 6;
                    rayMapY = (int)rayY >> 6;
                    rayMapIndex = rayMapY * mapX + rayMapX;

                    if (rayMapIndex >= mapX * mapY || rayMapIndex < 0) // out of bounds
                        break;

                    if (map[rayMapIndex] == 1) // if in bounds and that index is a wall, then the ray hit a wall
                    {
                        finalHX = rayX;
                        finalHY = rayY;
                        distH = GetDist(playerX, playerY, finalHX, finalHY);

                        break;
                    }

                    else // didn't hit a wall
                    {
                        // so increment to next intersection point
                        rayX += xOffset;
                        rayY += yOffset;
                        depthOfField += 1;

                        distH = GetDist(playerX, playerY, rayX, rayY);
                    }
                }

                //// Check vertical lines ////

                rayX = 0; rayY = 0;
                xOffset = 0; yOffset = 0;
                nTan = (float)-Math.Tan(rayAngle);
                depthOfField = 0;
                distV = 1000000; finalVX = playerX; finalVY = playerY;

                if ((rayAngle > (Math.PI / 2)) && (rayAngle < (3 * Math.PI / 2))) // ray is looking left
                {
                    // - 0.0001 is to ensure the tile we're looking at is not the one the player is already in.
                    // Basically prevents the map indexing from lagging behind by 1 tile, leading to the ray cutting
                    // off/hitting wall 1 tile too late
                    rayX = (float)((((int)playerX >> 6) << 6) - 0.0001);
                    rayY = (float)((playerX - rayX) * nTan + playerY);
                    xOffset = -64;
                    yOffset = (float)(-xOffset * nTan);
                }

                if ((rayAngle < (Math.PI / 2)) || (rayAngle > (3 * Math.PI / 2))) // ray is looking right
                {
                    rayX = (((int)playerX >> 6) << 6) + 64;
                    rayY = (float)((playerX - rayX) * nTan + playerY);
                    xOffset = 64;
                    yOffset = (float)(-xOffset * nTan);
                }

                if ((rayAngle == (Math.PI / 2)) || (rayAngle == (3 * Math.PI / 2))) // ray is looking straight up or down
                {
                    rayX = playerX;
                    rayY = playerY;
                    depthOfField = 8;
                }

                while (depthOfField < 8)
                {
                    // convert ray x,y to map tile number/index
                    rayMapX = (int)rayX >> 6;
                    rayMapY = (int)rayY >> 6;
                    rayMapIndex = rayMapY * mapX + rayMapX;

                    if (rayMapIndex >= mapX * mapY || rayMapIndex < 0) // out of bounds
                        break;

                    if (map[rayMapIndex] == 1) // if in bounds and that index is a wall, then the ray hit a wall
                    {
                        finalVX = rayX;
                        finalVY = rayY;
                        distV = GetDist(playerX, playerY, finalVX, finalVY);

                        break;
                    }

                    else // didn't hit a wall
                    {
                        // so increment to next intersection point
                        rayX += xOffset;
                        rayY += yOffset;
                        depthOfField += 1;

                        distV = GetDist(playerX, playerY, rayX, rayY);
                    }
                }

                //// Finalize the ray ////

                if(distH < distV) // horizontal ray is shorter, use it (horizontal wall hit)
                {
                    rayX = finalHX;
                    rayY = finalHY;
                    finalDist = distH;
                    wallColor = new Color(178, 0, 0); // 0.7 * 255 (trunacated) = 178
                }

                else // vertical ray is shorter, use it (vertical wall hit)
                {
                    rayX = finalVX;
                    rayY = finalVY;
                    finalDist = distV;
                    wallColor = new Color(229, 0, 0); // 0.9 * 255 (trunacated) = 229
                }

                // draw the ray line
                Vertex[] rayLineVert =
                {
                    new Vertex(new Vector2f(playerX, playerY), Color.Red),
                    new Vertex(new Vector2f(rayX, rayY), Color.Red)
                };
                window.Draw(rayLineVert, 0, 2, PrimitiveType.Lines);

                // fix fisheye effect
                float deltaAngle = playerAngle - rayAngle;
                if (deltaAngle < 0)
                    deltaAngle += (float)(2 * Math.PI);
                else if (deltaAngle > (2 * Math.PI))
                    deltaAngle -= (float)(2 * Math.PI);
                finalDist = (float)(finalDist * Math.Cos(deltaAngle));

                //// Draw "3D" Scene ////
                // Height of 3D scene = 320, Width = r (60) * 8 = 480
                float lineHeight = (tileSize * (160 * 2)) / finalDist;
                if (lineHeight > (160 * 2)) // max lineHeight = 160 * 2
                    lineHeight = 160 * 2;

                float lineVOffset = (320 / 2) - lineHeight / 2; // center line on screen

                RectangleShape wallLine = new RectangleShape(new Vector2f(8, lineHeight));
                wallLine.FillColor = wallColor;
                wallLine.Position = new Vector2f((r * 8) + 512, lineVOffset);
                window.Draw(wallLine);

                rayAngle += OneDegAsRad;
                if (rayAngle < 0)
                    rayAngle += (float)(2 * Math.PI);
                else if (rayAngle > (2 * Math.PI))
                    rayAngle -= (float)(2 * Math.PI);
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
                playerAngle -= 0.05f;
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
                playerAngle += 0.05f;
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

        private float GetDist(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public static void Main(string[] args)
        {
            ElaCaster elaCaster = new ElaCaster();
            elaCaster.Run();
        }
    }
}
