using System.Collections.Generic;
using Building;
using UnityEngine;
using Utility;

namespace UI.MainMenu.RobotDisplay
{
    public class DesignLoaderDisplay : MonoBehaviour
    {
        private readonly Dictionary<string, BuildObjectComponent> _partComponentDict =
            new Dictionary<string, BuildObjectComponent>();

        [SerializeField] private List<BuildObjectComponent> partList; // list of possible parts
        [SerializeField] private BuildObjectComponent rootCube; // starting cube if new robot

        [HideInInspector] public RobotStructure structure;
        public Vector3 maxDimensions = Vector3.one * 2;

        public void Start()
        {
            LoadComponentListIntoDict();
            CreateParts();
            ScaleToFit();
            //SetPivotToGeometricCenter();
        }

        private void SetPivotToGeometricCenter()
        {
            var worldMin = MeshUtility.GetMinWorldPointOfChildMeshes(gameObject);
            var worldMax = MeshUtility.GetMaxWorldPointOfChildMeshes(gameObject);
            var geometricCenter = (worldMax + worldMin) / 2;
            var delta = geometricCenter - transform.position;

            foreach (Transform child in transform)
                child.position -= delta;
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
            if (structure == null)
                structure = SystemUtility.GetSelectedRobotStructure();

            foreach (var part in structure.parts)
            {
                var obj = Instantiate(_partComponentDict[part.part].gameObject, transform);
                obj.transform.localPosition = part.position;
                obj.transform.localRotation = part.rotation;

                obj.GetComponent<BuildObjectComponent>().LoadInfoFromPartDescription(part);
            }
        }
        
        private void ScaleToFit()
        {
            var meshSize = MeshUtility.GetCompoundMeshSize(gameObject);
            var differenceFromDesiredSize = meshSize - maxDimensions;
            var axis = MetaUtility.ArgMax(differenceFromDesiredSize);
            var ratio = maxDimensions[axis] / meshSize[axis];
            transform.localScale *= ratio;
        }
    }
}