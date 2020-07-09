// Project: WorldCreatorUnityBridge
// Filename: ObjectSettings.cs
// Copyright (c) 2019 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;
using UnityEngine;

namespace Assets.WorldCreatorUnityBridge.Source
{
  [Serializable]
  public class ObjectSettings
  {
    public string Name;
    public string LayerName;
    public bool CreateGameObjects;
    public bool NatureShader = true;
    [NonSerialized]
    public GameObject Prefab;
    public bool ImportLOD = true;
  }
}
