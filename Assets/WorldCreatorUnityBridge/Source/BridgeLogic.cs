// Project: WorldCreatorUnityBridge
// Filename: BridgeLogic.cs
// Copyright (c) 2019 BiteTheBytes GmbH. All rights reserved
// *********************************************************

#region Using
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

#endregion

#if UNITY_EDITOR

namespace Assets.WorldCreatorUnityBridge.Source
{
    class BridgeLogic
    {
        #region Fields (Static / Public)

        /// <summary>
        ///   The path to the project folder.
        /// </summary>
        public string projectFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/WorldCreator/Bridge/";

        #endregion

        #region Methods (Public)

        /// <summary>
        ///   Updates the logic, e.g. auto synchronization.
        /// </summary>
        public void Update()
        {
        }

        /// <summary>
        ///   Opens a window with which you can select a project.
        ///   Returns null if the user pressed cancel.
        /// </summary>
        public void SelectProjectFolder(BridgeSettings settings)
        {
            string path = settings.bridgeFilePath;
            if( path == null)
            {
                path = projectFolderPath;
            }

            path = EditorUtility.OpenFilePanel("Select World Creator Bridge XML File", path, "xml");

            if (path != null && path.Length > 0)
            {
                settings.bridgeFilePath = path;
            }

        }

        /// <summary>
        ///   Synchronize/Update the terrain.
        /// </summary>
        public void Synchronize(BridgeSettings settings)
        {
            if (!settings.IsBridgeFileValid())
                return;

            // First copy the contents of the World Creator project folder over to the Unity folder and import them
            UnityTerrainUtility.CreateTerrainFromFile(settings);
        }

        #endregion
    }
}

#endif