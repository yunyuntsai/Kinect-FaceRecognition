using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class Delete
    {
        /// <summary>
        /// Delete .jpg file in the Directory
        /// </summary>
        /// <param name="deletepath">"The directory path,this directory is save the cropface file"</param>
        public Boolean DeletleImage(string deletepath)
        {
            if (System.IO.Directory.Exists(deletepath))
            {
                foreach (string imagePath in Directory.GetFiles(deletepath, "*.jpg"))
                {
                    if (System.IO.File.Exists(imagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(imagePath);
                           /* if (Directory.GetDirectories(deletepath).Length > 0 || Directory.GetFiles(deletepath).Length > 0)
                            {
                                Console.WriteLine("The Directory have other filetype or not empty");
                                return false;
                            }*/

                        }
                        catch (System.IO.IOException e)
                        {
                            Console.WriteLine(e.Message);
                            return false;
                        }

                    }

                }
            }
            else
            {
                Console.WriteLine(deletepath + " This Directory does not Exists");
                return false;
            }
            Console.WriteLine("Delete success");
            return true;
        }

    }
}
