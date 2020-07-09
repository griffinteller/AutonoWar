// Project: WorldCreatorUnityBridge
// Filename: Util.cs
// Copyright (c) 2019 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Assets.WorldCreatorSyncTool.Source
{
  static class Util
  {
    public static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
      DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
      DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

      CopyDirectory(diSource, diTarget);
    }

    private static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
    {
      Directory.CreateDirectory(target.FullName);

      // Copy each file into the new directory.
      foreach (FileInfo fi in source.GetFiles())
      {
        fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
      }

      // Copy each subdirectory using recursion.
      foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
      {
        DirectoryInfo nextTargetSubDir =
            target.CreateSubdirectory(diSourceSubDir.Name);
        CopyDirectory(diSourceSubDir, nextTargetSubDir);
      }
    }

    public static bool ToBoolean(this string s, bool defaultValue = false)
    {
      if (s == null)
        return defaultValue;
      if (Boolean.TryParse(s, out var res))
        return res;
      return defaultValue;
    }

    public static float ToFloat(this string s, float defaultValue = 0.0f)
    {
      if (s == null)
        return defaultValue;
      if (float.TryParse(s, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var res))
        return res;
      return defaultValue;
    }

    public static Vector3 ToVector3(this string s, Vector3 defaultValue)
    {
      if (s == null)
        return defaultValue;

      var parts = s.Split(',');      
      try
      {
        var v = new Vector3();
        v.x = float.Parse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        v.y = float.Parse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        v.z = float.Parse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture);
        return v;
      }
      catch (Exception e)
      {
        Debug.Log("Vector parse failed");
      }
      return defaultValue;
    }
    public static int ToInt(this string s, int defaultValue = 0)
    {
      if (s == null)
        return defaultValue;
      if (int.TryParse(s, out var res))
        return res;
      return defaultValue;
    }
  }
}
