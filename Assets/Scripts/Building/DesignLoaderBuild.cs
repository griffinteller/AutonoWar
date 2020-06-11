using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

namespace Building
{
    public class DesignLoaderBuild : MonoBehaviour
    {

        [SerializeField] private List<BuildObjectComponent> partList;  // list of possible parts
        [SerializeField] private GameObject rootCube;  // starting cube if new robot

        private readonly Dictionary<string, BuildObjectComponent> _partComponentDict = 
            new Dictionary<string, BuildObjectComponent>();

        public void Start()
        {
            
            LoadComponentListIntoDict();

            try
            {
                CreateParts();
            }
            catch (IOException)
            {
                Instantiate(rootCube, transform);
            }
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
            var structure = BuildHandler.GetRobotStructure();

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