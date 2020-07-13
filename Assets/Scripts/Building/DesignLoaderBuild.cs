﻿using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using Utility;

namespace Building
{
    public class DesignLoaderBuild : MonoBehaviour
    {
        private readonly Dictionary<string, BuildObjectComponent> _partComponentDict =
            new Dictionary<string, BuildObjectComponent>();

        [SerializeField] private List<BuildObjectComponent> partList; // list of possible parts
        [SerializeField] private BuildObjectComponent rootCube; // starting cube if new robot

        [CanBeNull] public string currentRobotName;

        public void Start()
        {
            var buildData = FindObjectOfType<CrossSceneDataContainer>();
            currentRobotName = (string) buildData.data["robotName"];

            LoadComponentListIntoDict();
            CreateParts();
        }

        private void LoadComponentListIntoDict()
        {
            while (partList.Count > 0)
            {
                var component = partList[0];
                partList.RemoveAt(0);

                _partComponentDict.Add(component.name, component);
            }
        }

        private void CreateParts()
        {
            RobotStructure structure;
            
            if (currentRobotName == null)
                structure = RobotStructure.FromSingleBuildComponent(currentRobotName, rootCube);
            else 
                structure = SystemUtility.GetRobotStructureByName(currentRobotName);

            foreach (var part in structure.parts)
            {
                var obj = Instantiate(_partComponentDict[part.part].gameObject, transform);
                obj.transform.localPosition = part.position;
                obj.transform.localRotation = part.rotation;

                obj.GetComponent<BuildObjectComponent>().LoadInfoFromPartDescription(part);
            }
        }
    }
}