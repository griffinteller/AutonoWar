// Project: WorldCreatorUnityBridge
// Filename: BridgeEditor.cs
// Copyright (c) 2019 BiteTheBytes GmbH. All rights reserved
// *********************************************************

#region Using

using System;
using System.Globalization;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

#endregion

#if UNITY_EDITOR

namespace Assets.WorldCreatorUnityBridge.Source
{
  [Serializable]
  public class BridgeEditor : EditorWindow, IHasCustomMenu
  {
    private class ModelPostprocessor : AssetPostprocessor
    {
      public static bool WorldCreatorImportActive = false;
      void OnPreprocessModel()
      {
        if (!WorldCreatorImportActive)
          return;
        ModelImporter modelImporter = assetImporter as ModelImporter;
        modelImporter.importMaterials = false;
      }
    }

    #region Fields (Static / Public)

    /// <summary>
    ///   This window as a static instance.
    /// </summary>
    public static BridgeEditor Window;

    /// <summary>
    /// Contains all settings
    /// </summary>
    public BridgeSettings settings;

    /// <summary>
    /// The World Creator logo.
    /// </summary>
    private Sprite logoWorldCreator;

    /// <summary>
    /// The YouTube logo.
    /// </summary>
    private Sprite logoYouTube;

    /// <summary>
    /// The Facebook logo.
    /// </summary>
    private Sprite logoFacebook;

    /// <summary>
    /// The Twitter logo.
    /// </summary>
    private Sprite logoTwitter;

    /// <summary>
    /// The Google+ logo.
    /// </summary>
    private Sprite logoGooglePlus;

    /// <summary>
    /// The Discord logo.
    /// </summary>
    private Sprite logoDiscord;

    /// <summary>
    /// The Instagram logo.
    /// </summary>
    private Sprite logoInstagram;

    /// <summary>
    /// The Vimeo logo.
    /// </summary>
    private Sprite logoVimeo;

    /// <summary>
    /// The Twitch logo.
    /// </summary>
    private Sprite logoTwitch;

    #endregion

    #region Fields (Private)

    /// <summary>
    ///   The logic behind the UI.
    /// </summary>
    private BridgeLogic logic = new BridgeLogic();

    /// <summary>
    ///   An array of strings that are used to build the toolbar.
    /// </summary>
    private readonly string[] toolbarItems =
    {
      //"General", "Textures", "Objects", "Details", "About"
      "General", "About"
    };

    /// <summary>
    ///   Indicates whether this window is locked.
    /// </summary>
    private bool locked;

    private Vector2 scrollPositionGeneralTab;
    private Vector2 scrollPositionObjects;      
      
    /// <summary>
    ///   The selected toolbar item index.
    /// </summary>
    private int selectedToolbarItemIndex;

    /// <summary>
    ///   The scrollposition of the folder name textfield
    /// </summary>
    private Vector2 folderScrollPosition;
    #endregion

    #region Methods (Public)

    public void Awake()
    {
      LoadSettings();
    }

    #region Settings
    private string GetSettingsDirectory()
    {
      return Application.dataPath + @"/WorldCreatorUnityBridge/Settings";
    }

    private string GetSettingsFilePath()
    {
      return GetSettingsDirectory() + "/BridgeSettings.json";
    }

    private void SaveSettings()
    {
      try
      {
        // ensure the target folder exists
        DirectoryInfo target = new DirectoryInfo(GetSettingsDirectory());

        if (!target.Exists)
        {
          target.Create();
          target.Refresh();
        }

        // create target file path
        string settingsFilePath = GetSettingsFilePath();

        string dataAsJson = JsonUtility.ToJson(settings);
        File.WriteAllText(settingsFilePath, dataAsJson);

        Debug.Log("Saving Bridge Settings: " + settingsFilePath);
      }
      catch (System.Exception ex)
      {
        Debug.Log("Couldn't save settings: " + ex.ToString());
      }
    }

    private void LoadSettings()
    {
      try
      {
        // create source file path
        string settingsFilePath = GetSettingsFilePath();

        Debug.Log("Loading Bridge Settings: " + settingsFilePath);

        if (File.Exists(settingsFilePath))
        {
          string dataAsJson = File.ReadAllText(settingsFilePath);
          settings = JsonUtility.FromJson<BridgeSettings>(dataAsJson);
        }
        else
        {
          settings = new BridgeSettings();
        }

      }
      catch (System.Exception ex)
      {
        Debug.Log("Couldn't load settings: " + ex.ToString());
      }

    }
    #endregion Settings

