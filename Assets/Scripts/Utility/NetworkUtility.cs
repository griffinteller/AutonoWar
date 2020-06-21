using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Photon.Pun;
using Photon.Realtime;

namespace Utility
{
    public static class NetworkUtility
    {
        public static Player[] PlayerArrayByActorNumber()
        {
            var playerDict = PhotonNetwork.CurrentRoom.Players;
            var result = new Player[playerDict.Count];
            
            playerDict.Values.CopyTo(result, 0);
            Array.Sort(result, (player1, player2) => player1.ActorNumber.CompareTo(player2.ActorNumber));

            return result;
        }

        public static int[] ActorNumberArray()
        {
            var players = PhotonNetwork.CurrentRoom.Players;
            var result = new int[players.Count];
            
            players.Keys.CopyTo(result, 0);
            Array.Sort(result);

            return result;
        }

        public static int GetPlayerIndex(int actorNumber)
        {
            var actorNumbers = ActorNumberArray();
            return Array.IndexOf(actorNumbers, actorNumber);
        }

        public static byte[] Serialize(object obj, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            byte[] result;
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, obj);
                result = Compress(memoryStream.ToArray());
            }

            return result;
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            T result;
            using (var memoryStream = new MemoryStream())
            {
                var decompressedBuffer = Decompress(buffer);
                var binaryFormatter = new BinaryFormatter();
                
                memoryStream.Write(decompressedBuffer, 0, decompressedBuffer.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                
                result = (T) binaryFormatter.Deserialize(memoryStream);
            }

            return result;
        }
        
        public static byte[] Compress(byte[] input, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            byte[] compressedData;

            using (var outputStream = new MemoryStream())
            {
                using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
                    zip.Write(input, 0, input.Length);

                compressedData = outputStream.ToArray();
            }

            return compressedData;
        }
        
        public static byte[] Decompress(byte[] input)
        {
            byte[] decompressedData;

            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = new MemoryStream(input))
                    using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                        zip.CopyTo(outputStream);

                decompressedData = outputStream.ToArray();
            }

            return decompressedData;
        }
    }
}