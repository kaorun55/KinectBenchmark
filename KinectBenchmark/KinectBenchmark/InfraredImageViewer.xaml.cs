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
    /// InfraredImageViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class InfraredImageViewer : UserControl
    {
        FpsCounter counter = new FpsCounter();

        // Kinect SDK
        KinectSensor kinect;

        InfraredFrameReader infraredFrameReader;
        FrameDescription infraredFrameDesc;

        // 表示用
        WriteableBitmap infraredBitmap;
        int infraredStride;
        Int32Rect infraredRect;
        ushort[] infraredBuffer;

        public InfraredImageViewer()
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

                // 赤外線画像の情報を取得する
                infraredFrameDesc = kinect.InfraredFrameSource.FrameDescription;

                // 赤外線リーダーを開く
                infraredFrameReader = kinect.InfraredFrameSource.OpenReader();
                infraredFrameReader.FrameArrived += infraredFrameReader_FrameArrived;

                // 表示のためのビットマップに必要なものを作成
                infraredBuffer = new ushort[infraredFrameDesc.LengthInPixels];
                infraredBitmap = new WriteableBitmap(
                    infraredFrameDesc.Width, infraredFrameDesc.Height,
                    96, 96, PixelFormats.Gray16, null);
                infraredRect = new Int32Rect(0, 0,
                    infraredFrameDesc.Width, infraredFrameDesc.Height);
                infraredStride = infraredFrameDesc.Width *
                                 (int)infraredFrameDesc.BytesPerPixel;

                ImageInfrared.Source = infraredBitmap;
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
            if (infraredFrameReader != null)
            {
                infraredFrameReader.FrameArrived -= infraredFrameReader_FrameArrived;
                infraredFrameReader.Dispose();
                infraredFrameReader = null;
            }

            ImageInfrared.Source = null;
            TextFps.Text = "0";
        }

        void infraredFrameReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            // カラーフレームを取得する
            using (var infraredFrame = e.FrameReference.AcquireFrame())
            {
                if (infraredFrame == null)
                {
                    return;
                }

                // 赤外線画像データを取得する
                infraredFrame.CopyFrameDataToArray(infraredBuffer);
                infraredBitmap.WritePixels(infraredRect, infraredBuffer, infraredStride, 0);

                // フレームレート更新
                TextFps.Text = counter.Update().ToString();
            }
        }
    }
}
