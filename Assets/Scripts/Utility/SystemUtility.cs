using System;
using System.IO;
using UnityEngine;

namespace Utility
{
    public static class SystemUtility
    {
        
        public static string GetAndCreateDataDirectory()
        {
            
            var directory = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            directory += AddSlashes("ABR");
            Directory.CreateDirectory(directory);
            return directory;

        }

        /*public static string GetAndCreateRobotsDirectory()
        {

            var dataDirectory = GetAndCreateDataDirectory();
            var robotsDirectory

        }*/

        private static string AddSlashes(string str)
        {

            string res;
            if (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {

                res = "\\" + str + "\\";

            }
            else
            {

                res = "/" + str + "/";

            }

            return res;

        }
        
    }
}