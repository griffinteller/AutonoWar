using System;
using System.Collections.Generic;
using UnityEngine;

namespace Building
{
    [Serializable]
    public enum PartType
    {
        Structural,
        Tire
    }

    [Serializable]
    public class RobotPart
    {
        public int mass;
        public string name;
        public string part;

        public Vector3 position;
        public Quaternion rotation;

        public PartType type;

        public RobotPart(Transform partTransform, Vector3 robotCenterOfMass,
            PartType type, string part, int mass, string name = "__NONE__")
        {
            position = partTransform.position - robotCenterOfMass;
            rotation = partTransform.rotation;
            this.type = type;
            this.part = part;
            this.mass = mass;
            this.name = name;
        }
    }

    [Serializable]
    public class RobotStructure
    {
        public string name;
        public float mass;
        public List<string> tires = new List<string>();
        public RobotPart[] parts;

        public RobotStructure(string name, 
            List<BuildObjectComponent> partsBuildComponents, 
            Vector3 robotCenterOfMass)
        {
            this.name = name;
            parts = new RobotPart[partsBuildComponents.Count];

            for (var i = 0; i < partsBuildComponents.Count; i++)
            {
                parts[i] = partsBuildComponents[i].GetRobotPartDescription(robotCenterOfMass);
                mass += parts[i].mass;
                
                if (parts[i].type == PartType.Tire)
                    tires.Add(parts[i].name);
            }
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static RobotStructure FromJson(string json)
        {
            return JsonUtility.FromJson<RobotStructure>(json);
        }

        public static RobotStructure FromSingleBuildComponent(string name, BuildObjectComponent component)
        {
            var buildComponents = new List<BuildObjectComponent> {component};
            return new RobotStructure(name, buildComponents, Vector3.zero);
        }
    }
}