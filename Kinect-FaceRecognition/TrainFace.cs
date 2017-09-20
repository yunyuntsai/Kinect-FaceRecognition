using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class TrainFace
    {
        /// <summary>
        /// This is training Face module
        /// </summary>
        /// <param name="personGroupId">"family"</param>
        /// <param name="name">family user name like "Jakcie","York"...</param>
        /// <param name="picpath">The picture path </param>
        /// <param name="Apikey">The cognitive Face api key</param>
        private static readonly string ApiKey = ConfigurationManager.AppSettings["FaceApiKey"].ToString();
        public async Task trainmodel(string personGroupId, string name, string personImageDir)
        {

            FaceServiceClient fc = new FaceServiceClient(ApiKey, "https://southeastasia.api.cognitive.microsoft.com/face/v1.0");
            //string personGroupId = "family";
            //await fc.CreatePersonGroupAsync(personGroupId, "Test");

            CreatePersonResult friend1 = await fc.CreatePersonAsync(personGroupId, name);

            foreach (string imagePath in Directory.GetFiles(personImageDir, "*.jpg"))
            {
                await WaitCallLimitPerSecondAsync();
                using (Stream stream = File.OpenRead(imagePath))
                {
                    await fc.AddPersonFaceAsync(personGroupId, friend1.PersonId, stream);
                }
            }
            await fc.TrainPersonGroupAsync(personGroupId);
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await fc.GetPersonGroupTrainingStatusAsync(personGroupId);

                if ((int)trainingStatus.Status != 2)
                {
                    break;
                }
                await Task.Delay(1000);
            }
            Console.WriteLine(trainingStatus.Status);        

            /*if((int)trainingStatus.Status == 0) {
                 Delete del = new Delete();
                 del.DeletleImage("C:\\Users\\v-altsai\\Pictures\\Original");
            }*/
        
        }

        const int CallLimitPerSecond = 10;
        static Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
        static async Task WaitCallLimitPerSecondAsync()
        {
            Monitor.Enter(_timeStampQueue);
            try
            {
                if (_timeStampQueue.Count >= CallLimitPerSecond)
                {
                    TimeSpan timeInterval = DateTime.UtcNow - _timeStampQueue.Peek();
                    if (timeInterval < TimeSpan.FromSeconds(1))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1) - timeInterval);
                    }
                    _timeStampQueue.Dequeue();
                }
                _timeStampQueue.Enqueue(DateTime.UtcNow);
            }
            finally
            {
                Monitor.Exit(_timeStampQueue);
            }
        }
    }
}
