using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace WriteableBitmapDemo
{
    class Program
    {
        static WriteableBitmap writeableBitmap;
        static Window w;
        static Image i;

        [STAThread]
        static void Main(string[] args)
        {
            i = new Image();
            RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(i, EdgeMode.Aliased);

            w = new Window();
            w.Content = i;
            w.Show();

            writeableBitmap = new WriteableBitmap(
                (int)w.ActualWidth,
                (int)w.ActualHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            i.Source = writeableBitmap;

            i.Stretch = Stretch.None;
            i.HorizontalAlignment = HorizontalAlignment.Left;
            i.VerticalAlignment = VerticalAlignment.Top;

            i.MouseMove += new MouseEventHandler(i_MouseMove);
            i.MouseLeftButtonDown +=
                new MouseButtonEventHandler(i_MouseLeftButtonDown);
            i.MouseRightButtonDown +=
                new MouseButtonEventHandler(i_MouseRightButtonDown);

            w.MouseWheel += new MouseWheelEventHandler(w_MouseWheel);

DrawHeatMap();

            Application app = new Application();
            app.Run();
        }

        // The DrawPixel method updates the WriteableBitmap by using
        // unsafe code to write a pixel into the back buffer.
        static void DrawPixel(MouseEventArgs e)
        {
            int column = (int)e.GetPosition(i).X;
            int row = (int)e.GetPosition(i).Y;

            try
            {
                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                    // Find the address of the pixel to draw.
                    pBackBuffer += row * writeableBitmap.BackBufferStride;
                    pBackBuffer += column * 4;

                    // Compute the pixel's color.
                    int color_data = 255 << 16; // R
                    color_data |= 128 << 8;   // G
                    color_data |= 255 << 0;   // B

                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = color_data;
                }

                // Specify the area of the bitmap that changed.
                writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
            }
        }

        static void ErasePixel(MouseEventArgs e)
        {
            byte[] ColorData = { 0, 0, 0, 0 }; // B G R

            Int32Rect rect = new Int32Rect(
                    (int)(e.GetPosition(i).X),
                    (int)(e.GetPosition(i).Y),
                    1,
                    1);

            writeableBitmap.WritePixels(rect, ColorData, 4, 0);
        }

        static void i_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ErasePixel(e);
        }

        static void i_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawPixel(e);
        }

        static void i_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DrawPixel(e);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                ErasePixel(e);
            }
        }

        static void w_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Media.Matrix m = i.RenderTransform.Value;

            if (e.Delta > 0)
            {
                m.ScaleAt(
                    1.5,
                    1.5,
                    e.GetPosition(w).X,
                    e.GetPosition(w).Y);
            }
            else
            {
                m.ScaleAt(
                    1.0 / 1.5,
                    1.0 / 1.5,
                    e.GetPosition(w).X,
                    e.GetPosition(w).Y);
            }

            i.RenderTransform = new MatrixTransform(m);
        }

        /* ====================================================================== HEATMAP PROTOTYPE */

        private static byte[][] _rgbTable;
        static void DrawHeatMap()
        {
            _rgbTable = InitializeColorMap(1440);
            DrawHeatMapColorScale(1000, 50, _rgbTable);
        }


        private static byte[][] InitializeColorMap(int length)
        {
            var colors = new byte[length][];
            var blackToRedLength = colors.Length / 2;
            var redPoint = blackToRedLength;
            var redToYellowLength = colors.Length / 4;
            var yellowPoint = redToYellowLength + redPoint;
            var restLength = colors.Length - blackToRedLength - redToYellowLength;

            for (var x = 0; x < colors.Length; x++)
            {
                colors[x] = new byte[3]; // 0:R, 1:G, 2:B
                if (x < redPoint)
                {
                    var xc = x;
                    var v = Convert.ToByte(xc * 256 / blackToRedLength);
                    colors[x][0] = v;
                    colors[x][1] = 0;
                    colors[x][2] = 0;
                }
                else if(x<yellowPoint)
                {
                    var xc= x - redPoint;
                    var v = Convert.ToByte(xc * 256 / redToYellowLength);
                    colors[x][0] = 255;
                    colors[x][1] = v;
                    colors[x][2] = 0;
                }
                else
                {
                    var xc = x - yellowPoint;
                    var v = Convert.ToByte(xc * 256 / restLength);
                    colors[x][0] = 255;
                    colors[x][1] = 255;
                    colors[x][2] = v;
                }
            }

            return colors;
        }
        private static void DrawHeatMapColorScale(int width, int height, byte[][] rgbTable)
        {
            for (int x = 0; x < width; x++)
            {
                var i = x * rgbTable.Length / width;
                DrawVerticalLine(x, 10, height, rgbTable[i]);
            }
        }

        static void DrawVerticalLine(int x, int y, int length, byte[] rgb)
        {
            int column = x;
            int row = y;

            try
            {
                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                unsafe
                {

                    // Compute the pixel's color.
                    int color = rgb[0] << 16; // R
                    color |= rgb[1] << 8;   // G
                    color |= rgb[2] << 0;   // B

                    for (int i = 0; i < length; i++)
                    {
                        // Get a pointer to the back buffer.
                        IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                        // Find the address of the pixel to draw.
                        pBackBuffer += (row + i) * writeableBitmap.BackBufferStride;
                        pBackBuffer += column * 4;

                        // Assign the color data to the pixel.
                        *((int*)pBackBuffer) = color;
                    }

                }

                // Specify the area of the bitmap that changed.
                writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, length));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
            }
        }
    }
}