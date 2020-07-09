// Project: WorldCreatorUnityBridge
// Filename: BridgeSettings.cs
// Copyright (c) 2019 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;
using UnityEngine;

#if UNITY_EDITOR

namespace Assets.WorldCreatorUnityBridge.Source
{
  [Serializable]
  public class BridgeSettings
  {
    public bool ClampValues = true;

    public int SplitThreshold = 2;

    public bool IsImportObjects = true;

    public bool IsImportDetails = true;

    public bool IsImportTextures = true;

    public float WorldScale = 1;

    public string WorldScaleString = "1.00";

    public MaterialType MaterialType = MaterialType.Standard;

    public string bridgeFilePath = null;

    public Material CustomMaterial;

    public ObjectSettings[] ObjectSettings;

    /// <summary>
    /// Default folder where imported terrain data are stored.
    /// </summary>
    public string terrainsFolderName = "WorldCreatorTerrains";

    public string terrainAssetName = "WC_Terrain";

    public bool deleteUnusedAssets = true;

    public int GetSplitResolution()
    {
      return 128 << SplitThreshold;
    }

    public bool IsBridgeFileValid()
    {
      return this.bridgeFilePath != null && this.bridgeFilePath.Length > 0;
    }
  }
}

#endif
