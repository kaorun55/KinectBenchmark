using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectBenchmark
{
    /// <summary>
    /// ColorImageViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorImageViewer : UserControl
    {
        FpsCounter counter = new FpsCounter();

        // Kinect SDK
        KinectSensor kinect;

        ColorFrameReader colorFrameReader;
        FrameDescription colorFrameDesc;

        ColorImageFormat colorFormat = ColorImageFormat.Bgra;

        // WPF
        WriteableBitmap colorBitmap;
        byte[] colorBuffer;
        int colorStride;
        Int32Rect colorRect;

        public ColorImageViewer()
        {
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsProcess.IsChecked == true)
            {
                Initialize();
            }
            else
            {
                Uninitialize();
            }
        }

        void Initialize()
        {
            try
            {
                Uninitialize();

                // Kinectを開く
                kinect = KinectSensor.GetDefault();
                if (!kinect.IsOpen)
                {
                    kinect.Open();
                }

                // カラー画像の情報を作成する(BGRAフォーマット)
                colorFrameDesc = kinect.ColorFrameSource.CreateFrameDescription(
                                                        colorFormat);

                // カラーリーダーを開く
                colorFrameReader = kinect.ColorFrameSource.OpenReader();
                colorFrameReader.FrameArrived += colorFrameReader_FrameArrived;

                // カラー用のビットマップを作成する
                colorBitmap = new WriteableBitmap(
                                    colorFrameDesc.Width, colorFrameDesc.Height,
                                    96, 96, PixelFormats.Bgra32, null);
                colorStride = colorFrameDesc.Width * (int)colorFrameDesc.BytesPerPixel;
                colorRect = new Int32Rect(0, 0,
                                    colorFrameDesc.Width, colorFrameDesc.Height);
                colorBuffer = new byte[colorStride * colorFrameDesc.Height];
                ImageColor.Source = colorBitmap;
            }
            catch (Exception ex)
            {
                Uninitialize();
                MessageBox.Show(ex.Message);
            }
        }

        void Uninitialize()
        {
            // 終了処理
            if (colorFrameReader != null)
            {
                colorFrameReader.FrameArrived -= colorFrameReader_FrameArrived;
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }

            ImageColor.Source = null;
            TextFps.Text = "0";
        }

        void colorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // カラーフレームを取得する
            using (var colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }

                // BGRAデータを取得する
                colorFrame.CopyConvertedFrameDataToArray( colorBuffer, colorFormat);
                colorBitmap.WritePixels(colorRect, colorBuffer, colorStride, 0);

                // フレームレート更新
                TextFps.Text = counter.Update().ToString();
            }
        }
    }
}
