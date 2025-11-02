using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanPlaneMaker
{
    internal class SuperResolution
    {
        public enum SuperResolutionType { moyenne, mediane }

        public static Mat MakeSuperResolutionFrom(List<Mat> alignedImages, SuperResolutionType superResolutionType = SuperResolutionType.moyenne, double sharpeningFactor = 1.5, double scaleFactor = 2)
        {
            //Combinaison des images
            Mat stackedImage;
            switch (superResolutionType)
            {
                case SuperResolutionType.mediane:
                    stackedImage = Mediane(alignedImages);
                    break;

                case SuperResolutionType.moyenne:
                default:
                    stackedImage = Moyenne(alignedImages);
                    break;
            }

            //Amélioration des détails
            Mat enhancedImage = EnhanceDetails(stackedImage, sharpeningFactor);

            //Augmentation de la résolution
            Mat highResImage = IncreaseResolution(enhancedImage, scaleFactor);

            // Nettoyage
            stackedImage.Dispose();
            enhancedImage.Dispose();

            return highResImage;
        }

        /// <summary>
        /// Combine les images alignées pour augmenter la netteté
        /// </summary>
        static Mat Moyenne(List<Mat> images)
        {
            if (images.Count == 0)
                return null;

            // Récupérer les dimensions de l'image
            int height = images[0].Height;
            int width = images[0].Width;
            int channels = images[0].Channels();

            // Créer la matrice de résultat
            var result = new Mat(height, width, MatType.CV_8UC3);



            List<byte> byteList = new List<byte> { 10, 20, 30, 40, 50 };

            // Method 1: Using LINQ Average()
            double averageLinq = byteList.Select(b => (double)b).Average();



            // Pour chaque pixel et chaque canal
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Liste des valeurs de chaque canal pour toutes les images
                    var valuesB = new List<byte>();
                    var valuesG = new List<byte>();
                    var valuesR = new List<byte>();

                    // Récupérer les valeurs des pixels à la même position dans toutes les images
                    foreach (var img in images)
                    {
                        var pixel = img.Get<Vec3b>(y, x);
                        valuesB.Add(pixel.Item0);
                        valuesG.Add(pixel.Item1);
                        valuesR.Add(pixel.Item2);
                    }

                    // Calcul de la moyenne pour chaque canal
                    byte medianB = (byte)valuesB.Select(b => (double)b).Average();                    
                    byte medianG = (byte)valuesG.Select(b => (double)b).Average();
                    byte medianR = (byte)valuesR.Select(b => (double)b).Average();

                    // Affecter la valeur médiane au pixel résultat
                    result.Set(y, x, new Vec3b(medianB, medianG, medianR));
                }
            }

            return result;
        }

        /// <summary>
        /// Combine les images alignées pour augmenter la netteté
        /// </summary>
        static Mat Mediane(List<Mat> images)
        {
            if (images.Count == 0)
                return null;

            // Récupérer les dimensions de l'image
            int height = images[0].Height;
            int width = images[0].Width;
            int channels = images[0].Channels();

            // Créer la matrice de résultat
            var result = new Mat(height, width, MatType.CV_8UC3);

            // Pour chaque pixel et chaque canal
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Liste des valeurs de chaque canal pour toutes les images
                    var valuesB = new List<byte>();
                    var valuesG = new List<byte>();
                    var valuesR = new List<byte>();

                    // Récupérer les valeurs des pixels à la même position dans toutes les images
                    foreach (var img in images)
                    {
                        var pixel = img.Get<Vec3b>(y, x);
                        valuesB.Add(pixel.Item0);
                        valuesG.Add(pixel.Item1);
                        valuesR.Add(pixel.Item2);
                    }

                    // Trier les valeurs pour déterminer la médiane
                    valuesB.Sort();
                    valuesG.Sort();
                    valuesR.Sort();

                    // Calcul de la médiane pour chaque canal
                    byte medianB = valuesB[valuesB.Count / 2];
                    byte medianG = valuesG[valuesG.Count / 2];
                    byte medianR = valuesR[valuesR.Count / 2];

                    // Affecter la valeur médiane au pixel résultat
                    result.Set(y, x, new Vec3b(medianB, medianG, medianR));
                }
            }

            return result;
        }

        /// <summary>
        /// Améliore les détails de l'image finale
        /// </summary>
        private static Mat EnhanceDetails(Mat image, double sharpeningFactor = 1.5)
        {
            // Filtre de netteté unsharp mask
            var gaussian = new Mat();
            Cv2.GaussianBlur(image, gaussian, new Size(0, 0), 3);

            var unsharpImage = new Mat();
            Cv2.AddWeighted(image, sharpeningFactor, gaussian, -0.5, 0, unsharpImage);

            return unsharpImage;
        }

        /// <summary>
        /// Augmente la résolution de l'image
        /// </summary>
        private static Mat IncreaseResolution(Mat image, double scaleFactor = 2.0)
        {
            int newHeight = (int)(image.Rows * scaleFactor);
            int newWidth = (int)(image.Cols * scaleFactor);

            // Interpolation bicubique
            var resizedImage = new Mat();
            Cv2.Resize(image, resizedImage, new Size(newWidth, newHeight), 0, 0, InterpolationFlags.Cubic);

            // Création d'une version floutée pour l'unsharp mask
            var blurred = new Mat();
            Cv2.GaussianBlur(resizedImage, blurred, new Size(0, 0), 2);

            // Application de l'unsharp mask avec coefficient modéré
            var sharpened = new Mat();
            Cv2.AddWeighted(resizedImage, 1.5, blurred, -0.5, 0, sharpened);

            return sharpened;
        }


    }
}
