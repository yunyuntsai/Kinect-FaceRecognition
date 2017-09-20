//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Collections.Generic;
    using Microsoft.ProjectOxford.Face;
    using Microsoft.ProjectOxford.Common;


    using System.Drawing;
    using System.Windows.Controls;
    using System.Text;
    using Microsoft.ProjectOxford.Face.Contract;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using System.Threading;
    using System.Configuration;
    using System.Linq;
    using Newtonsoft.Json;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json.Linq;
    using static Microsoft.Samples.Kinect.ColorBasics.Utils;
    using System.Windows.Threading;



    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        private Bitmap figureBitmap = null;
        private static readonly string FaceApiKey = ConfigurationManager.AppSettings["FaceApiKey"].ToString();
        string deviceConnectionString = ConfigurationManager.AppSettings["DeviceConnectionString"];
        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;
        private CameraSpacePoint[] cameraSpacePoints = null;
        private Body[] bodies = null;
        private BodyFrameReader bodyFrameReader = null;
        private List<float> List = new List<float>();
        private Dictionary<ulong, bool> idDictionary= new Dictionary<ulong , bool>();
        private ulong IdentifyID;
        private string _selectedFile;
        private string _detectedResultsInText;

        private bool _IdentifyCheck = false;
        private bool _detectedBackButton = false;
        private string identify_name = null;
        private string identify_Age = null;
        private string identify_Gender = null;
        private string identify_Emotion = null;
        private Dictionary<string, float> identify_EmotionScoreList = null;

        private string identifyState = "None";
        private string personName = "Who are you?";
        StringBuilder Mount_path = new StringBuilder();
        string[] HeadRandom;
        BitmapImage bg_pool = null;
        BitmapImage[] badge = new BitmapImage[3];
        BitmapImage figureImage = null;
        private bool viewmode = false;
        int time_counter = 0;
         //string deviceConnectionString = ConfigurationManager.AppSettings["DeviceConnectionString"];
        FaceServiceClient fc = new FaceServiceClient("bb8881f7ef5a41daa71cd1857aa746ce", "https://southeastasia.api.cognitive.microsoft.com/face/v1.0");
        string personGroupId = "test";
        private DispatcherTimer count_timer;
        private int countdown;
        private string read_name;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private string path1;
        private bool isTrain = false;
        private string nameText;



        // private readonly IFaceServiceClient faceServiceClient =
        //new FaceServiceClient("703419f55cb9486aa1d76e84d882e46a", "https://westus.api.cognitive.microsoft.com/face/v1.0");
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {

            

            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            this.cameraSpacePoints = new CameraSpacePoint[1];
            this.List = new List<float>();
           
            Load_BgImage();
           
            badge_Screen.Visibility = Visibility.Visible;
            BackGround_Screen.Visibility = Visibility.Visible;
            Figure_Screen.Visibility = Visibility.Collapsed;
            Console.WriteLine("deviceConnectionString={0}\n", deviceConnectionString);

            if (this.kinectSensor != null)
            {
                Console.Write("Kinect Connect\n");
                // int time_counter = 0;
                this.bodies = new Body[this.kinectSensor.BodyFrameSource.BodyCount];
                //open the reader for the body frames
                this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }


        private  async void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
           
            bool dataReceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {

                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            // if detect body then capture the face
            if (dataReceived)
            {
                foreach (Body body in bodies)
                {
                    if (!body.IsTracked) continue;

                    //Console.Write("Check Body ");
                    Joint userJoint_Head = body.Joints[JointType.Head];

                    cameraSpacePoints[0] = userJoint_Head.Position;

                    List.Add(cameraSpacePoints[0].Z);
                  
                    if ( _detectedBackButton == true && idDictionary.ContainsKey(body.TrackingId) == false )
                    {
                        try
                        {
                            Console.WriteLine("Detect more people add to idlist");
                            idDictionary.Add(body.TrackingId, false);
                           
                            badge_Screen.Visibility = Visibility.Collapsed;
                            BackGround_Screen.Visibility = Visibility.Collapsed;
                            Figure_Screen.Visibility = Visibility.Collapsed;
                            _detectedBackButton = false;
                            identify_name = null;

                            System.IO.File.Delete(@"C:\Users\v-altsai\Pictures\allfix.jpg");
                        }
                        catch (Exception ){
                        }
                        
                    }

               
                    SendMessageToCloud sc = new SendMessageToCloud();


                    if (List.Count >= 30 && _detectedBackButton == false && identify_name != null && idDictionary.ContainsKey(body.TrackingId) != false && idDictionary[body.TrackingId] == true)
                    {
                        try
                        {
                            sc.sendWindTurbineMessageToCloudAsync(identify_name, identify_Age, identify_Gender, identify_Emotion,identify_EmotionScoreList, deviceConnectionString);

                            identify_name = null;
                            identify_Age = null;
                            identify_Gender = null;
                            identify_Emotion = null;

                            List.Clear();
                        }
                        catch (Exception)
                        {

                        }

                    }


                    while (List.Count >= 30 && _detectedBackButton == false && idDictionary.ContainsKey(body.TrackingId) != false&& idDictionary[body.TrackingId] == false && identify_name == null)
                    {

                        try
                        {
                            Console.WriteLine("Check Body Z : {0}", cameraSpacePoints[0].Z);


                            if (List[29] < 0.9 && idDictionary[body.TrackingId] == false)
                            {

                                this.IdentifyState = "Image Capturing...";

                                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件

                                sw.Reset();//碼表歸零

                                sw.Start();//碼表開始計時

                                count_timer = new DispatcherTimer();

                                count_timer.Interval = new TimeSpan(0, 0, 0, 1, 0);

                                count_timer.Tick += CountTimer_Tick;

                                count_timer.Start();

                                idDictionary[body.TrackingId] = true;


                            }
                            else if (List[29] > 0.9 && idDictionary[body.TrackingId] == false)
                            {
                                this.IdentifyState = "Get closer... " + Convert.ToString(cameraSpacePoints[0].Z);
                            }


                            List.Clear();
                            //BackGround_Screen.Visibility = Visibility.Collapsed;
                        }
                        catch (Exception)
                        {

                        }
                    }
                  

                }
            }
        }
        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void Load_BgImage()

        {

                StringBuilder st = new StringBuilder();
                StringBuilder st1 = new StringBuilder();
                StringBuilder st2 = new StringBuilder();

                st.Append("Images/Slide11");
               
                st.Append(".JPG");

                bg_pool = new BitmapImage(new Uri(st.ToString(), UriKind.RelativeOrAbsolute));

                st1.Append("Images/badge3.PNG");

                st2.Append("Image/badge.PNG");

                badge[0] = new BitmapImage(new Uri(st1.ToString(), UriKind.RelativeOrAbsolute));
                badge[1] = new BitmapImage(new Uri(st2.ToString(), UriKind.RelativeOrAbsolute));
            //}

        }

        


        private void Load_FigureImage()

        {


            StringBuilder st = new StringBuilder();

            st.Append("C:/Users/v-altsai/Pictures/allfix");

            st.Append(".JPG");

           // figureImage = new BitmapImage(new Uri(st.ToString(), UriKind.RelativeOrAbsolute));

            //}

            using (var bitmap = new Bitmap(Constants.BG_WIDTH, Constants.BG_HEIGHT))

            {

                using (var canvas = Graphics.FromImage(bitmap))

                {

                    

                        //String Fi_Photos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                        //String fi_path = System.IO.Path.Combine(Fi_Photos, "Body" + Facename_Pool[i] + ".png");

                        System.Drawing.Image temp_body = System.Drawing.Image.FromFile(st.ToString());

                        int dx = Constants.POSITION_OFFSET[1].X;

                        int dy = Constants.POSITION_OFFSET[1].Y;

                        //canvas.DrawImage(temp_body, dx, dy, Constants.FIGURE_WIDTH * Constants.resizeRatio, Constants.FIGURE_HEIGHT * Constants.resizeRatio);

                        canvas.DrawImage(temp_body, dx, dy, Constants.FIGURE_WIDTH * Constants.resizeRatio, Constants.FIGURE_HEIGHT * Constants.resizeRatio);

                    

                    canvas.Save();

                    Figure_Screen.Source = Utils.Bitmap2BitmapImage(bitmap);

                    System.Console.WriteLine("finish fig bitmap");

                    canvas.Dispose();


                }
                
                bitmap.Dispose();

            }

        }

        public async void CaptureImg()
        {

            if (this.colorBitmap == null)
            {
               
            }
          
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".jpg");

            

            // write the new file to disk
            // FileStream is IDisposable
            try
            {
                // FileStream is IDisposable
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                    fs.Close();
                }
               
                this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
            }
            catch (IOException)
            {
                this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
            }

            // this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);

            System.Console.WriteLine(path);

            HeadRandom = System.IO.Path.GetRandomFileName().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);


            TestFace tf = new TestFace();

            if (path != null)
            {
                this.IdentifyState = "Analysis your face...";

                loading_animation.Visibility = Visibility.Visible;

                Console.WriteLine("Call Face API");

                var s = await tf.Testpicture(path);

                JSONHelper helper = new JSONHelper();


                People p = new People();

                p = helper.ConvertJSonToObject<People>(s.ToString());

                identify_name = p.Name;
                identify_Gender = p.Gender;
                identify_Age = p.Age;
                identify_Emotion = p.Emotion;
                identify_EmotionScoreList = p.Emotionlistscore;

                foreach (KeyValuePair<string, float> row in identify_EmotionScoreList)
                {
                    Console.WriteLine("Key: " + row.Key + " Value: " + row.Value);
                }

                if (p.Name == "none")
                    this.IdentifyState = "You aren't permitted to access!";
                else
                    this.IdentifyState = "Hi " + p.Name + ",\nYou are permitted to access!";

                this.PersonName = "Name: " + p.Name + "\nGender: " + p.Gender + "\nAge: " + p.Age + "\nEmotion: " + p.Emotion;

                viewmode = true;

                if (viewmode)
                {
                    
                    BackGround_Screen.Source = bg_pool;
     
                    badge_Screen.Source = badge[0];

                    Load_FigureImage();

                    loading_animation.Visibility = Visibility.Collapsed;

                    Figure_Screen.Visibility = Visibility.Visible;

                    BackGround_Screen.Visibility = Visibility.Visible;

                    badge_Screen.Visibility = Visibility.Visible;

                    viewmode = false;

                }

            }
         
        }

        public async void GetTestFace()
        {
            TestFace tf = new TestFace();

            string personImageDir = @"C:\Users\v-altsai\Pictures\Original";

            string[] ImagePathArray = Directory.GetFiles(personImageDir, "*.jpg");
            
            this.IdentifyState = "Analysis your face...";

            loading_animation.Visibility = Visibility.Visible;

            Console.WriteLine("Call Face API");

            string s = await tf.Testpicture(ImagePathArray[0]);

            JSONHelper helper = new JSONHelper();

            SendMessageToCloud sc = new SendMessageToCloud();

            People p = new People();

            p = helper.ConvertJSonToObject<People>(s.ToString());

            identify_name = p.Name;
            identify_Gender = p.Gender;
            identify_Age = p.Age;
            identify_Emotion = p.Emotion;
            identify_EmotionScoreList = p.Emotionlistscore;

            foreach (KeyValuePair<string, float> row in identify_EmotionScoreList)
            {
             Console.WriteLine("Key: " + row.Key + " Value: " + row.Value);
            }

            if (p.Name == "none")
            {
                this.IdentifyState = "You aren't permitted to access!";
            }
            else
            {
                sc.sendWindTurbineMessageToCloudAsync(identify_name, identify_Age, identify_Gender, identify_Emotion, identify_EmotionScoreList, deviceConnectionString);
                this.IdentifyState = "Hi " + p.Name + ",\nYou are permitted to access!";
            }
           this.PersonName = "Name: " + p.Name + "\nGender: " + p.Gender + "\nAge: " + p.Age + "\nEmotion: " + p.Emotion;

            viewmode = true;

            if (viewmode)
            {

                BackGround_Screen.Source = bg_pool;

                badge_Screen.Source = badge[0];

                Load_FigureImage();

                loading_animation.Visibility = Visibility.Collapsed;

                Figure_Screen.Visibility = Visibility.Visible;

                BackGround_Screen.Visibility = Visibility.Visible;

                badge_Screen.Visibility = Visibility.Visible;

                viewmode = false;

                Delete dd = new Delete();

                dd.DeletleImage(personImageDir);

            }
        }
 

            public async void CaptureTrainImg()
        {

            if (this.colorBitmap == null)
            {

            }

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".jpg");

            // write the new file to disk
            // FileStream is IDisposable
            try
            {
                // FileStream is IDisposable
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                    fs.Close();
                }

                this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
            }
            catch (IOException)
            {
                this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
            }

            // this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);

            System.Console.WriteLine(path);

            HeadRandom = System.IO.Path.GetRandomFileName().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            Bitmap oribmp = new Bitmap(path);

            using (Bitmap tmpBmp = new Bitmap(oribmp))

            {

                tmpBmp.Save("back.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                Mount_path.Clear();
                   
                Mount_path.Append("C:\\Users\\v-altsai\\Pictures\\Original\\");

                Mount_path.Append("Original_");

                Mount_path.Append(HeadRandom[0]);

                Mount_path.Append(".jpg");

                tmpBmp.Save(Mount_path.ToString(), System.Drawing.Imaging.ImageFormat.Jpeg);

                Console.WriteLine("{0}",Mount_path.ToString());
            }
 
       }


        private static void Log(string v, int length, object selectedFile)
        {
            throw new NotImplementedException();
        }

        public string PersonName
        {
            get
            {
                return this.personName;
            }

            private set
            {
                if (this.personName != value)
                {
                    this.personName = value;
                    this.PropertyChanged(this, new PropertyChangedEventArgs("PersonName"));

                }

            }
        }

        public string IdentifyState
        {
            get
            {
                return this.identifyState;
            }

            private set
            {
                if (this.identifyState != value)
                {
                    this.identifyState = value;
                    this.PropertyChanged(this, new PropertyChangedEventArgs("identifyState"));

                }

            }
        }
        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        public Boolean IdentifyCheck
        {
            get
            {
                return _IdentifyCheck;
            }
            set
            {
                _IdentifyCheck = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IdentifyCheck"));
                }
            }
        }


        /// <summary>

        /// Gets or sets face detection results in text string

        /// </summary>
       

        public string DetectedResultsInText
        {
            get
            {
                return _detectedResultsInText;
            }
           set
            {
                _detectedResultsInText = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DetectedResultsInText"));
                }
            }
        }
        public bool DetectedBackButton
        {
            get
            {
                return _detectedBackButton;
            }
            set
            {
                _detectedBackButton = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DetectedBackButton"));
                }
            }
        }
        public string SelectedFile

        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                if (PropertyChanged != null)
               {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
                }
            }
        }

       
       

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        private void RetryButton_Click(object sender , RoutedEventArgs e)
        {
            _detectedBackButton = true;
            this.PersonName = "Who are you?";
            this.IdentifyState = "None";
            badge_Screen.Visibility = Visibility.Collapsed;
            BackGround_Screen.Visibility = Visibility.Collapsed;
            Figure_Screen.Visibility = Visibility.Collapsed;
            Check_icon.Visibility = Visibility.Collapsed;
            
            idDictionary.Clear();

        }


        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
         
           
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            //badge_Screen.Visibility = Visibility.Collapsed;
            //BackGround_Screen.Visibility = Visibility.Collapsed;
            NewInputDialog tb = new NewInputDialog();
            this.IdentifyState = "None";
            tb.ShowDialog();

            if (tb.DialogResult == true)
            {
                nameText = tb.Answer;
                if (nameText.Length != 0)
                {
                    Console.WriteLine("nameText : {0}", nameText);
                    MessageBox.Show("Succeed add " + nameText + " !");
                    tb.Close();
                    badge_Screen.Visibility = Visibility.Collapsed;
                    BackGround_Screen.Visibility = Visibility.Collapsed;
                    Figure_Screen.Visibility = Visibility.Collapsed;
                    idDictionary.Clear();

                    isTrain = true;

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件

                    sw.Reset();//碼表歸零

                    sw.Start();//碼表開始計時

                    count_timer = new DispatcherTimer();

                    count_timer.Interval = new TimeSpan(0, 0, 0, 1, 0);

                    count_timer.Tick += CountTimer_Tick;

                    count_timer.Start();
                }
                else
                {
                    MessageBox.Show("Please type valid name!");
                }
                

            }

         

           
        }

        private async void CountTimer_Tick(object sender, EventArgs e)

        {

            countdown++;

            if (countdown == 1)

            {
                mediaPlayer.Open(new Uri("C:/Users/v-altsai/Documents/Visual Studio 2017/Projects/Kinect-FaceRecognition/crrect_answer1.mp3"));

                mediaPlayer.Play();


                number3.Visibility = Visibility.Visible;
            }

            if (countdown == 2)

            {
                mediaPlayer.Open(new Uri("C:/Users/v-altsai/Documents/Visual Studio 2017/Projects/Kinect-FaceRecognition/crrect_answer1.mp3"));

                mediaPlayer.Play();

              

                number3.Visibility = Visibility.Collapsed;

                number2.Visibility = Visibility.Visible;
            }

            if (countdown == 3)

            {
                mediaPlayer.Open(new Uri("C:/Users/v-altsai/Documents/Visual Studio 2017/Projects/Kinect-FaceRecognition/crrect_answer1.mp3"));

                mediaPlayer.Play();

       

                number2.Visibility = Visibility.Collapsed;

                number1.Visibility = Visibility.Visible;
            }

            if (countdown == 4)

            {
                number1.Visibility = Visibility.Collapsed;

                // TrainFace tf = new TrainFace();

                //  tf.trainmodel("family", read_name, path1);
                if (isTrain)
                {

                    for(int i=0;i<3;i++)CaptureTrainImg();

                    this.IdentifyState = "Hi "+nameText+",please wait for training";

                    loading_animation.Visibility = Visibility.Visible;

                    TrainFace TF = new TrainFace();

                    Console.WriteLine("name: " + nameText);

                    await TF.trainmodel(personGroupId, nameText, "C:\\Users\\v-altsai\\Pictures\\Original");

                    this.IdentifyState = "Training success!";

                    loading_animation.Visibility = Visibility.Collapsed;

                    GetTestFace();

                    isTrain = false;
                }
                else
                {
                    CaptureImg();
                }
                countdown = 0;
                //   Thread.Sleep(200);

                mediaPlayer.Open(new Uri("C:/Users/v-altsai/Documents/Visual Studio 2017/Projects/Kinect-FaceRecognition/crrect_answer3.mp3"));

                mediaPlayer.Play();

                count_timer.Stop();

              
            }

        }



        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}
