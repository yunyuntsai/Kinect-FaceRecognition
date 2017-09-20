using Microsoft.Azure.Devices.Client;
using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class TestFace
    {
        public static readonly string ApiKey = ConfigurationManager.AppSettings["FaceApiKey"].ToString();
        /// <summary>
        /// This is testting Face module
        /// </summary>
        public async Task<String> Testpicture(String testImageFile)
        {
            try
            {
                People people = new People();
                
                List<string> rslist = new List<string>();
                string[] HeadRandom;
                StringBuilder Mount_path = new StringBuilder();
                FaceServiceClient fc = new FaceServiceClient(ApiKey, "https://southeastasia.api.cognitive.microsoft.com/face/v1.0");
                string personGroupId = "test";
                // await fc.CreatePersonGroupAsync(personGroupId,"My Family"); 
                //using (Stream s = File.OpenRead(testImageFile))
                //{
                //    if (s != null)
                //        return "yes";
                //    else
                //        return "no";
                //}
                using (Stream s = File.OpenRead(testImageFile))
                {
                    var requiredFaceAttributes = new FaceAttributeType[]
                    {
                            FaceAttributeType.Age,
                            FaceAttributeType.Gender,
                            FaceAttributeType.Smile,
                            FaceAttributeType.FacialHair,
                            FaceAttributeType.HeadPose,
                            FaceAttributeType.Glasses,
                            FaceAttributeType.Emotion
                    };
                    var faces = await fc.DetectAsync(s, returnFaceLandmarks: true, returnFaceAttributes: requiredFaceAttributes);
                    var faceIds = faces.Select(face => face.FaceId).ToArray();
                    try
                    {
                        var results = await fc.IdentifyAsync(personGroupId, faceIds);
                        var fspicture = new FileStream(testImageFile, FileMode.Open);

                        Bitmap bmp = new Bitmap(fspicture);

                        Graphics g = Graphics.FromImage(bmp);

                        int isM = 0, isF = 0;
                        // string age = "";
                        string sex = "";
                        int age;
                        String age_s = "";
                        String emr = "";
                        String Top_Emotion = "";
                        Dictionary<string, float> Emotion = new Dictionary<string, float>();
                        foreach (var face in faces)
                        {
                            var faceRect = face.FaceRectangle;
                            var attributes = face.FaceAttributes;
                            float Happiness = attributes.Emotion.Happiness;
                            float Anger = attributes.Emotion.Anger;
                            float Neutral = attributes.Emotion.Neutral;
                            float Contempt = attributes.Emotion.Contempt;
                            float Disgust = attributes.Emotion.Disgust;
                            float Fear = attributes.Emotion.Fear;
                            float Sadness = attributes.Emotion.Sadness;
                            float Surprise = attributes.Emotion.Surprise;
                            String[] Emotion_string = { "Anger", "Happiness", "Neutral", "Contempt", "Disgust", "Fear", "Sadness", "Surprise" };
                            float[] Emotion_array = { Anger, Happiness, Neutral, Contempt, Disgust, Fear, Sadness, Surprise };
                            
                            // g.DrawEllipse(new Pen(Brushes.Blue, 5), new System.Drawing.Rectangle(faceRect.Left-90, faceRect.Top-90,
                            //   faceRect.Width+150, faceRect.Height+150));
                            /* g.DrawRectangle(
                             new Pen(Brushes.Red, 3),
                             new System.Drawing.Rectangle(faceRect.Left, faceRect.Top,
                                 faceRect.Width, faceRect.Height));*/
                            //g.DrawString(new Font(attributes.Gender.ToString(),));
                            for (int i = 0; i < Emotion_string.Length; i++)
                            {
                                Emotion.Add(Emotion_string[i], Emotion_array[i]);
                            }

                            if (attributes.Gender.StartsWith("male"))
                                isM += 1;
                            else
                                isF += 1;
                            

                            age = Convert.ToInt32(attributes.Age);
                            age_s = age.ToString();
                            sex = attributes.Gender.ToString();

                            Top_Emotion = GetEmotion(attributes.Emotion);
                            Console.WriteLine(Top_Emotion);
                            //Font drawFont = new Font("Arial", 60, FontStyle.Bold);
                            //SolidBrush drawBrush = new SolidBrush(Color.Blue);
                            //PointF drawPoint = new PointF(faceRect.Left - 90, faceRect.Top - 50);
                            //PointF drawPoint2 = new PointF(faceRect.Left - 10, faceRect.Top - 50);
                            //g.DrawString(age_s, drawFont, drawBrush, drawPoint);
                            //g.DrawString(sex, drawFont, drawBrush, drawPoint2);



                            Bitmap CroppedImage = null;

                            if (face.FaceAttributes.HeadPose.Roll >= 10 || face.FaceAttributes.HeadPose.Roll <= -10)

                            {
                                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(Convert.ToInt32(face.FaceRectangle.Left-200), Convert.ToInt32(face.FaceRectangle.Top-200), face.FaceRectangle.Width+200, face.FaceRectangle.Height+200);

                                CroppedImage = new Bitmap(CropRotatedRect(bmp, rect, Convert.ToSingle(face.FaceAttributes.HeadPose.Roll * -1), true));

                            }

                            else

                            {
                                try
                                {
                                    CroppedImage = new Bitmap(bmp.Clone(new System.Drawing.Rectangle(face.FaceRectangle.Left - 150, face.FaceRectangle.Top - 150, face.FaceRectangle.Width + 300, face.FaceRectangle.Height + 300), bmp.PixelFormat));

                                  
                                }
                                catch(Exception e)
                                {
                                    
                                }
                            }
                            bmp.Dispose();
                            CroppedImage.Save(@"C:\Users\v-altsai\Pictures\allfix.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            CroppedImage.Dispose();
                            Console.WriteLine("Age " + age);
                        }
                        Console.WriteLine("Female: " + isF);
                        Console.WriteLine("Male: " + isM);
                        String name = "";
                        foreach (var identifyResult in results)
                        {
                            // Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                            if (identifyResult.Candidates.Length == 0)
                            {
                                Console.WriteLine("No one identified");
                                name = "none";
                            }
                            else if(identifyResult.Candidates.Length != 0)
                            {
                                var candidateId = identifyResult.Candidates[0].PersonId;
                                var person = await fc.GetPersonAsync(personGroupId, candidateId);
                                Console.WriteLine("Identified as {0}", person.Name);
                                name = person.Name;

                                Bitmap oribmp = new Bitmap(@"C:\Users\v-altsai\Pictures\allfix.jpg");

                                using (Bitmap tmpBmp = new Bitmap(oribmp))

                                { 

                                    Mount_path.Clear();

                                    Mount_path.Append("C:\\Users\\v-altsai\\Pictures\\Family\\");

                                    Mount_path.Append(person.Name);

                                    Mount_path.Append(".jpg");

                                    tmpBmp.Save(Mount_path.ToString(), System.Drawing.Imaging.ImageFormat.Jpeg);

                                    Console.WriteLine("{0}", Mount_path.ToString());
                                }
                            }
                        }

                    

                        // bmp.Save(@"C:\Users\v-altsai\Documents\Visual Studio 2017\Projects\Kinect-FaceRecognition\Images\allfix.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                        people.Name = name;
                        people.Age = age_s;
                        people.Gender = sex;
                        people.Emotion = Top_Emotion;
                        people.Emotionlistscore = Emotion;
                        JSONHelper helper = new JSONHelper();
                        String jsonResult = "";
                        jsonResult = helper.ConvertObjectToJSon(people);
                        fspicture.Close();
                        s.Close();
                        Emotion.Clear();
                        return jsonResult;
                      
                    }
                    catch (FaceAPIException fs)
                    {
                        Console.WriteLine(fs.ToString());
                        Console.WriteLine("error results");
                        //if (s != null)
                        //    s.Close();
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                String msg = "Oops! Something went wrong. Try again later";
                if (e is ClientException && (e as ClientException).Error.Message.ToLowerInvariant().Contains("access denied"))
                {
                    msg += " (access denied - hint: check your APIKEY ).";
                    Console.Write(msg);
                }
                Console.Write(e.ToString());
                return null;
            }
        }
        private string GetEmotion(Microsoft.ProjectOxford.Common.Contract.EmotionScores emotion)
        {
            string emotionType = string.Empty;
            double emotionValue = 0.0;
            if (emotion.Anger > emotionValue)
            {
                emotionValue = emotion.Anger;
                emotionType = "Anger";
            }
            if (emotion.Contempt > emotionValue)
            {
                emotionValue = emotion.Contempt;
                emotionType = "Contempt";
            }
            if (emotion.Disgust > emotionValue)
            {
                emotionValue = emotion.Disgust;
                emotionType = "Disgust";
            }
            if (emotion.Fear > emotionValue)
            {
                emotionValue = emotion.Fear;
                emotionType = "Fear";
            }
            if (emotion.Happiness > emotionValue)
            {
                emotionValue = emotion.Happiness;
                emotionType = "Happiness";
            }
            if (emotion.Neutral > emotionValue)
            {
                emotionValue = emotion.Neutral;
                emotionType = "Neutral";
            }
            if (emotion.Sadness > emotionValue)
            {
                emotionValue = emotion.Sadness;
                emotionType = "Sadness";
            }
            if (emotion.Surprise > emotionValue)
            {
                emotionValue = emotion.Surprise;
                emotionType = "Surprise";
            }
            return emotionType;
        }

        public static Bitmap CropRotatedRect(Bitmap source, System.Drawing.Rectangle rect, float angle, bool HighQuality)

        {

            Bitmap result = new Bitmap(rect.Width, rect.Height);

            using (Graphics g = Graphics.FromImage(result))

            {

                g.InterpolationMode = HighQuality ? InterpolationMode.HighQualityBicubic : InterpolationMode.Default;

                using (System.Drawing.Drawing2D.Matrix mat = new System.Drawing.Drawing2D.Matrix())

                {

                    mat.Translate(-rect.Location.X, -rect.Location.Y);

                    System.Drawing.Point p = new System.Drawing.Point(rect.Location.X + rect.Width / 2, rect.Location.Y + rect.Height / 2);

                    mat.RotateAt(angle, p);

                    g.Transform = mat;

                    g.DrawImage(source, new System.Drawing.Point(0, 0));

                }

            }

            return result;

        }


    }
}
