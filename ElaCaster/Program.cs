using System;
using SFML;
using SFML.Graphics;
using SFML.Window;

namespace ElaCaster
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RenderWindow window = new RenderWindow(new VideoMode(200, 200), "SFML Test");

            CircleShape circleShape = new CircleShape(100.0f);
            circleShape.FillColor = Color.Green;

            while(window.IsOpen)
            {
                window.Clear();
                window.Draw(circleShape);
                window.Display();
            }
        }
    }
}
