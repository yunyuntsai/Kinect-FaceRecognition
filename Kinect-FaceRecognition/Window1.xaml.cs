using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
using System.Windows.Shapes;
using static Microsoft.Samples.Kinect.ColorBasics.Utils;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
      
        public Window1(string imagePath)
        {
            InitializeComponent();

            Console.WriteLine(imagePath);
               
            Loadfigure_image(imagePath);
               
        }
        private void Loadfigure_image(string path)
        {
         
            using (var bitmap = new Bitmap(Constants.BG_WIDTH, Constants.BG_HEIGHT))

            {

                using (var canvas = Graphics.FromImage(bitmap))

                {

                    //String Fi_Photos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                    //String fi_path = System.IO.Path.Combine(Fi_Photos, "Body" + Facename_Pool[i] + ".png");

                    System.Drawing.Image temp_body = System.Drawing.Image.FromFile(path);

                    int dx = Constants.POSITION_OFFSET[1].X;

                    int dy = Constants.POSITION_OFFSET[1].Y;

                    //canvas.DrawImage(temp_body, dx, dy, Constants.FIGURE_WIDTH * Constants.resizeRatio, Constants.FIGURE_HEIGHT * Constants.resizeRatio);

                    canvas.DrawImage(temp_body, dx, dy, Constants.FIGURE_WIDTH * Constants.resizeRatio, Constants.FIGURE_HEIGHT * Constants.resizeRatio);

                    canvas.Save();

                    
                    System.Windows.Controls.Image x = new System.Windows.Controls.Image();

                  
                        PhotoShot_Screen1.Source = Utils.Bitmap2BitmapImage(bitmap);
                   
                    // Console.WriteLine(x.Name);



                    //x.Visibility = Visibility.Visible;

                    System.Console.WriteLine("finish fig bitmap");

                    canvas.Dispose();


                }

                bitmap.Dispose();

            }
        }
 
    }
}
