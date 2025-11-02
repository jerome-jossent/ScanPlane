using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanPlaneMaker
{
    internal class ImageMeasure
    {
        static List<Point> points = new List<Point>();
        static Mat _image;
        static Mat displayImage;
        static double distance;

        public static double _Display(Mat image)
        {
            if (image == null || image.Empty())
                return -1;
            _image = image.Clone();
            displayImage = image.Clone();

            // Crée une fenêtre et associe un callback pour la souris
            Cv2.NamedWindow("Image", WindowFlags.Normal | WindowFlags.KeepRatio);
            Cv2.SetMouseCallback("Image", OnMouse);

            while (true)
            {
                Cv2.ImShow("Image", displayImage);
                int key = Cv2.WaitKey(1);
                if (key == 27) // Échappement pour quitter
                    break;
            }

            Cv2.DestroyAllWindows();
            return distance;
        }

        static void OnMouse(MouseEventTypes eventType, int x, int y, MouseEventFlags flags, IntPtr userdata)
        {
            if (points.Count == 1)
                if (eventType == MouseEventTypes.MouseMove)
                {
                    displayImage = _image.Clone();
                    DrawCross(displayImage, points[0], Scalar.Red);
                    Point mousepoint = new Point(x, y);
                    DrawLineAndDisplayMeasure(points[0], mousepoint);
                }

            if (eventType == MouseEventTypes.LButtonDown)
            {
                if (points.Count == 2)
                {
                    points.Clear();
                    displayImage = _image.Clone();
                }

                Point p = new Point(x, y);
                points.Add(p);

                DrawCross(displayImage, p, Scalar.Red);

                if (points.Count == 2)                
                    DrawLineAndDisplayMeasure(points[0], points[1]);                
            }
        }

        static void DrawCross(Mat img, Point center, Scalar color, int size = 5, int thickness = 1)
        {
            Cv2.Line(img, new Point(center.X - size, center.Y), new Point(center.X + size, center.Y), color, thickness);
            Cv2.Line(img, new Point(center.X, center.Y - size), new Point(center.X, center.Y + size), color, thickness);
        }

        static void DrawLineAndDisplayMeasure(Point A, Point B)
        {
            distance = Point.Distance(A, B);
            string text = $"{distance:F1}px";
            Point midPoint = new Point((A.X + B.X) / 2, (A.Y + B.Y) / 2);

            // Trace une ligne entre les points
            Cv2.Line(displayImage, A, B, Scalar.Green, 1);

            // Affiche la distance
            Cv2.PutText(displayImage, text, midPoint, HersheyFonts.HersheySimplex, 1, Scalar.Black, 4);
            Cv2.PutText(displayImage, text, midPoint, HersheyFonts.HersheySimplex, 1, Scalar.White, 1);
        }
    }
}
