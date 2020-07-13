using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using Building;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Utility
{
    public static class SystemUtility
    {
        public const string RobotDirectoryName = "Robots";

        public static SimplePlatform GetSimplePlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:

                    return SimplePlatform.Windows;

                case RuntimePlatform.WindowsPlayer:

                    return SimplePlatform.Windows;

                case RuntimePlatform.OSXPlayer:

                    return SimplePlatform.Posix;

                case RuntimePlatform.OSXEditor:

                    return SimplePlatform.Posix;

                case RuntimePlatform.LinuxEditor:

                    return SimplePlatform.Posix;

                case RuntimePlatform.LinuxPlayer:

                    return SimplePlatform.Posix;

                default:

                    throw new NotImplementedException("Operating systems other than Windows NT and " +
                                                      "POSIX systems are not currently supported.");
            }
        }

        public static bool TryConnectPipeClientWindows(NamedPipeClientStream clientStream)
        {
            try
            {
                clientStream.Connect();
            }
            catch (Win32Exception e)
            {
                return false;
            }

            return true;
        }

        public static bool TryConnectPipeClientWindows(NamedPipeClientStream clientStream, out bool connected)
        {
            connected = TryConnectPipeClientWindows(clientStream);
            return connected;
        }

        public static bool IsDuplexPipeStillConnectedWindows(NamedPipeClientStream clientStream)
        {
            try
            {
                clientStream.WriteByte(1);
            }
            catch (IOException)
            {
                Debug.LogError("Pipe is closed");
                return false;
            }

            return true;
        }

        private static string AddPlatformSlashes(string str)
        {
            if (GetSimplePlatform() == SimplePlatform.Windows) return "\\" + str + "\\";

            return "/" + str + "/";
        }

        public static string GetAndCreateDataDirectory()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            return Application.persistentDataPath;
        }

        public static string GetAndCreateRobotsDirectory()
        {
            var path = GetAndCreateDataDirectory() + AddPlatformSlashes(RobotDirectoryName);
            Directory.CreateDirectory(path);
            return path;
        }

        public static RobotStructure GetRobotStructureByName(string robotName)
        {
            if (string.IsNullOrEmpty(robotName))
                throw new ArgumentException("Robot name must not be null or empty");

            return GetRobotStructureByPath(GetAndCreateRobotsDirectory() + robotName + ".json");
        }

        public static RobotStructure GetSelectedRobotStructure()
        {
            return GetRobotStructureByName(PlayerPrefs.GetString(PlayerPrefKeys.SelectedRobotNameKey));
        }

        public static RobotStructure GetRobotStructureByPath(string filePath)
        {
            if (!filePath.Substring(filePath.Length - 5).Equals(".json"))
                throw new ArgumentException("Robot file must be json!");
            
            var file = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read);

            var fileReader = new StreamReader(file);

            var json = fileReader.ReadToEnd();
            var structure = JsonUtility.FromJson<RobotStructure>(json);

            fileReader.Close();

            return structure;
        }
        
        public static RobotStructure[] GetAllRobotStructures()
        {
            var robotDirectory = SystemUtility.GetAndCreateRobotsDirectory();
            var robotFiles = Directory.GetFiles(robotDirectory, 
                "*.json", 
                SearchOption.AllDirectories);

            var result = new List<RobotStructure>();
            foreach (var robotFile in robotFiles)
            {
                RobotStructure structure;
                
                try
                {
                    structure = GetRobotStructureByPath(robotFile);
                }
                catch (Exception e)
                {
                    Debug.LogError("Could not load robot file: " + e.Message);
                    continue;
                }
                
                result.Add(structure);
            }

            return result.ToArray();
        }
    }
}