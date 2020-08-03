using System.Collections;
using GamePhysics;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode
{
    public class PhysicsTests
    {
        public const string FrictionCurvesPath = "Assets/Scripts/GamePhysics/FrictionCurves/";
        public const string FrictionCurveName = "DefaultCurve" + ".asset";

        [Test]
        public void TestFrictionCurveIntegration()
        {
            var frictionCurve = AssetDatabase.LoadAssetAtPath<FrictionCurve>(FrictionCurvesPath + FrictionCurveName);
            Debug.Log(frictionCurve.integration.GetSlipAfterTime(Time.fixedDeltaTime,
                11, 0, 500, 1, 2));
            Assert.IsTrue(true);
        }
    }
}