// Project: WorldCreatorUnityBridge
// Filename: Importer.cs
// Copyright (c) 2019 BiteTheBytes GmbH. All rights reserved
// *********************************************************

#region Using

using System.IO;

#endregion

#if UNITY_EDITOR

namespace Assets.WorldCreatorUnityBridge.Source
{
  public static class Importer
  {
    #region Methods (Static / Public)

    public static float[,] RawUint16FromFile(string filePath, int width, int height, bool bigEndian, int stride = 0,
      int headerSize = 0, bool flipY = false, bool flipX = false)
    {
      stride = stride == 0 ? width * 2 : stride;
      var terrainData = new float[height, width];
      using (var stream = File.OpenRead(filePath))
      {
        if (!bigEndian)
        {
          for (var y = 0; y < height; y++)
          {
            stream.Position = y * stride + headerSize;
            var readY = flipY ? (height - y) - 1 : y;
            for (var x = 0; x < width; x++)
            {
              var lower = (byte) stream.ReadByte();
              var upper = (byte) stream.ReadByte();
              var val = (float) (lower | (upper << 8)) / ushort.MaxValue;
              var readX = flipX ? (width - x) - 1 : x;

              terrainData[readY, readX] = val;
            }
          }
        }
        else
        {
          for (var y = 0; y < height; y++)
          {
            stream.Position = y * stride + headerSize;
            var readY = flipY ? (height - y) - 1 : y;
            for (var x = 0; x < width; x++)
            {
              var upper = (byte) stream.ReadByte();
              var lower = (byte) stream.ReadByte();
              var val = (float) (lower | (upper << 8)) / ushort.MaxValue;
              var readX = flipX ? (width - x) - 1 : x;

              terrainData[readY, readX] = val;
            }
          }
        }
      }

      return terrainData;
    }

    #endregion
  }
}

#endif