    /// <summary>
    ///   Updates the logic behind this GUI.
    /// </summary>
    public void Update()
    {
      this.logic.Update();
    }

    /// <summary>
    ///   Draws this windows' GUI.
    /// </summary>
    public void OnGUI()
    {
      // for some reason OnGUI is called earlier than Awake() or Awake() isn't called at all => lazy initialization to make sure we have settings
      if (settings == null)
      {
        LoadSettings();
      }

      EditorGUILayout.BeginVertical("box");
      {
        this.selectedToolbarItemIndex = GUILayout.Toolbar(this.selectedToolbarItemIndex, this.toolbarItems,
          GUILayout.Height(32));
      }
      EditorGUILayout.EndVertical();

      EditorGUILayout.BeginVertical("box");
      {
        scrollPositionGeneralTab = GUILayout.BeginScrollView(scrollPositionGeneralTab);
        {
          switch (this.selectedToolbarItemIndex)
          {
            case 0:
              this.DrawTabGeneral();
              break;

            case 1:
              this.DrawTabAbout();
              break;
          }
        }
        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
      }
      EditorGUILayout.EndVertical();

      // Only show the synchronize button when a project folder has been selected
      if (settings.IsBridgeFileValid())
      {
        if (GUILayout.Button("SYNCHRONIZE", GUILayout.Height(50)))
        {
          if (!File.Exists(settings.bridgeFilePath))
          {
            Debug.LogError("Selected file does not exist.");
            return;
          }

          // Copy the sync folder ...
          var terrainFolder = Application.dataPath + @"/" + settings.terrainsFolderName + "/" + settings.terrainAssetName;
          DirectoryInfo target = new DirectoryInfo(terrainFolder + "/Assets");
          DirectoryInfo source = new DirectoryInfo(settings.bridgeFilePath).Parent;

          if (source != null && source.Parent != null)
            source = new DirectoryInfo(source.FullName + "/Assets/");

          ModelPostprocessor.WorldCreatorImportActive = true;
          AssetDatabase.StartAssetEditing();          
          this.CopyAll(source, target);
          AssetDatabase.StopAssetEditing();
          AssetDatabase.Refresh();
          ModelPostprocessor.WorldCreatorImportActive = false;


          if (settings.deleteUnusedAssets)
          {
            // Build a list of all terrain asset files
            foreach (string num in Directory.GetFiles(@"Assets/" + settings.terrainsFolderName + "/" + settings.terrainAssetName))
            {
              if (num.Contains(settings.terrainAssetName + "_") || num.EndsWith(".mat"))
                AssetDatabase.DeleteAsset(num);
            }
          }

          // Copy colormap
          try
          {
            if (File.Exists(source.Parent.FullName + "/colormap.png"))
              File.Copy(source.Parent.FullName + "/colormap.png", terrainFolder + "/colormap.png", true);
            else
              File.Delete(terrainFolder + "/colormap.png");
          }
          catch (Exception e)
          {
            Debug.Log(e.Message);
          }

          AssetDatabase.Refresh();

          // ... perform synchronization ...
          logic.Synchronize(settings);

          // save the settings for the next time the window is used
          SaveSettings();
        }
      }
    }

