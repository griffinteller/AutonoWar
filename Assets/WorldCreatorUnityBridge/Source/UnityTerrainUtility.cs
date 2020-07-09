// Project: WorldCreatorUnityBridge
// Filename: UnityTerrainUtility.cs
// Copyright (c) 2019 BiteTheBytes GmbH. All rights reserved
// *********************************************************

#region Using

using Assets.WorldCreatorSyncTool.Source;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#endregion

#if UNITY_EDITOR

namespace Assets.WorldCreatorUnityBridge.Source
{
  struct ObjectData
  {
    public Vector3 Position;
    public Vector2 Size;
    public Quaternion Rotation;
    public Color32 Color;
  }

  public static class UnityTerrainUtility
  {
    #region Methods (Static / Public)

    public static GameObject CreateTerrainFromFile(BridgeSettings settings)
    {
      var directory = Path.GetDirectoryName(settings.bridgeFilePath);
      var terrainDirectory = "Assets/" + settings.terrainsFolderName + "/" + settings.terrainAssetName + "/";
      var assetsDirectory = terrainDirectory + "Assets/";

      GameObject newTerrainGameObject = null;

      Vector3 terrainPosition = Vector3.zero;
      float widthScaleOff, heightScaleOff, realLength, realHeight;
      int xParts, yParts;
      int realResX, realResY;
      int splitRes = settings.GetSplitResolution();
      Terrain[,] parts = null;



      // Load sync file
      /////////////////
      XmlDocument doc = new XmlDocument();
      doc.Load(settings.bridgeFilePath);

      // Load Surface
      ///////////////      
      var surfaceElements = doc.GetElementsByTagName("Surface");
      if (surfaceElements.Count > 0)
      {
        var surface = surfaceElements[0];

        int xBase, yBase, heightmapResolution, alphamapResolution, width, length;
        float height, minHeight, maxHeight, heightCenter;

        int.TryParse(surface.Attributes["ResolutionX"].Value, out xBase);
        int.TryParse(surface.Attributes["ResolutionY"].Value, out yBase);
        int.TryParse(surface.Attributes["Width"].Value, out width);
        int.TryParse(surface.Attributes["Length"].Value, out length);
        float.TryParse(surface.Attributes["Height"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out height);
        float.TryParse(surface.Attributes["MinHeight"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out minHeight);
        float.TryParse(surface.Attributes["MaxHeight"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out maxHeight);
        float.TryParse(surface.Attributes["HeightCenter"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out heightCenter);

        realResX = xBase;
        realResY = yBase;
        int maxRes = Mathf.Max(xBase, yBase);
        int nextP2 = Mathf.NextPowerOfTwo(maxRes);
        if (nextP2 < splitRes)
          splitRes = nextP2;

        xParts = Mathf.CeilToInt((float)xBase / splitRes);
        yParts = Mathf.CeilToInt((float)yBase / splitRes);

        parts = new Terrain[xParts, yParts];


        int resPow2 = Mathf.CeilToInt((float)maxRes / splitRes) * splitRes;
        splitRes = Mathf.Min(resPow2, splitRes);

        int baseRes = Mathf.Max(yBase, xBase);
        length = Mathf.FloorToInt(length * ((float)resPow2 / baseRes));
        width = Mathf.FloorToInt(width * ((float)resPow2 / baseRes));
        heightmapResolution = alphamapResolution = splitRes;

        widthScaleOff = (float)resPow2 / xBase;
        heightScaleOff = (float)resPow2 / yBase;
        heightmapResolution += 1;

        var splitHeightMap = new float[heightmapResolution, heightmapResolution];

        var splitLength = (float)length / yParts;
        var splitWidth = (float)width / xParts;
        realLength = Mathf.Max(splitWidth, splitLength);
        float heightSize = maxHeight - minHeight;


        // Load complete height data
        ////////////////////////////
        var heightmap = Importer.RawUint16FromFile(directory + "/heightmap.raw", xBase, yBase, false);


        newTerrainGameObject = GameObject.Find(settings.terrainAssetName);
        if (newTerrainGameObject != null)
        {
          GameObject.DestroyImmediate(newTerrainGameObject);
        }
        newTerrainGameObject = new GameObject(settings.terrainAssetName);

        for (int yP = 0; yP < yParts; yP++)
        {
          for (int xP = 0; xP < xParts; xP++)
          {

            int xOff = xP * splitRes;
            int yOff = yP * splitRes;
            var assetPath = @"Assets/" + settings.terrainsFolderName + "/" + settings.terrainAssetName + @"/" + settings.terrainAssetName + "_" +
                            xP + "_" + yP + ".asset";
            var terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);
            GameObject terrainPartObject = null;

            if (terrainData == null)
            {
              terrainData = new TerrainData();
              terrainData.SetDetailResolution(splitRes, 16);
              terrainData.name = "WCTerrainData" + "_" + xP + "_" + yP;
              AssetDatabase.CreateAsset(terrainData, assetPath);
            }

            // Load Height Values
            /////////////////////       
            terrainData.heightmapResolution = heightmapResolution;
            terrainData.alphamapResolution = alphamapResolution;

            // Fill missing values if terrain is not quadratic
            //////////////////////////////////////////////////
            if (xBase != yBase || xBase != heightmapResolution)
            {

              if (settings.ClampValues)
              {
                for (int y = 0; y < heightmapResolution; y++)
                {
                  for (int x = 0; x < heightmapResolution; x++)
                  {
                    splitHeightMap[y, x] = heightmap[Mathf.Min(yOff + y, yBase - 1), Mathf.Min(xOff + x, xBase - 1)];
                  }
                }
              }
              else
              {
                for (int y = 0; y < heightmapResolution; y++)
                {
                  for (int x = 0; x < heightmapResolution; x++)
                  {
                    int realY = y + yOff;
                    int realX = x + xOff;
                    if (realY > yBase - 1 || realX > xBase - 1)
                      splitHeightMap[y, x] = 0;
                    else
                      splitHeightMap[y, x] = heightmap[realY, realX];
                  }
                }
              }
            }

            terrainData.SetHeights(0, 0, splitHeightMap);
            realHeight = height * heightSize;
            terrainData.size = new Vector3(realLength, realHeight, realLength) * settings.WorldScale;
            terrainPosition = new Vector3(realLength * xP, height * minHeight, realLength * yP) *
                              settings.WorldScale;


            var terrainPartObjectTransform = newTerrainGameObject.transform.Find(settings.terrainAssetName + "_" + xP + "_" + yP);
            if (terrainPartObjectTransform == null)
            {
              terrainPartObject = Terrain.CreateTerrainGameObject(terrainData);
              terrainPartObject.name = settings.terrainAssetName + "_" + xP + "_" + yP;
              terrainPartObject.transform.parent = newTerrainGameObject.transform;
            }
            else
            {
              terrainPartObject = terrainPartObjectTransform.gameObject;
              terrainPartObject.GetComponent<Terrain>().terrainData = terrainData;
            }

            // set pixel error to 1 for crisp textures
            terrainPartObject.GetComponent<Terrain>().heightmapPixelError = 1.0f;

            var terrain = terrainPartObject.GetComponent<Terrain>();
            parts[xP, yP] = terrain;

            // Set / Create Material
            ////////////////////////
            Material createdMat = null;
            if (settings.MaterialType != MaterialType.Custom)
            {
              var mat = AssetDatabase.LoadAssetAtPath("Assets/WorldCreatorUnityBridge/Content/Materials/WC-Default-Terrain-" + settings.MaterialType + ".mat", typeof(Material)) as Material;
              var tex = AssetDatabase.LoadAssetAtPath("Assets/" + settings.terrainsFolderName + "/" + settings.terrainAssetName + "/colormap.png", typeof(Texture2D)) as Texture2D;
              var newAssetPath = "Assets/" + settings.terrainsFolderName + "/" + settings.terrainAssetName + $"/terrain_material_x{xP}_y{yP}.mat";
              var matInstance = new Material(mat);
              if (settings.MaterialType == MaterialType.HDRP)
                matInstance.EnableKeyword("HDRP_ENABLED");
              else if (settings.MaterialType == MaterialType.LWRP)
                matInstance.EnableKeyword("LWRP_ENABLED");

              matInstance.SetTexture("_ColorMap", tex);
              matInstance.SetVector("_OffsetSize", new Vector4((float)xP / xParts, (float)yP / yParts, 1.0f / xParts, 1.0f / yParts));
              AssetDatabase.CreateAsset(matInstance, newAssetPath);
              AssetDatabase.SaveAssets();
              AssetDatabase.Refresh();
              createdMat = AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(Material)) as Material;
              terrain.materialTemplate = createdMat;
            }
            else
              terrain.materialTemplate = settings.CustomMaterial;


            var c = terrainPartObject.transform.Find("WC_ObjectContainer");
            if (c != null)
              GameObject.DestroyImmediate(c.gameObject);


            var objContainer = new GameObject("WC_ObjectContainer");
            objContainer.transform.parent = terrainPartObject.transform;
            terrainPartObject.transform.position = terrainPosition;
          }
        }

        // Set neighbours for parts
        for (int yP = 0; yP < yParts; yP++)
        {
          for (int xP = 0; xP < xParts; xP++)
          {
            var left = xP > 0 ? parts[xP - 1, yP] : null;
            var right = xP < xParts - 1 ? parts[xP + 1, yP] : null;
            var top = yP > 0 ? parts[xP, yP - 1] : null;
            var bottom = yP < yParts - 1 ? parts[xP, yP + 1] : null;

            parts[xP, yP].SetNeighbors(left, bottom, right, top);
          }
        }
      }
      else
        return null;

      // Load Splatmaps
      ///////////////// 
      if (settings.IsImportTextures)
      {
        var texturingElements = doc.GetElementsByTagName("Texturing");
        if (texturingElements.Count > 0)
        {
          var texturing = texturingElements[0];
          int textureCount = 0;
          List<TerrainLayer> splatPrototypes = new List<TerrainLayer>();
          foreach (XmlElement splatmap in texturing)
          {
            foreach (XmlElement textureInfo in splatmap.ChildNodes)
            {
              var tileSize = Vector2FromString(textureInfo.Attributes["TileSize"].Value);
              var tileOffset = Vector2FromString(textureInfo.Attributes["TileOffset"].Value);

              var smoothnessString = textureInfo.Attributes["Smoothness"].Value;
              var metallicString = textureInfo.Attributes["Metallic"].Value;
              float smoothness = 1, metallic = 0;
              float.TryParse(smoothnessString, NumberStyles.Any, CultureInfo.InvariantCulture, out smoothness);
              float.TryParse(metallicString, NumberStyles.Any, CultureInfo.InvariantCulture, out metallic);

              TerrainLayer tmp = new TerrainLayer();


              tmp.metallic = metallic;
              tmp.smoothness = smoothness;
              tmp.specular = Color.white;
              tmp.tileOffset = tileOffset;
              tmp.tileSize = tileSize;

              var fileName = textureInfo.Attributes["FileName"].Value;
              var assetName = textureInfo.Attributes["Name"].Value;

              var assetProjectPath = "Assets/" + settings.terrainsFolderName + "/" + settings.terrainAssetName + "/Assets/" + fileName + "/";

              var assetPath = directory + "/Assets/" + fileName + "/Description.xml";
              bool success = false;

              if (File.Exists(assetPath))
              {

                try
                {
                  var assetDoc = new XmlDocument();
                  assetDoc.Load(assetPath);
                  var diffuse = assetDoc.GetElementsByTagName("Diffuse")[0];
                  var normal = assetDoc.GetElementsByTagName("Normal")[0];
                  var diffusePath = assetProjectPath + diffuse.Attributes["File"].Value;
                  tmp.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(diffusePath);
                  var normalPath = assetProjectPath + normal.Attributes["File"].Value;
                  tmp.normalMapTexture = ImportNormal(normalPath);
                  success = true;
                }
                catch (Exception e)
                {
                  Debug.Log("Failed to load textures: " + e);
                }
              }

              if (!success)
              {
                tmp.diffuseTexture = Texture2D.whiteTexture;
                tmp.normalMapTexture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                var normal = new Color(0, 0.5f, 0, 0.5f);
                tmp.normalMapTexture.SetPixel(0, 0, normal);
                tmp.normalMapTexture.SetPixel(1, 0, normal);
                tmp.normalMapTexture.SetPixel(0, 1, normal);
                tmp.normalMapTexture.SetPixel(1, 1, normal);
                tmp.normalMapTexture.Apply(true);
                Debug.Log("Failed to load Texture Asset");
              }

              AssetDatabase.CreateAsset(tmp, terrainDirectory + $"terrain_layer_{textureCount}");
              splatPrototypes.Add(tmp);
              textureCount++;
            }
          }
          var splatProtoArray = splatPrototypes.ToArray();


          var alphaMaps = new float[realResX, realResY, textureCount];

          int textureIndex = 0;
          foreach (XmlElement splatmap in texturing)
          {
            var fileName = splatmap.Attributes["Name"].Value;
            int textureWidth, textureHeight;
            var pixels = LoadTGA(directory + "/" + fileName, out textureWidth, out textureHeight);

            int channelIndex = 0;
            foreach (XmlElement textureInfo in splatmap.ChildNodes)
            {
              for (int y = 0; y < realResY; y++)
              {
                for (int x = 0; x < realResX; x++)
                {
                  int realY = Mathf.FloorToInt(y);
                  int realX = (textureWidth - Mathf.FloorToInt(x)) - 1;


                  if (settings.ClampValues)
                  {
                    realX = Mathf.Clamp(realX, 0, textureWidth - 1);
                    realY = Mathf.Clamp(realY, 0, textureHeight - 1);
                  }
                  else if (realX < 0 || realX >= textureWidth || realY < 0 || realY >= textureHeight)
                  {
                    alphaMaps[x, y, textureIndex] = 0;
                    continue;
                  }

                  int pixelIndex = realY * textureWidth + realX;
                  switch (channelIndex)
                  {

                    case 2:
                      alphaMaps[x, y, textureIndex] = pixels[pixelIndex].x;
                      break;
                    case 1:
                      alphaMaps[x, y, textureIndex] = pixels[pixelIndex].y;
                      break;
                    case 0:
                      alphaMaps[x, y, textureIndex] = pixels[pixelIndex].z;
                      break;
                    case 3:
                      alphaMaps[x, y, textureIndex] = pixels[pixelIndex].w;
                      break;
                  }
                }
              }
              channelIndex++;
              textureIndex++;
            }
          }

          int alphaMapRes = Mathf.Min(4096, splitRes);
          var splitAlphaMaps = new float[alphaMapRes, alphaMapRes, textureCount];
          for (int yP = 0; yP < yParts; yP++)
          {
            for (int xP = 0; xP < xParts; xP++)
            {
              var terrainData = parts[xP, yP].terrainData;
              terrainData.terrainLayers = splatProtoArray;


              int xOff = xP * splitRes;
              int yOff = yP * splitRes;

              float scale = splitRes <= 4096 ? 1 : (float)splitRes / alphaMapRes;

              for (int y = 0; y < alphaMapRes; y++)
              {
                for (int x = 0; x < alphaMapRes; x++)
                {
                  int realY = yOff + Mathf.CeilToInt(y * ((float)(splitRes + 1) / splitRes) * scale);
                  int realX = xOff + Mathf.CeilToInt(x * ((float)(splitRes + 1) / splitRes) * scale);


                  if (settings.ClampValues)
                  {
                    if (realX >= realResX)
                      realX = realResX - 1;
                    if (realY >= realResY)
                      realY = realResY - 1;
                  }
                  else if (realX < 0 || realX >= realResX || realY < 0 || realY >= realResY)
                  {
                    for (int i = 0; i < textureCount; i++)
                    {
                      splitAlphaMaps[y, x, i] = 0;
                    }
                    continue;
                  }

                  for (int i = 0; i < textureCount; i++)
                  {
                    splitAlphaMaps[y, x, i] = alphaMaps[realX, realY, i];
                  }
                }
              }
              terrainData.SetAlphamaps(0, 0, splitAlphaMaps);
            }
          }
        }
      }


      // Load Details
      ///////////////
      if (settings.IsImportDetails)
      {
        var detailElements = doc.GetElementsByTagName("Details");
        if (detailElements.Count > 0 && detailElements[0].ChildNodes.Count > 0)
        {
          XmlNode details = detailElements[0];
          XmlNode layers = details.ChildNodes[0];

          var prototypes = new List<DetailPrototype>();
          int count = 0;
          foreach (XmlElement layer in layers.ChildNodes)
          {
            foreach (XmlElement detail in layer.ChildNodes)
            {
              var minWidthString = detail.Attributes["MinWidth"].Value;
              var maxWidthString = detail.Attributes["MaxWidth"].Value;
              var minHeightString = detail.Attributes["MinHeight"].Value;
              var maxHeightString = detail.Attributes["MaxHeight"].Value;

              float minWidth = 1, maxWidth = 1, minHeight = 1, maxHeight = 1;
              float.TryParse(minWidthString, NumberStyles.Any, CultureInfo.InvariantCulture, out minWidth);
              float.TryParse(maxWidthString, NumberStyles.Any, CultureInfo.InvariantCulture, out maxWidth);
              float.TryParse(minHeightString, NumberStyles.Any, CultureInfo.InvariantCulture, out minHeight);
              float.TryParse(maxHeightString, NumberStyles.Any, CultureInfo.InvariantCulture, out maxHeight);

              DetailPrototype tmp = new DetailPrototype();

              const float scaleFactor = 3;
              tmp.usePrototypeMesh = false;
              tmp.bendFactor = 1;
              tmp.dryColor = Color.white;
              tmp.healthyColor = Color.white;
              tmp.minWidth = minWidth * scaleFactor;
              tmp.maxWidth = maxWidth * scaleFactor;
              tmp.minHeight = minHeight * scaleFactor;
              tmp.maxHeight = maxHeight * scaleFactor;
              tmp.noiseSpread = 1;




              var fileName = detail.Attributes["AssetName"].Value;
              var assetName = detail.Attributes["Name"].Value;
              var assetProjectPath = "Assets/" + settings.terrainsFolderName + "/" + settings.terrainAssetName + "/Assets/" + fileName + "/";
              var assetFilePath = assetProjectPath + "/Description.xml";
              bool success = false;
              if (File.Exists(assetFilePath))
              {
                try
                {
                  var assetDoc = new XmlDocument();
                  assetDoc.Load(assetFilePath);
                  var diffuse = assetDoc.GetElementsByTagName("Diffuse")[0];

                  var diffusePath = assetProjectPath + diffuse.Attributes["File"].Value;
                  tmp.prototypeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(diffusePath);
                  tmp.renderMode = DetailRenderMode.GrassBillboard;
                  success = true;
                }
                catch (Exception e)
                {
                }
              }

              if (!success)
              {
                tmp.prototypeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WorldCreatorUnityBridge/Content/Standard Assets/Environment/TerrainAssets/BillboardTextures/GrassFrond01AlbedoAlpha.psd");

                if (tmp.prototypeTexture == null)
                {
                  tmp.prototypeTexture = Texture2D.whiteTexture;
                }

                tmp.renderMode = DetailRenderMode.GrassBillboard;
                Debug.Log("Failed to load Detail Asset");
              }

              prototypes.Add(tmp);

              count++;
            }
          }
          var prototypeArray = prototypes.ToArray();



          int layerIndex = 0;
          foreach (XmlElement layer in layers.ChildNodes)
          {
            foreach (XmlElement detail in layer.ChildNodes)
            {
              var filename = detail.Attributes["FileName"].Value;
              var file = new Texture2D(2, 2);
              file.LoadImage(File.ReadAllBytes(directory + "/" + filename));
              file.Apply();

              var pixels = file.GetPixels();
              var data = new int[realResX, realResY];

              float density;
              if (!float.TryParse(detail.Attributes["Density"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out density))
                density = 1;

              heightScaleOff = (float)file.width / realResX;
              widthScaleOff = (float)file.height / realResY;

              for (int y = 0; y < realResY; y++)
              {
                for (int x = 0; x < realResX; x++)
                {
                  int xIndex = Mathf.FloorToInt((file.width - 1) - x * heightScaleOff);
                  int yIndex = Mathf.FloorToInt((file.height - 1) - y * widthScaleOff);
                  if (!settings.ClampValues && (xIndex < 0 || yIndex < 0))
                  {
                    data[x, y] = 0;
                  }
                  else
                  {
                    if (settings.ClampValues)
                    {
                      xIndex = Mathf.Clamp(xIndex, 0, file.width - 1);
                      yIndex = Mathf.Clamp(yIndex, 0, file.height - 1);
                    }

                    int pixelIndex = yIndex * file.width + xIndex;
                    data[x, y] = Mathf.RoundToInt(16 * pixels[pixelIndex].r * density);
                  }
                }
              }

              int[,] splitData = new int[splitRes, splitRes];
              for (int yP = 0; yP < yParts; yP++)
              {
                for (int xP = 0; xP < xParts; xP++)
                {
                  var terrainData = parts[xP, yP].terrainData;
                  terrainData.detailPrototypes = prototypeArray;

                  int xOff = xP * splitRes;
                  int yOff = yP * splitRes;

                  for (int y = 0; y < splitRes; y++)
                  {
                    for (int x = 0; x < splitRes; x++)
                    {
                      int realY = yOff + Mathf.CeilToInt(y * ((float)(splitRes + 1) / splitRes));
                      int realX = xOff + Mathf.CeilToInt(x * ((float)(splitRes + 1) / splitRes));

                      if (!settings.ClampValues)
                      {
                        if (realX >= realResX || realY >= realResY)
                          splitData[y, x] = 0;
                        else
                          splitData[y, x] = data[realX, realY];
                      }
                      else
                      {
                        if (realX >= data.GetLength(0))
                          realX = data.GetLength(0) - 1;
                        if (realY >= data.GetLength(1))
                          realY = data.GetLength(1) - 1;

                        splitData[y, x] = data[realX, realY];
                      }
                    }
                  }
                  terrainData.SetDetailLayer(0, 0, layerIndex, splitData);
                }
              }
              layerIndex += 1;
            }
          }
        }
      }


      // Load Objects
      ///////////////
      if (settings.IsImportObjects)
      {
        var objectElements = doc.GetElementsByTagName("Objects");
        if (objectElements.Count > 0 && objectElements[0].ChildNodes.Count > 0)
        {
          XmlNode objects = objectElements[0];
          XmlNode layers = objects.ChildNodes[0];
          List<ObjectSettings> objectSettings = new List<ObjectSettings>();
          Queue<GameObject> gameObjectPrefabs = new Queue<GameObject>();

          List<string> objectNames = new List<string>();

          // Add tree prototypes for unity tree system
          ////////////////////////////////////////////          
          int treeCount = 0;
          List<TreePrototype> treePrototypes = new List<TreePrototype>();
          foreach (XmlElement layer in layers)
          {
            string layerName = layer.Attributes["Name"].Value;
            foreach (XmlElement obj in layer.ChildNodes)
            {
              string assetName = obj.Attributes["MeshAssetName"].Value;
              string assetFileName = obj.Attributes["MeshAssetFileName"].Value;
              var assetFolder = Path.GetDirectoryName(settings.bridgeFilePath) + "/Assets/" + assetFileName;
              GameObject prefab = null;

              ObjectSettings objSettings = new ObjectSettings();
              if (settings.ObjectSettings != null && settings.ObjectSettings.Length > treeCount)
                objSettings = settings.ObjectSettings[treeCount];

              try
              {
                if (Directory.Exists(assetFolder))
                {
                  prefab = ImportPrefab(assetName, assetFileName, assetFolder, assetsDirectory, objSettings.NatureShader, objSettings.ImportLOD);
                }
              }
              catch (Exception e)
              {
                Debug.Log(e);
              }


              if (prefab == null)
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WorldCreatorSyncTool/Content/Standard Assets/Environment/SpeedTree/Conifer/Conifer_Desktop.spm");

              if (objSettings.Prefab == null)
                objSettings.Prefab = prefab;

              objSettings.Name = assetName;
              objSettings.LayerName = layerName;
              objectSettings.Add(objSettings);
              if (objSettings.CreateGameObjects)
              {
                gameObjectPrefabs.Enqueue(objSettings.Prefab);
              }
              else
              {
                TreePrototype tmp = new TreePrototype();
                tmp.prefab = prefab;
                treePrototypes.Add(tmp);
              }
              treeCount++;
            }
          }
          settings.ObjectSettings = objectSettings.ToArray();
          var treePrototypeArray = treePrototypes.ToArray();


          // Set all tree prototypes
          //////////////////////////
          foreach (var part in parts)
          {
            part.terrainData.treeInstances = new TreeInstance[0];
            part.terrainData.treePrototypes = treePrototypeArray;
          }


          int index = 0;
          int prototypeIndex = 0;
          foreach (XmlElement layer in layers)
          {
            foreach (XmlElement obj in layer.ChildNodes)
            {
              var objName = obj.Attributes["Name"].Value;
              var objTag = obj.Attributes["Tag"].Value;
              var objHeightOffset = obj.Attributes["HeightOffset"].Value;


              var dataString = obj.InnerText;
              var dataCountString = obj.ChildNodes[0].Attributes["DataCount"].Value;
              int dataCount;

              bool createGameObject = false;
              if (settings.ObjectSettings != null && settings.ObjectSettings.Length > index)
                createGameObject = settings.ObjectSettings[index].CreateGameObjects;

              if (int.TryParse(dataCountString, out dataCount))
              {
                var data = Convert.FromBase64String(dataString);
                int objectSize = Marshal.SizeOf(typeof(ObjectData));
                var newBuffer = Marshal.AllocHGlobal(objectSize);
                var objectData = new ObjectData[dataCount];
                for (int i = 0; i < dataCount; i++)
                {
                  Marshal.Copy(data, objectSize * i, newBuffer, objectSize);
                  objectData[i] = (ObjectData)Marshal.PtrToStructure(newBuffer, typeof(ObjectData));
                }
                Marshal.FreeHGlobal(newBuffer);

                if (createGameObject)
                {
                  var prefab = gameObjectPrefabs.Dequeue();

                  for (int i = 0; i < dataCount; i++)
                  {
                    var d = objectData[i];
                    var treePos = d.Position * settings.WorldScale;

                    int indexX = Mathf.Clamp(Mathf.FloorToInt(treePos.x / realLength), 0, parts.GetLength(0) - 1);
                    int indexY = Mathf.Clamp(Mathf.FloorToInt(treePos.z / realLength), 0, parts.GetLength(1) - 1);
                    var terrainComponent = parts[indexX, indexY];
                    var terrainData = terrainComponent.terrainData;
                    var c = terrainComponent.transform.Find("WC_ObjectContainer");
                    var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                    treePos.y = terrainData.GetInterpolatedHeight((treePos.x - indexX * realLength) / terrainData.size.x,
                      (treePos.z - indexY * realLength) / terrainData.size.z);
                    instance.transform.parent = c;
                    instance.transform.position = treePos;
                    instance.transform.localPosition = new Vector3(instance.transform.localPosition.x, treePos.y, instance.transform.localPosition.z);
                    float w = d.Size.x * settings.WorldScale;
                    instance.transform.localScale = new Vector3(w, d.Size.y * settings.WorldScale, w);
                    instance.transform.rotation = d.Rotation;
                  }
                }
                else
                {
                  for (int i = 0; i < dataCount; i++)
                  {
                    var d = objectData[i];
                    var treePos = d.Position;

                    int indexX = Mathf.Clamp(Mathf.FloorToInt(treePos.x / realLength), 0, parts.GetLength(0) - 1);
                    int indexY = Mathf.Clamp(Mathf.FloorToInt(treePos.z / realLength), 0, parts.GetLength(1) - 1);
                    var terrainComponent = parts[indexX, indexY];
                    var terrainData = terrainComponent.terrainData;
                    treePos.x = (treePos.x - indexX * realLength) / terrainData.size.x;
                    treePos.y /= terrainData.size.y;
                    treePos.z = (treePos.z - indexY * realLength) / terrainData.size.z;

                    var instance = new TreeInstance();
                    instance.position = treePos * settings.WorldScale;
                    instance.widthScale = d.Size.x * settings.WorldScale;
                    instance.heightScale = d.Size.y * settings.WorldScale;
                    var quat = d.Rotation;
                    instance.rotation = quat.eulerAngles.y;
                    instance.prototypeIndex = prototypeIndex;
                    instance.color = d.Color;
                    terrainComponent.AddTreeInstance(instance);
                  }

                  prototypeIndex++;
                }
              }
              index++;
            }
          }
        }
      }

      // Finish terrain
      /////////////////
      foreach (var c in newTerrainGameObject.transform.GetComponentsInChildren<Terrain>())
      {
        c.Flush();
      }

      return newTerrainGameObject;
    }

    private static Texture2D ImportNormal(string path)
    {
      var normalImporter = TextureImporter.GetAtPath(path) as TextureImporter;
      if (normalImporter != null)
      {
        normalImporter.textureType = TextureImporterType.NormalMap;
        normalImporter.SaveAndReimport();
      }
      return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    public static GameObject ImportPrefab(string name, string folderName, string path, string assetsDir, bool natureShader, bool importLOD)
    {
      //Util.CopyDirectory(path, assetsDir);
      //AssetDatabase.Refresh();
      var assetDir = assetsDir + folderName + "/";
      var assetPath = assetDir + folderName + ".prefab";
      GameObject gameObj = new GameObject(name);

      XmlDocument doc = new XmlDocument();
      doc.Load(path + "/description.xml");

      bool infiniteDisplay = doc.SelectSingleNode($"/WorldCreator/Mesh/@InfiniteDisplay").Value.ToBoolean();
      float viewDistance = doc.SelectSingleNode($"/WorldCreator/Mesh/@ViewDistance").Value.ToFloat();
      float scaleFactor = doc.SelectSingleNode($"/WorldCreator/Mesh/@ScaleFactor").Value.ToFloat();
      var rotationVec = -doc.SelectSingleNode($"/WorldCreator/Mesh/@Rotation").Value.ToVector3(Vector3.zero);
      var transformRot = Quaternion.Euler(rotationVec.x, rotationVec.y, rotationVec.z);


      List<LOD> renderers = new List<LOD>();
      for (int i = 0; i < (importLOD ? 3 : 1); i++)
      {
        int subsetIndex = 0;
        var lodObj = new GameObject("LOD" + i);
        lodObj.transform.parent = gameObj.transform;

        var bounds = new Bounds();
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Material> materials = new List<Material>();

        var lodNode = doc.SelectSingleNode($"/WorldCreator/Mesh/LOD{i}");
        var billboard = lodNode.SelectSingleNode("@Billboard").Value.ToBoolean();
        var posOffset = lodNode.SelectSingleNode("@PositionOffset").Value.ToVector3(Vector3.zero);

        if (billboard)
        {
          var billboardDiff = AssetDatabase.LoadAssetAtPath<Texture2D>(assetDir + $"billboard_diffuse_{i}.png");
          var billboardNormal = ImportNormal(assetDir + $"billboard_normal_{i}.png");

          var bbrenderer = lodObj.AddComponent<BillboardRenderer>();
          bbrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
          bbrenderer.receiveShadows = true;
          var bba = new BillboardAsset();
          bbrenderer.billboard = bba;

          bba.SetVertices(new Vector2[]
          {
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, 0),
            new Vector2(0, 1)
          });
          bba.SetIndices(new ushort[] { 0, 1, 2, 3, 0, 2 });
          bba.SetImageTexCoords(new Vector4[]
          {
            new Vector4(0,0,1,1)
          });

          bba.width = bba.height = scaleFactor;

          var mat = new Material(Shader.Find("Nature/SpeedTree Billboard"));
          mat.SetFloat("_Cutoff", 0.79f);
          mat.SetFloat("_NormalMapKwToggle", 1);
          mat.EnableKeyword("EFFECT_BUMP");
          mat.mainTexture = billboardDiff;
          mat.SetTexture("_BumpMap", billboardNormal);
          AssetDatabase.CreateAsset(mat, assetDir + $"billboard_mat_{i}.mat");
          AssetDatabase.CreateAsset(bba, assetDir + $"billboard_{i}.asset");

          bba.material = mat;
          renderers.Add(new LOD(0.5f / (i + 1), new[] { bbrenderer }));
          break;
        }


        foreach (XmlNode subset in doc.SelectNodes($"/WorldCreator/Mesh/LOD{i}/Subset"))
        {
          var modelFilePath = subset.SelectSingleNode("@File")?.Value;
          var meshIndexString = subset.SelectSingleNode("@MeshIndex")?.Value;
          var meshName = subset.SelectSingleNode("@MeshName")?.Value;
          var submeshIndexString = subset.SelectSingleNode("@SubsetIndex")?.Value;
          if (!string.IsNullOrEmpty(meshIndexString) && int.TryParse(meshIndexString, out var meshIndex) && int.TryParse(submeshIndexString, out var submeshIndex))
          {
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetDir + modelFilePath);
            MeshFilter mesh = null;
            foreach (var o in objects)
            {
              if (mesh == null && o is MeshFilter m)
              {
                if (meshName == m.sharedMesh.name)
                  mesh = m;
              }
              else if (o is MeshRenderer mr)
              {
                bounds.Encapsulate(mr.bounds);
              }
            }

            if (mesh == null)
            {
              Debug.Log("Failed to load model subset.");
              continue;
            }


            mesh.transform.rotation = transformRot;

            combineInstances.Add(new CombineInstance()
            {
              mesh = mesh.sharedMesh,
              subMeshIndex = submeshIndex,
              transform = mesh.transform.localToWorldMatrix
            });

            var diffuse = subset.SelectSingleNode("Textures/Diffuse/@File")?.Value;
            var normal = subset.SelectSingleNode("Textures/Normal/@File")?.Value;
            var disp = subset.SelectSingleNode("Textures/Displacement/@File")?.Value;

            var alphaTest = subset.SelectSingleNode("@AlphaTest").Value.ToBoolean();
            var doubleSided = subset.SelectSingleNode("@DoubleSided").Value.ToBoolean();
            var alphaCutoff = subset.SelectSingleNode("@AlphaCutoff").Value.ToFloat();
            var metallic = subset.SelectSingleNode("@Metallic").Value.ToFloat();
            var smoothness = subset.SelectSingleNode("@Smoothness").Value.ToFloat();
            var scale = subset.SelectSingleNode("@Scale").Value.ToVector3(Vector3.one);

            // 0 = static | 1 = branch | 2 = leaf
            var type = subset.SelectSingleNode("@GeometryType").Value.ToInt();


            Texture2D diffuseTexture = null;
            Texture2D normalTexture = null;
            Texture2D dispTexture = null;


            if (!string.IsNullOrEmpty(diffuse) && File.Exists(assetDir + diffuse))
              diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetDir + diffuse);
            if (!string.IsNullOrEmpty(normal) && File.Exists(assetDir + normal))
              normalTexture = ImportNormal(assetDir + normal);
            if (!string.IsNullOrEmpty(disp) && File.Exists(assetDir + disp))
              dispTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetDir + disp);


            Material mat;
            var matPath = assetDir + $"/mat_lod{i}_subset{subsetIndex}.mat";
            if (natureShader)
            {
              mat = new Material(Shader.Find("Nature/SpeedTree8"));
              mat.SetFloat("_Cutoff", alphaCutoff);
              mat.SetFloat("_Glossiness", smoothness);
              mat.SetFloat("_Metallic", metallic);
              mat.SetFloat("_NormalMapKwToggle", 1);
              mat.SetInt("_TwoSided", doubleSided ? 0 : 2);
              mat.EnableKeyword("EFFECT_BUMP");
            }
            else
            {
              mat = new Material(Shader.Find("Standard"));
              mat.SetFloat("_Metallic", metallic);
              mat.SetFloat("_Smoothness", smoothness);
              mat.SetFloat("_AlphaCutoff", alphaCutoff);
              mat.SetInt("_Cull", doubleSided ? 0 : 2);
              mat.EnableKeyword("_NORMALMAP");

              if (alphaTest)
              {
                mat.SetOverrideTag("RenderType", "TransparentCutout");
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
              }
              else
              {
                mat.SetOverrideTag("RenderType", "");
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
              }
            }

            mat.mainTexture = diffuseTexture;
            mat.SetTexture("_BumpMap", normalTexture);
            AssetDatabase.CreateAsset(mat, matPath);


            materials.Add(mat);

            subsetIndex++;
          }
          else
          {
            Debug.Log("Failed to load model subset.");
          }
        }

        float max = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
        float prefabScale = scaleFactor / max;
        for (int j = 0; j < combineInstances.Count; j++)
        {
          var instance = combineInstances[j];
          instance.transform = Matrix4x4.Scale(new Vector3(prefabScale, prefabScale, prefabScale)) * instance.transform;
          combineInstances[j] = instance;
        }

        if (combineInstances.Count == 0)
        {
          continue;
        }
        var combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances.ToArray(), false, true);
        AssetDatabase.CreateAsset(combinedMesh, assetDir + $"/combined_mesh_lod{i}.mesh");
        lodObj.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
        var renderer = lodObj.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = materials.ToArray();
        renderers.Add(new LOD(0.5f / (i + 1), new[] { renderer }));
      }



