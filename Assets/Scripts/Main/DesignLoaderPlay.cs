using System;
using System.Collections.Generic;
using System.IO;
using Building;
using Photon.Pun;
using UnityEngine;

namespace Main
{
    public class DesignLoaderPlay : MonoBehaviourPun
    {

        [SerializeField] private ActionHandler actionHandler;
        [SerializeField] private List<GameObject> gamePartList;
        [SerializeField] private Transform structureRoot;
        [SerializeField] private Transform wheelColliderRoot;
        [SerializeField] private Transform tireMeshRoot;
        [SerializeField] private GameObject wheelColliderPrefab;

        private readonly Dictionary<string, GameObject> _gamePartsDict = new Dictionary<string, GameObject>();

        private Rigidbody _robotRigidbody;

        [PunRPC]
        public void BuildRobotRpc(object data)
        {
            LoadPartListIntoDict();
            _robotRigidbody = GetComponent<Rigidbody>();
            var structure = RobotStructure.FromJson((string) data);
            CreateParts(structure);
            actionHandler.LoadTiresIntoDict();
        }

        public void BuildRobotSinglePlayer()
        {
            LoadPartListIntoDict();
            _robotRigidbody = GetComponent<Rigidbody>();
            var structure = BuildHandler.GetRobotStructure();
            CreateParts(structure);
            actionHandler.LoadTiresIntoDict();
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

            foreach (var part in structure.parts)
            {

                switch (part.type)
                {
                    
                    case PartType.Structural:

                        AddStructuralPart(part);
                        break;
                    
                    case PartType.Tire:
                        
                        AddTirePart(part);
                        break;
                    
                    default:
                        
                        throw new NotImplementedException();
                    
                }
                
            }
            
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
            
            var wheelCollider = AddWheelCollider(part);
            AddTireMesh(part, wheelCollider);
            _robotRigidbody.mass += part.mass;

        }

        private WheelCollider AddWheelCollider(RobotPart part)
        {

            var wheelColliderInstance = Instantiate(
                wheelColliderPrefab, wheelColliderRoot);
            wheelColliderInstance.transform.localPosition = part.position;

            wheelColliderInstance.name = part.name;

            var wheelCollider = wheelColliderInstance.GetComponent<WheelCollider>();
            wheelCollider.steerAngle = part.rotation.eulerAngles.y;

            return wheelCollider;

        }

        private void AddTireMesh(RobotPart part, WheelCollider wheelCollider)
        {

            var tireObjectPrefab = _gamePartsDict[part.part];
            
            var tireObjectInstance = Instantiate(
                tireObjectPrefab, tireMeshRoot);
            tireObjectInstance.name = part.name + "Vis";
            tireObjectInstance.transform.localPosition = part.position;
            tireObjectInstance.transform.localRotation = part.rotation;

            var tireMeshAnimator = tireObjectInstance.GetComponent<TireMeshAnimator>();
            tireMeshAnimator.tireMeshRoot = tireMeshRoot;
            tireMeshAnimator.wheelCollider = wheelCollider;

            var tireComponent = tireObjectInstance.GetComponent<TireComponent>();
            tireComponent.WheelCollider = wheelCollider;
            tireComponent.baseSteerAngle = wheelCollider.steerAngle;

        }
        
    }
    
}