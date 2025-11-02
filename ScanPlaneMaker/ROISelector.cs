using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanPlaneMaker
{
    internal class ROISelector
    {
        bool only_one_roi;

        private Mat originalImage;
        private Mat displayImage;
        private List<Rect> rois;
        private Point startPoint;
        private Point currentPoint;
        private bool isDrawing;
        private int currentROIIndex;
        private string windowName;
        private Mat frame;

        public ROISelector(Mat frame, bool only_one_roi = true)
        {
            originalImage = frame;
            if (originalImage.Empty())
                throw new ArgumentException("Impossible de charger l'image");

            displayImage = originalImage.Clone();
            rois = new List<Rect>();
            windowName = "Sélecteur ROI Avancé";
            currentROIIndex = -1;
            this.only_one_roi = only_one_roi;
        }

        public static OpenCvSharp.Rect? SelectROI(Mat frame)
        {
            string window_name = "Valid ROI with 'Enter' or 'Space', Cancel with 'c'";
            OpenCvSharp.Rect? newroi = Cv2.SelectROI(window_name, frame.Clone(), true);
            if (((OpenCvSharp.Rect)newroi).Width == 0 || ((OpenCvSharp.Rect)newroi).Height == 0)
                newroi = null;
            Cv2.DestroyWindow(window_name);
            return newroi;
        }


        internal static List<Rect> CreateROIs(Mat frame)
        {
            ROISelector selector = new ROISelector(frame);
            return selector.Run();
        }


        public List<Rect> Run()
        {
            Cv2.NamedWindow(windowName, WindowFlags.AutoSize);
            Cv2.SetMouseCallback(windowName, OnMouseCallback);

            PrintInstructions();

            while (true)
            {
                UpdateDisplay();
                Cv2.ImShow(windowName, displayImage);

                int key = Cv2.WaitKey(30) & 0xFF;

                //if (key == )

                if (key == 27) // ESC
                    break;
                else if (key == (int)'r' || key == (int)'R')
                    ResetAll();
                else if (key == (int)'d' || key == (int)'D')
                    DeleteLastROI();
                else if (key == (int)'s' || key == (int)'S')
                    SaveAllROIs();
                else if (key == (int)'c' || key == (int)'C')
                    ClearAll();
                else if (key == (int)'i' || key == (int)'I')
                    PrintROIInfo();
            }

            Cleanup();

            return GetROIs();
        }

        private void OnMouseCallback(MouseEventTypes @event, int x, int y, MouseEventFlags flags, IntPtr userData)
        {
            switch (@event)
            {
                case MouseEventTypes.LButtonDown:
                    StartSelection(x, y);
                    break;

                case MouseEventTypes.MouseMove:
                    if (isDrawing)
                        UpdateSelection(x, y);
                    break;

                case MouseEventTypes.LButtonUp:
                    FinishSelection(x, y);
                    break;

                case MouseEventTypes.RButtonDown:
                    // Clic droit pour supprimer une ROI
                    RemoveROIAtPoint(x, y);
                    break;
            }
        }

        private void StartSelection(int x, int y)
        {
            isDrawing = true;
            startPoint = new Point(x, y);
            currentPoint = startPoint;
        }

        private void UpdateSelection(int x, int y)
        {
            currentPoint = new Point(x, y);
        }

        private void FinishSelection(int x, int y)
        {
            isDrawing = false;
            currentPoint = new Point(x, y);

            Rect newROI = CalculateROI(startPoint, currentPoint);
            if (newROI.Width > 5 && newROI.Height > 5) // ROI minimum de 5x5 pixels
            {
                rois.Add(newROI);
                Console.WriteLine($"ROI #{rois.Count} ajoutée: {newROI.X},{newROI.Y} - {newROI.Width}x{newROI.Height}");
            }

            while (rois.Count > 2)
                rois.RemoveAt(0);
        }

        private void RemoveROIAtPoint(int x, int y)
        {
            Point clickPoint = new Point(x, y);
            for (int i = rois.Count - 1; i >= 0; i--)
            {
                if (rois[i].Contains(clickPoint))
                {
                    rois.RemoveAt(i);
                    Console.WriteLine($"ROI #{i + 1} supprimée");
                    break;
                }
            }
        }

        private Rect CalculateROI(Point start, Point end)
        {
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(end.X - start.X);
            int height = Math.Abs(end.Y - start.Y);

            // Contraindre dans les limites de l'image
            x = Math.Max(0, Math.Min(x, originalImage.Width - 1));
            y = Math.Max(0, Math.Min(y, originalImage.Height - 1));
            width = Math.Min(width, originalImage.Width - x);
            height = Math.Min(height, originalImage.Height - y);

            return new Rect(x, y, width, height);
        }

        private void UpdateDisplay()
        {
            originalImage.CopyTo(displayImage);

            // Dessiner toutes les ROIs existantes
            for (int i = 0; i < rois.Count; i++)
            {
                Scalar color = GetROIColor(i);
                Cv2.Rectangle(displayImage, rois[i], color, 2);

                // Numéroter les ROIs
                string label = $"ROI {i + 1}";
                Cv2.PutText(displayImage, label,
                           new Point(rois[i].X + 5, rois[i].Y + 20),
                           HersheyFonts.HersheySimplex, 0.6, color, 2);
            }

            // Dessiner la ROI en cours de sélection
            if (isDrawing)
            {
                Rect currentROI = CalculateROI(startPoint, currentPoint);
                Cv2.Rectangle(displayImage, currentROI, Scalar.Red, 2);

                // Afficher les dimensions en temps réel
                string dimensions = $"{currentROI.Width}x{currentROI.Height}";
                Cv2.PutText(displayImage, dimensions,
                           new Point(currentPoint.X + 10, currentPoint.Y - 10),
                           HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 1);
            }

            // Afficher le nombre total de ROIs
            string info = $"ROIs: {rois.Count}";
            Cv2.PutText(displayImage, info, new Point(10, 30),
                       HersheyFonts.HersheySimplex, 0.7, Scalar.White, 2);
            Cv2.PutText(displayImage, info, new Point(10, 30),
                       HersheyFonts.HersheySimplex, 0.7, Scalar.Black, 1);
        }

        private Scalar GetROIColor(int index)
        {
            Scalar[] colors = {
            Scalar.Green, Scalar.Blue, Scalar.Yellow, Scalar.Cyan,
            Scalar.Magenta, Scalar.Orange, Scalar.Pink, Scalar.Purple
        };
            return colors[index % colors.Length];
        }

        private void ResetAll()
        {
            rois.Clear();
            isDrawing = false;
            Console.WriteLine("Toutes les ROIs ont été supprimées");
        }

        private void DeleteLastROI()
        {
            if (rois.Count > 0)
            {
                rois.RemoveAt(rois.Count - 1);
                Console.WriteLine("Dernière ROI supprimée");
            }
        }

        private void ClearAll()
        {
            rois.Clear();
            Console.WriteLine("Toutes les ROIs effacées");
        }

        private void SaveAllROIs()
        {
            if (rois.Count == 0)
            {
                Console.WriteLine("Aucune ROI à sauvegarder");
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            for (int i = 0; i < rois.Count; i++)
            {
                Mat roiImage = new Mat(originalImage, rois[i]);
                string filename = $"roi_{i + 1}_{timestamp}.jpg";
                Cv2.ImWrite(filename, roiImage);
                roiImage.Dispose();
            }

            Console.WriteLine($"{rois.Count} ROI(s) sauvegardée(s)");
        }

        private void PrintROIInfo()
        {
            Console.WriteLine($"\n=== Informations ROI ({rois.Count} ROI(s)) ===");
            for (int i = 0; i < rois.Count; i++)
            {
                Rect roi = rois[i];
                Console.WriteLine($"ROI #{i + 1}: Position({roi.X},{roi.Y}) - Taille({roi.Width}x{roi.Height})");
            }
            Console.WriteLine();
        }

        private void PrintInstructions()
        {
            Console.WriteLine("=== INSTRUCTIONS ===");
            Console.WriteLine("• Clic gauche + glisser : Sélectionner une ROI");
            Console.WriteLine("• Clic droit : Supprimer la ROI sous le curseur");
            Console.WriteLine("• 'R' : Supprimer toutes les ROIs");
            Console.WriteLine("• 'D' : Supprimer la dernière ROI");
            Console.WriteLine("• 'C' : Effacer toutes les ROIs");
            Console.WriteLine("• 'S' : Sauvegarder toutes les ROIs");
            Console.WriteLine("• 'I' : Afficher les informations des ROIs");
            Console.WriteLine("• 'ESC' : Quitter");
            Console.WriteLine("====================\n");
        }

        private void Cleanup()
        {
            Cv2.DestroyAllWindows();
            originalImage?.Dispose();
            displayImage?.Dispose();
        }

        public List<Rect> GetROIs()
        {
            return new List<Rect>(rois);
        }
    }
}