      if (infiniteDisplay)
        renderers[renderers.Count - 1] = new LOD(0, renderers[renderers.Count - 1].renderers);


      if (importLOD)
      {
        const float minLod = 0.05f;
        const float maxLod = 0.5f;
        for (int i = 0; i < renderers.Count; i++)
        {
          renderers[i] = new LOD(Mathf.Lerp(maxLod, minLod, (float)i / (renderers.Count - 1)), renderers[i].renderers);
        }
      }
      else
        renderers[0] = new LOD(0, renderers[0].renderers);

      var lod = gameObj.AddComponent<LODGroup>();
      lod.RecalculateBounds();
      lod.SetLODs(renderers.ToArray());
      lod.fadeMode = LODFadeMode.CrossFade;
      lod.animateCrossFading = true;


      var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(gameObj, assetPath, InteractionMode.AutomatedAction);
      AssetDatabase.Refresh();
      GameObject.DestroyImmediate(gameObj);

      return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
    }


    #endregion

    /// <summary>
    ///   Creates an instance of Vector3 from the specified string.
    /// </summary>
    /// <param name="value">A string that encodes a Vector3.</param>
    /// <returns>A new instance of Vector3.</returns>
    public static Vector2 Vector2FromString(string value)
    {
      var parts = value.Replace("(", "").Replace(")", "").Split(',');
      var v = new Vector2();
      try
      {
        v.x = float.Parse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        v.y = float.Parse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (Exception)
      {
        Debug.Log("Vector parse failed");
      }
      return v;
    }

    /// <summary>
    /// Loads a tga from the given filepath
    /// </summary>
    /// <param name="path"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Vector4[] LoadTGA(string path, out int width, out int height)
    {
      using (var fileStream = File.OpenRead(path))
      {
        using (var reader = new BinaryReader(fileStream))
        {
          reader.BaseStream.Seek(12, SeekOrigin.Begin);
          width = reader.ReadInt16();
          height = reader.ReadInt16();
          int bitDepth = reader.ReadByte();
          reader.BaseStream.Seek(1, SeekOrigin.Current);

          int size = width * height;
          Vector4[] textureData = new Vector4[size];
          const float invByte = 1.0f / 255.0f;
          if (bitDepth == 32)
          {
            for (int i = 0; i < size; i++)
            {
              textureData[i] = new Vector4(reader.ReadByte() * invByte,
                                           reader.ReadByte() * invByte,
                                           reader.ReadByte() * invByte,
                                           reader.ReadByte() * invByte);
            }
          }
          else if (bitDepth == 24)
          {
            for (int i = 0; i < size; i++)
            {
              textureData[i] = new Vector4(
                reader.ReadByte() * invByte,
                reader.ReadByte() * invByte,
                reader.ReadByte() * invByte, 1);
            }
          }
          else
            return null;
          return textureData;
        }
      }
    }
  }
}

#endif