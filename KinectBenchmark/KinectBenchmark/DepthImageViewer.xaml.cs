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
    /// DepthImageViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class DepthImageViewer : UserControl
    {
        FpsCounter counter = new FpsCounter();

        // Kinect SDK
        KinectSensor kinect;

        DepthFrameReader depthFrameReader;
        FrameDescription depthFrameDesc;

        // 表示
        WriteableBitmap depthImage;
        ushort[] depthBuffer;
        byte[] depthBitmapBuffer;
        Int32Rect depthRect;
        int depthStride;

        public DepthImageViewer()
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
                kinect.Open();

                // 表示のためのデータを作成
                depthFrameDesc = kinect.DepthFrameSource.FrameDescription;

                // 表示のためのビットマップに必要なものを作成
                depthImage = new WriteableBitmap(
                    depthFrameDesc.Width, depthFrameDesc.Height,
                    96, 96, PixelFormats.Gray8, null);
                depthBuffer = new ushort[depthFrameDesc.LengthInPixels];
                depthBitmapBuffer = new byte[depthFrameDesc.LengthInPixels];
                depthRect = new Int32Rect(0, 0,
                                        depthFrameDesc.Width, depthFrameDesc.Height);
                depthStride = (int)(depthFrameDesc.Width);

                ImageDepth.Source = depthImage;

                // Depthリーダーを開く
                depthFrameReader = kinect.DepthFrameSource.OpenReader();
                depthFrameReader.FrameArrived += depthFrameReader_FrameArrived;
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
            if (depthFrameReader != null)
            {
                depthFrameReader.FrameArrived -= depthFrameReader_FrameArrived;
                depthFrameReader.Dispose();
                depthFrameReader = null;
            }

            ImageDepth.Source = null;
            TextFps.Text = "0";
        }

        void depthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            using (var depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }

                // Depthデータを取得する
                depthFrame.CopyFrameDataToArray(depthBuffer);

                // 0-8000のデータを255ごとに折り返すようにする(見やすく)
                for (int i = 0; i < depthBuffer.Length; i++)
                {
                    depthBitmapBuffer[i] = (byte)(depthBuffer[i] % 255);
                }

                depthImage.WritePixels(depthRect, depthBitmapBuffer, depthStride, 0);

                // フレームレート更新
                TextFps.Text = counter.Update().ToString();
            }
        }
    }
}
