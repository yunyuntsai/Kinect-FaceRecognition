using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class Utils
    {
        static public class Constants

        {

            public const bool STREAM_ANALYSTIC = true;

            public const bool WHITE_BOARDING = false;

            public const bool SAVE_TO_CLOUD_DRIVE = false;



            public const int QRIMG_SIZE = 800;

            public const int MAX_BG_NUM = 13;

            public const int MAX_FACE_NUM = 4;

            public const int FIGURE_WIDTH = 1858;

            public const int FIGURE_HEIGHT = 2480;

            public const float resizeRatio = 0.3f;

            public const int BG_WIDTH = 1280;

            public const int BG_HEIGHT = 720;



            public const string filename = "account.csv";

            public static Point[] POSITION_OFFSET = new Point[]

     {

            new Point(233,100),

            new Point(520,100),

            new Point(-100,100),

            new Point(820,100)

     };






        }
      
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)

        {

            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));



            using (MemoryStream outStream = new MemoryStream())

            {

                BitmapEncoder enc = new BmpBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(bitmapImage));

                enc.Save(outStream);

                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);



                return new Bitmap(bitmap);

            }

        }



        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)

        {

            using (var memory = new MemoryStream())

            {

                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);

                memory.Position = 0;



                var bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();

                bitmapImage.StreamSource = memory;

                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                bitmapImage.EndInit();



                return bitmapImage;

            }

        }



        public static byte[] BitmapToByteArray(Bitmap bitmap)

        {

            BitmapData bmpdata = null;

            try

            {

                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

                int numbytes = bmpdata.Stride * bitmap.Height;

                byte[] bytedata = new byte[numbytes];

                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;

            }

            finally

            {

                if (bmpdata != null)

                    bitmap.UnlockBits(bmpdata);

            }

        }

    }
    }
