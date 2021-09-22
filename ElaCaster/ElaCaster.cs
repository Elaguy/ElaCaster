using System;
using SFML;
using SFML.Graphics;
using SFML.Window;

namespace ElaCaster
{
    public class ElaCaster
    {
        public static void Main(string[] args)
        {
            RenderWindow window = new RenderWindow(new VideoMode(200, 200), "SFML Test");

            window.Closed += Window_Closed;

            CircleShape circleShape = new CircleShape(100.0f);
            circleShape.FillColor = Color.Green;

            while(window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear();
                window.Draw(circleShape);
                window.Display();
            }
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            RenderWindow window = sender as RenderWindow;
            window.Close();
        }
    }
}
