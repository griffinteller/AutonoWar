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

        private readonly List<BuildObjectComponent> _parts = new List<BuildObjectComponent>();
        
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

        private void LoadParts()
        {

            foreach (Transform child in transform)
            {
                
                _parts.Add(child.GetComponent<BuildObjectComponent>());
                
            }
            
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
            LoadParts();
            
            var structure = new RobotStructure(_parts, robotCenterOfMass);
            var structureJson = JsonUtility.ToJson(structure);
            
            var file = new StreamWriter(SystemUtility.GetAndCreateRobotsDirectory() + RobotFileName);
            
            file.Write(structureJson);
            file.Close();

        }
        
        public static RobotStructure GetRobotStructure()
        {
            
            var fileReader = new StreamReader(
                SystemUtility.GetAndCreateRobotsDirectory() + BuildHandler.RobotFileName);

            var json = fileReader.ReadToEnd();
            var structure = JsonUtility.FromJson<RobotStructure>(json);

            fileReader.Close();

            return structure;

        }

    }

}
