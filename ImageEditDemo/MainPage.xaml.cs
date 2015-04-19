using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Imaging;
using Silverlight.Samples;
using Img2DocxSlvrt;
using System.Diagnostics;
using System.Windows.Browser;

namespace ImageEditDemo
{
    public partial class MainPage : UserControl
    {
        private WriteableBitmap _bitmap;   
        private bool _down = false;
        private double _x, _y;
        private Img2Docx _i2dx = new Img2Docx();
        private MainViewModel _vm = new MainViewModel();


        public MainPage()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        private void btnAddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp;*.tif)|*.jpg;*.jpeg;*.png;*.bmp;*.tif";
            ofd.FilterIndex = 1;

            if ((bool)ofd.ShowDialog())
            {
                using (Stream stream = ofd.File.OpenRead())
                {
                   //  BitmapImage bi = new BitmapImage();

                    //  bi.SetSource(stream);                  
                    //byte[] buffer = new byte[2048];
                    //int count;
                    //while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                    //{
                    //    int t = 4;
                    //    t = 4;
                    //}
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    ImageItem ii = new ImageItem(ofd.File.Name, buffer);
                      _vm.Items.Add(ii);
                }
                //using (Stream stream = ofd.File.OpenRead())
                //{
                //    BitmapImage bi = new BitmapImage();
                //    bi.SetSource(stream);

                //    // The following size limitation comes from the EditableImage class
                //    // used for PNG encoding
                //    if (bi.PixelWidth > 2047 || bi.PixelHeight > 2047)
                //    {
                //        MessageBox.Show("Image size cannot exceed 2047x2047");
                //    }
                //    else
                //    {
                //        _bitmap = new WriteableBitmap(bi);
                //        Photo.Source = _bitmap;
                //        ZoomStoryboard.Begin();
                //    }
                //}
            }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Word Document (*.docx)|*.docx";
            sfd.DefaultExt = ".docx";
            sfd.FilterIndex = 1;
            if ((bool)sfd.ShowDialog())
            {
                using (Stream strm = sfd.OpenFile())
                {
                    _i2dx.Reset();
                    foreach (Item item in _vm.Items)
                    {
                        if (item is ImageItem)
                        {
                            ImageItem ii = item as ImageItem;
                            _i2dx.AddImage(ii.Filename, ii.Buffer);
                        }
                    }
                    _i2dx.Save(strm);

                    HtmlPage.Window.Alert("Generation complete");
                }
            }
            //if (_bitmap != null)
            //{
            //    SaveFileDialog sfd = new SaveFileDialog();
            //    sfd.Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*";
            //    sfd.DefaultExt = ".png";
            //    sfd.FilterIndex = 1;

            //    if ((bool)sfd.ShowDialog())
            //    {
            //        using (Stream fs = sfd.OpenFile())
            //        {
            //            int width = _bitmap.PixelWidth;
            //            int height = _bitmap.PixelHeight;

            //            // Create an EditableImage for encoding
            //            EditableImage ei = new EditableImage(width, height);

            //            // Transfer pixels from the WriteableBitmap to the EditableImage
            //            for (int i = 0; i < height; i++)
            //            {
            //                for (int j = 0; j < width; j++)
            //                {
            //                    int pixel = _bitmap.Pixels[(i * width) + j];
            //                    ei.SetPixel(j, i,
            //                                (byte)((pixel >> 16) & 0xFF),   // R
            //                                (byte)((pixel >> 8) & 0xFF),    // G
            //                                (byte)(pixel & 0xFF),           // B
            //                                (byte)((pixel >> 24) & 0xFF)    // A
            //                    );
            //                }
            //            }

            //            // Generate a PNG stream from the EditableImage and convert to byte[]
            //            Stream png = ei.GetStream();
            //            int len = (int)png.Length;
            //            byte[] bytes = new byte[len];
            //            png.Read(bytes, 0, len);

            //            // Write the PNG to disk
            //            fs.Write(bytes, 0, len);
            //        }
            //    }
            //}
        }

        private void Photo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RectCanvas.Width = Photo.ActualWidth;
            RectCanvas.Height = Photo.ActualHeight;

            _x = e.GetPosition((FrameworkElement)sender).X;
            _y = e.GetPosition((FrameworkElement)sender).Y;

            ((FrameworkElement)sender).CaptureMouse();
            _down = true;
        }

        private void Photo_MouseMove(object sender, MouseEventArgs e)
        {
            if (_down)
            {
                double x = e.GetPosition((FrameworkElement)sender).X;
                double y = e.GetPosition((FrameworkElement)sender).Y;

                Rect.SetValue(Canvas.LeftProperty, Math.Min(x, _x));
                Rect.SetValue(Canvas.TopProperty, Math.Min(y, _y));

                Rect.Width = Math.Abs(x - _x);
                Rect.Height = Math.Abs(y - _y);

                if (Rect.Visibility != Visibility.Visible)
                    Rect.Visibility = Visibility.Visible;
            }
        }

        private void Photo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rect.Visibility = Visibility.Collapsed;
            ((FrameworkElement)sender).ReleaseMouseCapture();
            _down = false;

            double x = e.GetPosition((FrameworkElement)sender).X;
            double y = e.GetPosition((FrameworkElement)sender).Y;

            // Do nothing if no selection
            if (x == _x || y == _y)
                return;

            // Order coordinates or selection region
            int x1 = (int)Math.Min(_x, x);
            int x2 = (int)Math.Max(_x, x);
            int y1 = (int)Math.Min(_y, y);
            int y2 = (int)Math.Max(_y, y);

            // Constrain coordinates of selection region
            x1 = (int)Math.Max(0, x1);
            x2 = (int)Math.Min(Photo.ActualWidth, x2);
            y1 = (int)Math.Max(0, y1);
            y2 = (int)Math.Min(Photo.ActualHeight, y2);

            // Translate pixel coordinates to WriteableBitmap coordinates
            double wratio = _bitmap.PixelWidth / Photo.ActualWidth;
            double hratio = _bitmap.PixelHeight / Photo.ActualHeight;

            x1 = (int)(x1 * wratio);
            x2 = (int)(x2 * wratio);
            y1 = (int)(y1 * hratio);
            y2 = (int)(y2 * hratio);

            // Remove red-eye
            RemoveRedEye(x1, x2, y1, y2);
            Photo.Source = _bitmap;
            btnGenerate.IsEnabled = true;
        }

        private void RemoveRedEye(int x1, int x2, int y1, int y2)
        {
            int width = _bitmap.PixelWidth;

            for (int i = y1; i < y2; i++)
            {
                for (int j = x1; j < x2; j++)
                {
                    int index = (i * width) + j;
                    int pixel = _bitmap.Pixels[index];

                    int a = (int)((pixel & 0xFF000000) >> 24);
                    int r = (pixel & 0x00FF0000) >> 16;
                    int g = (pixel & 0x0000FF00) >> 8;
                    int b = (pixel & 0x000000FF);

                    if (r > (g + b))
                        r = (g + b) / 2;
                    
                    _bitmap.Pixels[index] = (a << 24) + (r << 16) + (g << 8) + b;
                }
            }

            _bitmap.Invalidate();
        }

        private void lbDocItems_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (HtmlPage.Window.Confirm("Are you sure to delete this item?") == true)
                    _vm.Delete();
            }
        }
   }
}
