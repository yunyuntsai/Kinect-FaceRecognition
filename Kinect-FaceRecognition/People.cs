using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    public class People
    {

            public String Age { get; set; }
            public String Name { get; set; }
            public String Gender { get; set; }
            public String Emotion { get; set; }
            public Dictionary<string,float> Emotionlistscore { get; set; }

    }
}