    private void CopyAll(DirectoryInfo source, DirectoryInfo target)
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
        CopyAll(diSourceSubDir, nextTargetSubDir);
      }
    }

    /// <summary>
    ///   Locks this window inside the Inspector.
    /// </summary>
    /// <param name="menu">Specified by Unity Engine.</param>
    public void AddItemsToMenu(GenericMenu menu)
    {
      menu.AddItem(new GUIContent("Lock"), locked, () => { locked = !locked; });
    }

    #endregion

    #region Methods (Private)

    /// <summary>
    ///   Draws the contents of the General toolbar selection.
    /// </summary>
    private void DrawTabGeneral()
    {
      GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
      boxStyle.alignment = TextAnchor.MiddleLeft;
      boxStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.81f, 0.77f, 0.67f) : Color.black;
      boxStyle.stretchWidth = true;
      float spacePixels = 8;

      // reset settings button
      EditorGUILayout.BeginHorizontal();
      {
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset Settings", GUILayout.Width(160)))
        {
          settings = new BridgeSettings();
        }
      }
      EditorGUILayout.EndHorizontal();

      GUILayout.Box("Please select the World Creator Bridge XML file that you have exported through the Standalone World Creator application.\n\nThe name of the file is 'Bridge.xml' and can be found in folder:\n\n[USER]/Documents/WorldCreator/Bridge/[PROJECT_NAME]", boxStyle);

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        if (GUILayout.Button("SELECT BRIDGE XML FILE", GUILayout.Height(30)))
        {
          this.logic.SelectProjectFolder(settings);
        }
      }
      GUILayout.EndHorizontal();

      folderScrollPosition = EditorGUILayout.BeginScrollView(folderScrollPosition);
      { 
        string path = settings.IsBridgeFileValid() ? settings.bridgeFilePath : logic.projectFolderPath;

        GUILayout.TextField(path, GUILayout.ExpandWidth(true));
      }
      EditorGUILayout.EndScrollView();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("Specify the name of the terrain asset. The World Creator Bridge will use the name to create a GameObject container that contains your terrain GameObject(s).", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Terrain Asset Name:", GUILayout.Width(160));
        settings.terrainAssetName = GUILayout.TextField(settings.terrainAssetName, GUILayout.ExpandWidth(true));
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("Please enable the checkbox below if you would like the World Creator Bridge to clean up non-used terrain assets.", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Delete unused Assets:", GUILayout.Width(160));
        settings.deleteUnusedAssets = GUILayout.Toggle(settings.deleteUnusedAssets, "");
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("Choose whether to import objects (e.g. trees, rocks).", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Import Objects:", GUILayout.Width(160));
        settings.IsImportObjects = GUILayout.Toggle(settings.IsImportObjects, "");
      }
      GUILayout.EndHorizontal();

      if (settings.IsImportObjects)
      {
        if (settings.ObjectSettings != null)
        {
          if(settings.ObjectSettings.Length > 0)
          {
            GUILayout.BeginHorizontal();
            {
              GUILayout.Box("If your terrain has some objects applied, you might want to adjust them separately.\n\n* PREFAB\nLet's you set a custom object.\n\n* UNITY TREE\nIndicates, whether the imported object should be imported as a Unity Terrain Tree object. If unchecked, GameObject's are created instead.\n\n* NATURE SHADER\nIndicates, whether the Unity Nature Sahder should be used.\n\n* LOD\nEnables LOD typical detail changes and culling.", boxStyle);
            }
            GUILayout.EndHorizontal();

            scrollPositionObjects = GUILayout.BeginScrollView(scrollPositionObjects, GUILayout.Height(150));
            {
              GUILayout.BeginHorizontal();
              {
                string lastLayer = null;
                foreach (var o in settings.ObjectSettings)
                {
                  if (lastLayer != o.LayerName)
                  {
                    if (lastLayer != null)
                    {
                      GUILayout.EndHorizontal();
                      GUILayout.EndVertical();
                    }
                    GUILayout.BeginVertical(boxStyle);
                    GUILayout.Label("Layer: " + o.LayerName);
                    GUILayout.BeginHorizontal();
                    lastLayer = o.LayerName;
                  }

                  const int leftSize = 100;
                  const int rightSize = 156;
                  GUILayout.BeginVertical(boxStyle, GUILayout.Width(leftSize + rightSize));
                  {
                    GUILayout.BeginHorizontal();
                    {
                      GUILayout.Label("Name:", GUILayout.Width(leftSize));
                      GUILayout.FlexibleSpace();
                      GUILayout.Label(o.Name, GUILayout.Width(rightSize));
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                      GUILayout.Label("Prefab:", GUILayout.Width(leftSize));
                      GUILayout.FlexibleSpace();
                      o.Prefab = EditorGUILayout.ObjectField(o.Prefab, typeof(GameObject), false, GUILayout.Width(rightSize)) as GameObject;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                      GUILayout.Label("Unity Tree:", GUILayout.Width(leftSize));
                      GUILayout.FlexibleSpace();
                      o.CreateGameObjects = !GUILayout.Toggle(!o.CreateGameObjects, "", GUILayout.Width(rightSize));
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                      GUILayout.Label("Nature Shader:", GUILayout.Width(leftSize));
                      GUILayout.FlexibleSpace();
                      o.NatureShader = GUILayout.Toggle(o.NatureShader, "", GUILayout.Width(rightSize));
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                      GUILayout.Label("LOD:", GUILayout.Width(leftSize));
                      GUILayout.FlexibleSpace();
                      o.ImportLOD = GUILayout.Toggle(o.ImportLOD, "", GUILayout.Width(rightSize));
                    }
                    GUILayout.EndHorizontal();
                  }
                  GUILayout.EndVertical();
                }
                if (lastLayer != null)
                {
                  GUILayout.EndHorizontal();
                  GUILayout.EndVertical();
                }
              }
              GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
          }
        }
      }

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("Choose whether to import details (e.g. grass, flowers).", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Import Details:", GUILayout.Width(160));
        settings.IsImportDetails = GUILayout.Toggle(settings.IsImportDetails, "");
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("Choose whether to import textures.", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Import Textures:", GUILayout.Width(160));
        settings.IsImportTextures = GUILayout.Toggle(settings.IsImportTextures, "");
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("The Split Threshold specifies when to split the created Unity terrain. This might be important if you want to split your Unity terrain into smaller chunks (e.g. for streaming).", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Split Threshold:", GUILayout.Width(160));
        settings.SplitThreshold = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.SplitThreshold, 0, 5));
        GUILayout.Label((128 << settings.SplitThreshold).ToString(), GUILayout.Width(32));
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("If the terrain size you would like to synchronize does not follow the power-of-two + 1 rule of Unity's terrain system, you can choose to clamp the values.", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Clamp Values:", GUILayout.Width(160));
        settings.ClampValues = GUILayout.Toggle(settings.ClampValues, "");
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("With World Scale you can scale your terrain to a different value.", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
      textFieldStyle.alignment = TextAnchor.MiddleRight;

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("World Scale:", GUILayout.Width(160));

        float oldScale = settings.WorldScale;
        float newScale = GUILayout.HorizontalSlider(settings.WorldScale, 0, 8, GUILayout.ExpandWidth(true));
        if (oldScale != newScale)
        {
          settings.WorldScaleString = newScale.ToString("#0.00");
          settings.WorldScale = newScale;
        }

        var oldString = settings.WorldScaleString;
        settings.WorldScaleString = GUILayout.TextField(settings.WorldScaleString, textFieldStyle, GUILayout.Width(50));
        if (oldString != settings.WorldScaleString)
        {
          float newVal;
          if (float.TryParse(settings.WorldScaleString, NumberStyles.Any, CultureInfo.InvariantCulture, out newVal))
            settings.WorldScale = newVal;
        }
        GUILayout.Label("m", GUILayout.Width(14));
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Box("Select the material type. Use Standard for default 3D Unity projects, HDRP for HDRP Unity projects and LWRP for LWRP Unity projects. If you would like to plugin a custom material, please choose Custom.", boxStyle);
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(spacePixels);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Material Type:", GUILayout.Width(160));
        settings.MaterialType = (MaterialType)EditorGUILayout.EnumPopup(settings.MaterialType, GUILayout.ExpandWidth(true));
      }
      GUILayout.EndHorizontal();

      if (settings.MaterialType == MaterialType.Custom)
      {
        GUILayout.BeginHorizontal();
        {
          settings.CustomMaterial = EditorGUILayout.ObjectField("", settings.CustomMaterial, typeof(Material), false, GUILayout.ExpandWidth(true)) as Material;
        }
        GUILayout.EndHorizontal();
      }
    }

    /// <summary>
    ///   Draws the contents of the About toolbar selection.
    /// </summary>
    private void DrawTabAbout()
    {
      string spritesFolder = @"Assets/WorldCreatorUnityBridge/Content/Sprites/";

      this.logoWorldCreator = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "worldcreator.png" : spritesFolder + "worldcreator_inv.png");
      this.logoYouTube = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "youtube.png" : spritesFolder + "youtube_inv.png");
      this.logoFacebook = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "facebook.png" : spritesFolder + "facebook_inv.png");
      this.logoTwitter = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "twitter.png" : spritesFolder + "twitter_inv.png");
      this.logoGooglePlus = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "googlePlus.png" : spritesFolder + "googlePlus_inv.png");
      this.logoDiscord = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "discord.png" : spritesFolder + "discord_inv.png");
      this.logoInstagram = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "instagram.png" : spritesFolder + "instagram_inv.png");
      this.logoVimeo = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "vimeo.png" : spritesFolder + "vimeo_inv.png");
      this.logoTwitch = AssetDatabase.LoadAssetAtPath<Sprite>(EditorGUIUtility.isProSkin ? spritesFolder + "twitch.png" : spritesFolder + "twitch_inv.png");

      if (this.logoWorldCreator != null)
      {
        if (GUILayout.Button(this.logoWorldCreator.texture))
        {
          Application.OpenURL("https://www.world-creator.com");
        }
      }

      var guiStyleButton = new GUIStyle(GUI.skin.button);
      guiStyleButton.fontSize = 18;
      var styleLegal = new GUIStyle(GUI.skin.box);
      styleLegal.richText = true;
      GUILayoutOption[] guiLayoutOptionsHelpLarge = { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) };

      string color = "#000000";
      if (EditorGUIUtility.isProSkin)
      {
        color = "#D0C6AB";
      }

      GUILayout.Box("<color=" + color + ">\nWorld Creator Bridge for Unity\nVersion 1.1.0 \n</color>", styleLegal, guiLayoutOptionsHelpLarge);

      GUILayout.BeginHorizontal();
      {
        if (GUILayout.Button("COMPANY", guiStyleButton))
        {
          EditorUtility.DisplayDialog
            ("About - Company",
              "BiteTheBytes GmbH\n" + "Mainzer Str. 9\n" + "36039 Fulda\n\n" +
              "Responsible: BiteTheBytes GmbH\n" + "Commercial Register Fulda: HRB 5804\n" +
              "VAT / Ust-IdNr: DE 272746606", "OK");
        }
      }
      GUILayout.EndHorizontal();

      GUILayout.Box
        ("<color=" + color + ">\nJoin our community on DISCORD and follow us on our social sites to get the " +
         "latest information of the World Creator product series.\n\n" +
         "Get into touch with the devs and share your ideas and suggestions.\n</color>", styleLegal, guiLayoutOptionsHelpLarge);

      GUILayout.BeginHorizontal();
      {
        if (this.logoFacebook != null)
          if (GUILayout.Button(this.logoFacebook.texture))
            Application.OpenURL("https://www.facebook.com/worldcreator3d");

        if (this.logoTwitter != null)
          if (GUILayout.Button(this.logoTwitter.texture))
            Application.OpenURL("https://twitter.com/worldcreator3d");

        if (this.logoYouTube != null)
          if (GUILayout.Button(this.logoYouTube.texture))
            Application.OpenURL("https://www.youtube.com/channel/UClabqa6PHVjXzR2Y7s1MP0Q");
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      {
        if (this.logoInstagram != null)
          if (GUILayout.Button(this.logoInstagram.texture))
            Application.OpenURL("https://www.instagram.com/worldcreator3d/");

        if (this.logoVimeo != null)
          if (GUILayout.Button(this.logoVimeo.texture))
            Application.OpenURL("https://vimeo.com/user82114310");

        if (this.logoTwitch != null)
          if (GUILayout.Button(this.logoTwitch.texture))
            Application.OpenURL("https://www.twitch.tv/worldcreator3d");

        if (this.logoDiscord != null)
          if (GUILayout.Button(this.logoDiscord.texture))
            Application.OpenURL("https://discordapp.com/invite/bjMteus");
      }
      GUILayout.EndHorizontal();

      GUILayout.Box
        ("<color=" + color + ">\nVisit our website and join us on Discord - we are always looking for new ideas and suggestions to improve World Creator.\n</color>",
          styleLegal, guiLayoutOptionsHelpLarge);

      GUILayout.BeginHorizontal();
      {
        if (GUILayout.Button("WEBSITE", guiStyleButton))
        {
          Application.OpenURL("https://www.world-creator.com");
        }

        if (GUILayout.Button("DISCORD", guiStyleButton))
          Application.OpenURL("https://discordapp.com/invite/bjMteus");
      }
      GUILayout.EndHorizontal();
    }

    #endregion

    #region Methods (Static / Public)

    /// <summary>
    ///   Initializes this window.
    /// </summary>
    [MenuItem("Window/World Creator to Unity")]
    public static void Init()
    {

      Window = (BridgeEditor)GetWindow(typeof(BridgeEditor));
      Window.autoRepaintOnSceneChange = true;
      Window.minSize = new Vector2(425, 500);
      Window.titleContent = new GUIContent("World Creator to Unity", AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/WorldCreatorUnityBridge/Content/Sprites/icon.png"));
      Window.Show();
    }

    #endregion
  }
}

#endif