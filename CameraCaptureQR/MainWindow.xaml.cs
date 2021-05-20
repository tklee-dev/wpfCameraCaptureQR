using AForge.Video.DirectShow;
using log4net;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ZXing;
using BarcodeReader = ZXing.Presentation.BarcodeReader;

namespace CameraCaptureQR
{
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
        }

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        DispatcherTimer dt_qr = new DispatcherTimer();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (filterInfoCollection.Count == 0)
                MessageBox.Show("장비에서 카메라를 찾지 못했습니다");
            else
            {
                foreach (FilterInfo filterInfo in filterInfoCollection)
                    cboDevice.Items.Add(filterInfo.Name);
                cboDevice.SelectedIndex = 0;


                videoCaptureDevice = new VideoCaptureDevice();

                videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cboDevice.SelectedIndex].MonikerString);
                videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
                videoCaptureDevice.VideoResolution = videoCaptureDevice.VideoCapabilities[6];
                videoCaptureDevice.Start();

                dt_qr = new DispatcherTimer();
                dt_qr.Interval = new TimeSpan(500);
                dt_qr.Tick += Dt_qr_Tick;
                dt_qr.Start();
            }
        }

        private void Dt_qr_Tick(object sender, EventArgs e)
        {
            //BarcodeReader object in zxing and also zxing.presentation(bitmapSource용)
            BarcodeReader barcodeReader = new BarcodeReader();
            Result result = null;

            if (frameHolder.Source != null)
            {
                result = barcodeReader.Decode((BitmapSource)frameHolder.Source);
                if (result != null)
                    tbQRCode.Text += result.ToString();
            }
        }


        private void VideoCaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameHolder.Source = bi;
                }
                ));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
            {
                dt_qr.Stop();
                videoCaptureDevice.Stop();
            }


        }
    }
}