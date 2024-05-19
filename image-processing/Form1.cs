using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Net.Mime.MediaTypeNames;

namespace Goruntu_Isleme
{
    public partial class Goruntu_Isleme : Form
    {
        private Bitmap selectedBitmap;
        private Action storedFunction;
        public Goruntu_Isleme()
        {
            InitializeComponent();        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            file.RestoreDirectory = true;
            if (file.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = file.FileName;
                selectedBitmap = new Bitmap(selectedFilePath);
                pictureBox1.Image = selectedBitmap;
            }
        }
        static Bitmap ApplySobelFilter(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            Bitmap resultImage = new Bitmap(width, height);

            int[,] Gx = {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
            };

            int[,] Gy = {
            { -1, -2, -1 },
            {  0,  0,  0 },
            {  1,  2,  1 }
            };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int pixelX = (
                        (Gx[0, 0] * image.GetPixel(x - 1, y - 1).R) + (Gx[0, 1] * image.GetPixel(x, y - 1).R) + (Gx[0, 2] * image.GetPixel(x + 1, y - 1).R) +
                        (Gx[1, 0] * image.GetPixel(x - 1, y).R) + (Gx[1, 1] * image.GetPixel(x, y).R) + (Gx[1, 2] * image.GetPixel(x + 1, y).R) +
                        (Gx[2, 0] * image.GetPixel(x - 1, y + 1).R) + (Gx[2, 1] * image.GetPixel(x, y + 1).R) + (Gx[2, 2] * image.GetPixel(x + 1, y + 1).R)
                    );

                    int pixelY = (
                        (Gy[0, 0] * image.GetPixel(x - 1, y - 1).R) + (Gy[0, 1] * image.GetPixel(x, y - 1).R) + (Gy[0, 2] * image.GetPixel(x + 1, y - 1).R) +
                        (Gy[1, 0] * image.GetPixel(x - 1, y).R) + (Gy[1, 1] * image.GetPixel(x, y).R) + (Gy[1, 2] * image.GetPixel(x + 1, y).R) +
                        (Gy[2, 0] * image.GetPixel(x - 1, y + 1).R) + (Gy[2, 1] * image.GetPixel(x, y + 1).R) + (Gy[2, 2] * image.GetPixel(x + 1, y + 1).R)
                    );

                    int edgeStrength = (int)Math.Sqrt((pixelX * pixelX) + (pixelY * pixelY));
                    edgeStrength = Math.Min(edgeStrength, 255); // 255'i aşmasın

                    Color edgeColor = Color.FromArgb(edgeStrength, edgeStrength, edgeStrength);

                    resultImage.SetPixel(x, y, edgeColor);
                }
            }

