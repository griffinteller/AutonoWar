using System;
using UnityEngine;

namespace Main.Building
{
    public class BuildTireComponent : BuildObjectComponent
    {
        public override ConnectionDescription GetConnection(Vector3 hitPointWorldSpace)
        {
            return null;
        }

        public override Vector3 GetConnectingFaceOutwardsDirection()
        {
            
            return Vector3.back;
            
        }
    }
}