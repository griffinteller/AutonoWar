using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using UnityEngine;

namespace Utility
{
    public static class SystemUtility
    {

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
        
    }
    
}