            return resultImage;
        }

        static Bitmap parlaklık(Bitmap originalImage,int deger)
        {
            int width = originalImage.Width;
            int height = originalImage.Height;

            Bitmap resultImage = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = originalImage.GetPixel(x, y);

                    int r = Math.Min(pixel.R + deger, 255);
                    int g = Math.Min(pixel.G + deger, 255);
                    int b = Math.Min(pixel.B + deger, 255);

                    Color newPixel = Color.FromArgb(pixel.A, r, g, b);

                    resultImage.SetPixel(x, y, newPixel);
                }
            }
            return resultImage;
        }
        static Bitmap ConvertToGrayscale(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            Bitmap resultImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int grayValue = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    Color grayPixel = Color.FromArgb(grayValue, grayValue, grayValue);
                    resultImage.SetPixel(x, y, grayPixel);
                }
            }

            return resultImage;
        }
        static Bitmap ConvertToBinary(Bitmap image, int threshold)
        {
            int width = image.Width;
            int height = image.Height;
            Bitmap resultImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int grayValue = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    int binaryValue = grayValue < threshold ? 0 : 255;
                    Color binaryPixel = Color.FromArgb(binaryValue, binaryValue, binaryValue);
                    resultImage.SetPixel(x, y, binaryPixel);
                }
            }

            return resultImage;
        }
        static Bitmap CropImage(Bitmap image, int x, int y, int width, int height)
        {
            Rectangle cropArea = new Rectangle(x, y, width, height);
            Bitmap croppedImage = image.Clone(cropArea, image.PixelFormat);
            return croppedImage;
        }
        static Bitmap ApplyDilation(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            Bitmap resultImage = new Bitmap(width, height);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int maxPixel = 0;
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            int pixel = image.GetPixel(x + i, y + j).R;
                            if (pixel > maxPixel)
                            {
                                maxPixel = pixel;
                            }
                        }
                    }
                    resultImage.SetPixel(x, y, Color.FromArgb(maxPixel, maxPixel, maxPixel));
                }
            }

            return resultImage;
        }
        static Bitmap ApplyErosion(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            Bitmap resultImage = new Bitmap(width, height);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int minPixel = 255;
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            int pixel = image.GetPixel(x + i, y + j).R;
                            if (pixel < minPixel)
                            {
                                minPixel = pixel;
                            }
                        }
                    }
                    resultImage.SetPixel(x, y, Color.FromArgb(minPixel, minPixel, minPixel));
                }
            }

            return resultImage;
        }
        static Bitmap ApplyAdaptiveThreshold(Bitmap image, int blockWidth, int blockHeight)
        {
            int width = image.Width;
            int height = image.Height;
            Bitmap resultImage = new Bitmap(width, height);
            int[,] grayImage = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int grayValue = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    grayImage[x, y] = grayValue;
                }
            }

            for (int y = 0; y < height; y += blockHeight)
            {
                for (int x = 0; x < width; x += blockWidth)
                {
                    ApplyOtsuThresholdToBlock(grayImage, resultImage, x, y, blockWidth, blockHeight);
                }
            }

            return resultImage;
        }

        static void ApplyOtsuThresholdToBlock(int[,] grayImage, Bitmap resultImage, int startX, int startY, int blockWidth, int blockHeight)
        {
            int width = resultImage.Width;
            int height = resultImage.Height;

            int endX = Math.Min(startX + blockWidth, width);
            int endY = Math.Min(startY + blockHeight, height);

            int[] histogram = new int[256];
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    histogram[grayImage[x, y]]++;
                }
            }
            int total = (endX - startX) * (endY - startY);
            float sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += i * histogram[i];
            }

            float sumB = 0;
            int wB = 0;
            int wF = 0;
            float varMax = 0;
            int threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];
                if (wB == 0) continue;
                wF = total - wB;
                if (wF == 0) break;

                sumB += (float)(i * histogram[i]);
                float mB = sumB / wB;
                float mF = (sum - sumB) / wF;

                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = i;
                }
            }

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int binaryValue = grayImage[x, y] < threshold ? 0 : 255;
                    Color binaryPixel = Color.FromArgb(binaryValue, binaryValue, binaryValue);
                    resultImage.SetPixel(x, y, binaryPixel);
                }
            }
        }
        static Bitmap ApplyGaussianBlur(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            Bitmap resultImage = new Bitmap(width, height);

            double[,] gaussianBlurFilter = {
            { 1, 2, 1 },
            { 2, 4, 2 },
            { 1, 2, 1 }
        };

            double filterSum = 16.0; 
            int filterSize = 3;
            int filterOffset = filterSize / 2;

            for (int y = filterOffset; y < height - filterOffset; y++)
            {
                for (int x = filterOffset; x < width - filterOffset; x++)
                {
                    double rSum = 0.0;
                    double gSum = 0.0;
                    double bSum = 0.0;

                    for (int filterY = 0; filterY < filterSize; filterY++)
                    {
                        for (int filterX = 0; filterX < filterSize; filterX++)
                        {
                            int imageX = (x - filterOffset + filterX);
                            int imageY = (y - filterOffset + filterY);

                            Color pixel = image.GetPixel(imageX, imageY);

                            rSum += pixel.R * gaussianBlurFilter[filterY, filterX];
                            gSum += pixel.G * gaussianBlurFilter[filterY, filterX];
                            bSum += pixel.B * gaussianBlurFilter[filterY, filterX];
                        }
                    }
                    int r = (int)(rSum / filterSum);
                    int g = (int)(gSum / filterSum);
                    int b = (int)(bSum / filterSum);

                    Color newPixel = Color.FromArgb(r, g, b);

                    resultImage.SetPixel(x, y, newPixel);
                }
            }

            return resultImage;
        }
        static Bitmap AddSaltAndPepperNoise(Bitmap image, double noiseLevel)
        {
            Random rand = new Random();
            Bitmap noisyImage = new Bitmap(image);

            int width = image.Width;
            int height = image.Height;
            int totalPixels = width * height;
            int numNoisePixels = (int)(totalPixels * noiseLevel);

            for (int i = 0; i < numNoisePixels; i++)
            {
                int x = rand.Next(width);
                int y = rand.Next(height);
                Color noiseColor = rand.Next(2) == 0 ? Color.Black : Color.White;
                noisyImage.SetPixel(x, y, noiseColor);
            }

            return noisyImage;
        }
        static Bitmap ApplyMedianFilter(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            Bitmap resultImage = new Bitmap(width, height);

            int filterSize = 3;
            int filterOffset = (filterSize - 1) / 2;

            for (int y = filterOffset; y < height - filterOffset; y++)
            {
                for (int x = filterOffset; x < width - filterOffset; x++)
                {
                    int[] red = new int[filterSize * filterSize];
                    int[] green = new int[filterSize * filterSize];
                    int[] blue = new int[filterSize * filterSize];

                    int k = 0;
                    for (int fy = -filterOffset; fy <= filterOffset; fy++)
                    {
                        for (int fx = -filterOffset; fx <= filterOffset; fx++)
                        {
                            Color pixel = image.GetPixel(x + fx, y + fy);

                            red[k] = pixel.R;
                            green[k] = pixel.G;
                            blue[k] = pixel.B;
                            k++;
                        }
                    }

                    Array.Sort(red);
                    Array.Sort(green);
                    Array.Sort(blue);

                    int medianIndex = red.Length / 2;
                    resultImage.SetPixel(x, y, Color.FromArgb(red[medianIndex], green[medianIndex], blue[medianIndex]));
                }
            }

            return resultImage;
        }
        static int[] CalculateGrayHistogram(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            int[] histogram = new int[256];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int grayValue = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    histogram[grayValue]++;
                }
            }

            return histogram;
        }

        private void ShowHistogram(int[] histogram)
        {
            chartHistogram.Series.Clear();
            chartHistogram.ChartAreas.Clear();

            ChartArea chartArea = new ChartArea();
            chartHistogram.ChartAreas.Add(chartArea);

            Series series = new Series
            {
                Name = "Histogram",
                Color = Color.Black,
                ChartType = SeriesChartType.Column
            };

            chartHistogram.Series.Add(series);
            for (int i = 0; i < histogram.Length; i++)
            {
                series.Points.AddXY(i, histogram[i]);
            }

            chartArea.AxisX.Title = "Gray Value";
            chartArea.AxisY.Title = "Frequency";
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 255;
            chartArea.AxisY.Minimum = 0;
        }
        
        private Bitmap ResizeImage(Bitmap img, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.DrawImage(img, 0, 0, width, height);
            }
            return resizedImage;
        }

        private Bitmap AddImages(Bitmap img1, Bitmap img2)
        {
            int width = img1.Width;
            int height = img1.Height;
            Bitmap resultImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel1 = img1.GetPixel(x, y);
                    Color pixel2 = img2.GetPixel(x, y);
                    int newR = Math.Min(255, pixel1.R + pixel2.R);
                    int newG = Math.Min(255, pixel1.G + pixel2.G);
                    int newB = Math.Min(255, pixel1.B + pixel2.B);
                    Color newColor = Color.FromArgb(newR, newG, newB);
                    resultImage.SetPixel(x, y, newColor);
                }
            }

            return resultImage;
        }

        private Bitmap MultiplyImages(Bitmap img1, Bitmap img2)
        {
            int width = img1.Width;
            int height = img1.Height;
            Bitmap resultImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel1 = img1.GetPixel(x, y);
                    Color pixel2 = img2.GetPixel(x, y);
                    int newR = Math.Min(255, pixel1.R * pixel2.R / 255);
                    int newG = Math.Min(255, pixel1.G * pixel2.G / 255);
                    int newB = Math.Min(255, pixel1.B * pixel2.B / 255);
                    Color newColor = Color.FromArgb(newR, newG, newB);
                    resultImage.SetPixel(x, y, newColor);
                }
            }

            return resultImage;
        }

        private void btnSelectImage2_Click(object sender, EventArgs e)
        {
            Bitmap image2;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string imagePath2 = openFileDialog.FileName;
                    Bitmap originalImage2 = new Bitmap(imagePath2);
                    image2 = ResizeImage(originalImage2, selectedBitmap.Width, selectedBitmap.Height);
                    Bitmap resultImage = AddImages(selectedBitmap, image2);
                    pictureBox2.Image = resultImage;
                }
            }
        }
        static Bitmap ApplyMeanFilter(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            Bitmap resultImage = new Bitmap(width, height);

            int[,] filterMatrix =
            {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

            int filterSize = 3;
            int filterOffset = (filterSize - 1) / 2;

            for (int y = filterOffset; y < height - filterOffset; y++)
            {
                for (int x = filterOffset; x < width - filterOffset; x++)
                {
                    int red = 0, green = 0, blue = 0;

                    for (int fy = -filterOffset; fy <= filterOffset; fy++)
                    {
                        for (int fx = -filterOffset; fx <= filterOffset; fx++)
                        {
                            Color pixel = image.GetPixel(x + fx, y + fy);

                            red += pixel.R * filterMatrix[fy + filterOffset, fx + filterOffset];
                            green += pixel.G * filterMatrix[fy + filterOffset, fx + filterOffset];
                            blue += pixel.B * filterMatrix[fy + filterOffset, fx + filterOffset];
                        }
                    }

                    red /= 9;
                    green /= 9;
                    blue /= 9;

                    resultImage.SetPixel(x, y, Color.FromArgb(red, green, blue));
                }
            }

            return resultImage;
        }
        private Bitmap RotateImage(Bitmap image, double angle)
        {
            int width = image.Width;
            int height = image.Height;
            double radians = angle * Math.PI / 180;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            int newWidth = (int)(Math.Abs(width * cos) + Math.Abs(height * sin));
            int newHeight = (int)(Math.Abs(width * sin) + Math.Abs(height * cos));
            Bitmap rotatedImage = new Bitmap(newWidth, newHeight);

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    double newX = (x - newWidth / 2.0) * cos + (y - newHeight / 2.0) * sin + width / 2.0;
                    double newY = -(x - newWidth / 2.0) * sin + (y - newHeight / 2.0) * cos + height / 2.0;

                    if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                    {
                        rotatedImage.SetPixel(x, y, BilinearInterpolation(image, newX, newY));
                    }
                    else
                    {
                        rotatedImage.SetPixel(x, y, Color.Transparent);
                    }
                }
            }

            return rotatedImage;
        }

        private Bitmap ResizeImage(Bitmap image, double widthRatio, double heightRatio)
        {
            int newWidth = (int)(image.Width * widthRatio);
            int newHeight = (int)(image.Width * heightRatio);
            Bitmap resizedImage = new Bitmap(newWidth, newHeight);

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    double srcX = x / widthRatio;
                    double srcY = y / heightRatio;
                    resizedImage.SetPixel(x, y, BilinearInterpolation(image, srcX, srcY));
                }
            }

            return resizedImage;
        }
        static Bitmap ApplyBlur(Bitmap image, int blurAmount)
        {
            Bitmap blurredImage = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color blurredPixel = CalculateBlurredPixel(image, x, y, blurAmount);
                    blurredImage.SetPixel(x, y, blurredPixel);
                }
            }

            return blurredImage;
        }

        static Color CalculateBlurredPixel(Bitmap image, int x, int y, int blurAmount)
        {
            int red = 0, green = 0, blue = 0;
            int totalPixels = 0;

            for (int offsetX = -blurAmount; offsetX <= blurAmount; offsetX++)
            {
                for (int offsetY = -blurAmount; offsetY <= blurAmount; offsetY++)
                {
                    int newX = x + offsetX;
                    int newY = y + offsetY;

                    if (newX >= 0 && newX < image.Width && newY >= 0 && newY < image.Height)
                    {
                        Color pixel = image.GetPixel(newX, newY);
                        red += pixel.R;
                        green += pixel.G;
                        blue += pixel.B;
                        totalPixels++;
                    }
                }
            }

            red /= totalPixels;
            green /= totalPixels;
            blue /= totalPixels;

            return Color.FromArgb(red, green, blue);
        }

    private Color BilinearInterpolation(Bitmap image, double x, double y)
        {
            int x1 = (int)x;
            int y1 = (int)y;
            int x2 = x1 + 1;
            int y2 = y1 + 1;

            if (x2 >= image.Width) x2 = image.Width - 1;
            if (y2 >= image.Height) y2 = image.Height - 1;

            Color Q11 = image.GetPixel(x1, y1);
            Color Q21 = image.GetPixel(x2, y1);
            Color Q12 = image.GetPixel(x1, y2);
            Color Q22 = image.GetPixel(x2, y2);

            double xFraction = x - x1;
            double yFraction = y - y1;

            double r1 = (1 - xFraction) * Q11.R + xFraction * Q21.R;
            double r2 = (1 - xFraction) * Q12.R + xFraction * Q22.R;
            double r = (1 - yFraction) * r1 + yFraction * r2;

            double g1 = (1 - xFraction) * Q11.G + xFraction * Q21.G;
            double g2 = (1 - xFraction) * Q12.G + xFraction * Q22.G;
            double g = (1 - yFraction) * g1 + yFraction * g2;

            double b1 = (1 - xFraction) * Q11.B + xFraction * Q21.B;
            double b2 = (1 - xFraction) * Q12.B + xFraction * Q22.B;
            double b = (1 - yFraction) * b1 + yFraction * b2;

            return Color.FromArgb((int)r, (int)g, (int)b);
        }
        private Bitmap ContrastStretch(Bitmap image, int min, int max)
        {
            Bitmap stretchedImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int r = StretchValue(pixelColor.R, min, max);
                    int g = StretchValue(pixelColor.G, min, max);
                    int b = StretchValue(pixelColor.B, min, max);
                    stretchedImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return stretchedImage;
        }

        private int StretchValue(int value, int min, int max)
        {
            int newValue = (value - min) * 255 / (max - min);
            if (newValue < 0) return 0;
            if (newValue > 255) return 255;
            return newValue;
        }

        private Bitmap HistogramEqualize(Bitmap image)
        {
            int[] histogram = new int[256];
            int[] cdf = new int[256];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int intensity = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);
                    histogram[intensity]++;
                }
            }

            cdf[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            Bitmap equalizedImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int r = EqualizeValue(pixelColor.R, cdf, image.Width * image.Height);
                    int g = EqualizeValue(pixelColor.G, cdf, image.Width * image.Height);
                    int b = EqualizeValue(pixelColor.B, cdf, image.Width * image.Height);
                    equalizedImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return equalizedImage;
        }
        private void DisplayHistogram(Bitmap image)
        {
            int[] histogram = new int[256];
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int intensity = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);
                    histogram[intensity]++;
                }
            }

            chartHistogram.Series.Clear();
            Series series = new Series
            {
                Name = "Intensity",
                Color = Color.Black,
                ChartType = SeriesChartType.Column
            };

            for (int i = 0; i < histogram.Length; i++)
            {
                series.Points.AddXY(i, histogram[i]);
            }

            chartHistogram.Series.Add(series);
            chartHistogram.Invalidate();
        }
        private int EqualizeValue(int value, int[] cdf, int totalPixels)
        {
            return (cdf[value] - cdf[0]) * 255 / (totalPixels - cdf[0]);
        }
        static void SaveImage(Bitmap image, string filePath)
        {
            image.Save(filePath);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = ApplySobelFilter(selectedBitmap);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int deger = (int)numericUpDown1.Value;
            pictureBox2.Image = parlaklık(selectedBitmap,deger);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = ConvertToGrayscale(selectedBitmap);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int deger = (int)numericUpDown2.Value;
            pictureBox2.Image = ConvertToBinary(selectedBitmap, deger);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int width = (int)numericUpDown9.Value;
            int height = (int)numericUpDown10.Value;
            int x = (int)numericUpDown12.Value;
            int y = (int)numericUpDown11.Value;
            pictureBox2.Image = CropImage(selectedBitmap, x, y, width, height);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Bitmap binaryImage = ConvertToBinary(selectedBitmap, 128);
            pictureBox2.Image = ApplyDilation(binaryImage);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Bitmap binaryImage = ConvertToBinary(selectedBitmap, 128);
            pictureBox2.Image = ApplyErosion(binaryImage);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Bitmap binaryImage = ConvertToBinary(selectedBitmap, 128);
            Bitmap erodedImage = ApplyErosion(binaryImage);
            pictureBox2.Image = ApplyDilation(erodedImage);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Bitmap binaryImage = ConvertToBinary(selectedBitmap, 128);
            Bitmap erodedImage = ApplyDilation(binaryImage);
            pictureBox2.Image = ApplyErosion(erodedImage);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            int width = (int)numericUpDown6.Value;
            int height = (int)numericUpDown7.Value;
            pictureBox2.Image = ApplyAdaptiveThreshold(selectedBitmap, width, height);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = ApplyGaussianBlur(selectedBitmap);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            double noiseLevel = (double)numericUpDown8.Value;
            pictureBox2.Image = AddSaltAndPepperNoise(selectedBitmap, noiseLevel/100);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string saveFilePath = saveFileDialog.FileName;
                SaveImage((Bitmap)pictureBox2.Image, saveFilePath);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = ApplyMedianFilter(selectedBitmap);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            int[] histogram = CalculateGrayHistogram(selectedBitmap);
            ShowHistogram(histogram);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            int min = (int)numericUpDown4.Value;
            int max = (int)numericUpDown5.Value;
            Bitmap stretchedImage = ContrastStretch(selectedBitmap, min, max);
            pictureBox2.Image = stretchedImage;
            DisplayHistogram(stretchedImage);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            Bitmap stretchedImage = HistogramEqualize(selectedBitmap);
            pictureBox2.Image = stretchedImage;
            DisplayHistogram(stretchedImage);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            Bitmap image2;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string imagePath2 = openFileDialog.FileName;
                    Bitmap originalImage2 = new Bitmap(imagePath2);

                    image2 = ResizeImage(originalImage2, selectedBitmap.Width, selectedBitmap.Height);
                    Bitmap resultImage = MultiplyImages(selectedBitmap, image2);
                    pictureBox2.Image = resultImage;
                }
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = ApplyMeanFilter(selectedBitmap);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            int deger = (int)numericUpDown3.Value;
            pictureBox2.Image = RotateImage(selectedBitmap, deger);
        }

        private void button24_Click(object sender, EventArgs e)
        {
            int h = (int)numericUpDown13.Value;
            int w = (int)numericUpDown14.Value;
            pictureBox2.Image = ResizeImage(selectedBitmap, w, h);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            int blurAmount = trackBar1.Value;
            pictureBox2.Image = ApplyBlur(selectedBitmap, blurAmount);
        }
    }
}
