using System;
using System.Collections.Generic;
using Building;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Main
{
    public class DesignLoaderPlay : MonoBehaviour
    {
        private const float ColliderBuffer = 0.3f;

        private readonly Dictionary<string, GameObject> _gamePartsDict = new Dictionary<string, GameObject>();

        private Rigidbody _robotRigidbody;
        [SerializeField] private ActionHandler actionHandler;
        [SerializeField] private List<GameObject> gamePartList;

        [SerializeField] private RobotMain robotMain;
        [SerializeField] private Transform structureRoot;
        [SerializeField] private Transform tireRoot;

        [PunRPC]
        public void BuildRobotRpc(object data)
        {
            var structure = JsonUtility.FromJson<RobotStructure>((string) data);
            BuildRobot(structure);
        }

        public void BuildRobot(RobotStructure structure = null)
        {
            LoadPartListIntoDict();
            _robotRigidbody = GetComponent<Rigidbody>();

            if (structure == null)
                structure = SystemUtility.GetSelectedRobotStructure();

            CreateParts(structure);

            robotMain.OnPartsLoaded();
        }

        private void LoadPartListIntoDict()
        {
            while (gamePartList.Count > 0)
            {
                var part = gamePartList[0];
                gamePartList.RemoveAt(0);

                _gamePartsDict.Add(part.name, part);
            }
        }

        private void CreateParts(RobotStructure structure)
        {
            var tires = new List<RobotPart>();
            foreach (var part in structure.parts)
                switch (part.type)
                {
                    case PartType.Structural:

                        AddStructuralPart(part);
                        break;

                    case PartType.Tire:

                        tires.Add(part);
                        break;

                    default:

                        throw new NotImplementedException();
                }
            
            foreach (var tire in tires) // so suspension can be set correctly
                AddTirePart(tire);
        }

        private void AddStructuralPart(RobotPart part)
        {
            var obj = Instantiate(_gamePartsDict[part.part], structureRoot);
            obj.transform.localPosition = part.position;
            obj.transform.localRotation = part.rotation;
            _robotRigidbody.mass += part.mass;
        }

        private void AddTirePart(RobotPart part)
        {
            AddTireMesh(part);
            _robotRigidbody.mass += part.mass;
        }

        private void AddTireMesh(RobotPart part)
        {
            var tireObjectPrefab = _gamePartsDict[part.part];

            var tireObjectInstance = Instantiate(tireObjectPrefab, tireRoot);
            tireObjectInstance.name = part.name;
            tireObjectInstance.transform.localPosition = part.position;
            tireObjectInstance.transform.localRotation = part.rotation;
        }
    }
}