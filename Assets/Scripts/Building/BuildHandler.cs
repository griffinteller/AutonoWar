using System.Collections.Generic;
using System.IO;
using UI;
using UnityEngine;
using Utility;

namespace Building
{
    public class BuildHandler : MonoBehaviour
    {

        private const string RobotFileName = "robot.json";
        
        public bool interactable; // can the player currently place blocks?
        public TireNameInputHandler tireNameInputHandler;
        public GameObject currentItemPrefab;

        private BuildObjectComponent _currentItemBuildComponent;
        private GameObject _objectLastClickedOn;
        
        public void Start()
        {

            SetCurrentObject(currentItemPrefab); // updates the build component reference

        }

        public void SetCurrentObject(GameObject prefab)
        {

            currentItemPrefab = prefab;
            _currentItemBuildComponent = currentItemPrefab.GetComponent<BuildObjectComponent>();
            
        }

        public void Update()
        {

            if (!interactable)
                return;

            if (!Input.GetKey(KeyCode.LeftShift))
                return;

            if (Input.GetMouseButtonDown(0))
            {
                RegisterMouseDown(); // we want to make sure we don't drag and then place.
            }

            if (Input.GetMouseButtonUp(0))
            {
                PlaceItemIfPossible();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                RemoveItemIfPossible();
            }

        }

        private void RegisterMouseDown()
        {

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
            {

                _objectLastClickedOn = hit.collider.gameObject;

            }

        }

        private void PlaceItemIfPossible()
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!BuildObjectRaycast(mouseRay, out var hitInfo, out var hitBuildComponent))
                return;

            if (_objectLastClickedOn && 
                hitInfo.collider.gameObject.GetInstanceID() != _objectLastClickedOn.GetInstanceID())
                return;

            var connection = hitBuildComponent.GetConnection(hitInfo.point);
            
            if (!connection)
                return;  // in deadZone or on illegal face

            // we can now be sure we are placing a new item
            
            var newObjectRotation = 
                Quaternion.FromToRotation(_currentItemBuildComponent.GetConnectingFaceOutwardsDirection(),
                -connection.outwardsDirection); // rotate the item so that the two connecting faces are facing each other
            
            var newObjectCenter = connection.connectionCenter;
            newObjectCenter += _currentItemBuildComponent.GetRadius() * connection.outwardsDirection;
            //displaces by radius of the object
            
            // is this a tire, and if it is, name it
            
             var obj = Instantiate(currentItemPrefab, newObjectCenter, newObjectRotation, transform);
             var instantiatedBuildComponent = obj.GetComponent<BuildObjectComponent>();

             var possibleTireComponent = instantiatedBuildComponent as BuildTireComponent;
            if (possibleTireComponent)
            {
                Debug.Log("Naming!");
                tireNameInputHandler.ShowInputWindow(possibleTireComponent);
            }
        }

        private List<BuildObjectComponent> LoadParts()
        {
            var parts = new List<BuildObjectComponent>();

            foreach (Transform child in transform)
            {
                parts.Add(child.GetComponent<BuildObjectComponent>());
            }

            return parts;
        }

        private void RemoveItemIfPossible()
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!BuildObjectRaycast(mouseRay, out var hitInfo, out var bc) || !bc.removable) 
                return;

            Destroy(hitInfo.transform.gameObject);
        }

        private static bool BuildObjectRaycast(Ray ray, out RaycastHit hitInfo, out BuildObjectComponent buildComponent)
        {

            if (!Physics.Raycast(ray, out var hit))
            {
                buildComponent = null;
                hitInfo = hit;
                return false;
            }

            var hitObject = hit.transform.gameObject;
            var hitBuildComponent = hitObject.GetComponent<BuildObjectComponent>();

            if (!hitBuildComponent)
            {
                buildComponent = null;
                hitInfo = hit;
                return false;
            }

            buildComponent = hitBuildComponent;
            hitInfo = hit;
            return true;
            
        }
        
        private Vector3 GetWorldCenterOfMass()
        {

            var rigidbody = gameObject.AddComponent<Rigidbody>();
            var com = rigidbody.centerOfMass;
            com = transform.TransformPoint(com);
            Destroy(rigidbody);
            return com;

        }

        public void SaveDesign()
        {
            var robotCenterOfMass = GetWorldCenterOfMass();
            var parts = LoadParts();
            
            var structure = new RobotStructure(parts, robotCenterOfMass);
            var structureJson = JsonUtility.ToJson(structure);
            
            var file = new FileStream(SystemUtility.GetAndCreateRobotsDirectory() + RobotFileName, FileMode.Create,
                FileAccess.Write);
            var writer = new StreamWriter(file);
            
            writer.Write(structureJson);
            writer.Close();
        }
        
        public static RobotStructure GetRobotStructure()
        {
            var file = new FileStream(SystemUtility.GetAndCreateRobotsDirectory() + RobotFileName, 
                FileMode.Open,
                FileAccess.Read);
            
            var fileReader = new StreamReader(file);

            var json = fileReader.ReadToEnd();
            var structure = JsonUtility.FromJson<RobotStructure>(json);

            fileReader.Close();

            return structure;
        }

    }

}
