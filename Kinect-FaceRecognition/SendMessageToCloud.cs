using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class SendMessageToCloud
    {
        private const string DEVICENAME = "Monitor";// It's hard-coded for this workshop
        private static DeviceClient _deviceClient;
        private static bool _isStopped = false;
        ArrayList EmotionArray = new ArrayList();
        public  async void sendWindTurbineMessageToCloudAsync(string PersonName, string PersonAge, string PersonGender, string PersonEmotion, Dictionary<string,float> EmotionScoreList,string DeviceConnectionString)
        {
            // Random rand = new Random();
            // string[] names = { "Amy", "Alice", "Cindy", "Eric", "Jackie", "Joanne", "Jonathan", "York", "None" };

            int i = 1;
         
                _deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Amqp);
                Console.WriteLine("connect to iot hub");
                if (_isStopped == false)
                {
                    //string tmp_name = names[rand.Next(names.Length)];
                    int tmp_open = 1;
                    if (String.Compare(PersonName, "None", true) == 0)
                    {
                        tmp_open = 0;
                    }
              
              
                foreach (KeyValuePair<string, float> element in EmotionScoreList)
                {
                    //Console.WriteLine("Key: " + element.Key + " Value: " + element.Value);
                    EmotionArray.Add(element.Value);
                }

                var telemetryDataPoint = new
                    {
                        deviceId = DEVICENAME,
                        msgId = "message id " + i,
                        name = PersonName,
                        open = tmp_open,
                        age = PersonAge,
                        gender = PersonGender,
                        emotion = PersonEmotion,       
                        angerScore = EmotionArray[0],
                        happyScore = EmotionArray[1],
                        neutralScore = EmotionArray[2],
                        contemptScore = EmotionArray[3],
                        disgustScore = EmotionArray[4],
                        fearScore = EmotionArray[5],
                        sadScore = EmotionArray[6],
                        surpriseScore = EmotionArray[7],
                        time = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-ddTHH:mm:ss.fffZ") // ISO8601 format, https://zh.wikipedia.org/wiki/ISO_8601
                    };

                    var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                    var message = new Message(Encoding.ASCII.GetBytes(messageString));
                    message.Properties.Add("Device", "Kinect");
                    await _deviceClient.SendEventAsync(message);
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                    i++;
       
                }
                else
                {
                    Console.WriteLine("{0} > Turn Off", DateTime.Now);
               
                }

             
            

        }
    }
}
