using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RosSharp;

//[CustomEditor(typeof(RobotLoader))]
public class RobotLoader : Editor
{
    // Start is called before the first frame update
    [MenuItem("AGV/Load")]
    public static GameObject LoadRobot()
    {
        ImportSettings settings = ImportSettings.DefaultSettings();
        GameObject rob = RosSharp.Urdf.Editor.UrdfRobotExtensions.Create("/Users/vidur.vij/Desktop/Robotics-AMR-Spike/Warehouse/Assets/turtlebot3/model.urdf", settings);
        if (rob == null)
            Debug.Log("nope");
        //Tile Constraint
        GameObject tiltConstraint = new GameObject("TiltConstraint");
        tiltConstraint.transform.parent = rob.transform;
        ConfigurableJoint constraint = tiltConstraint.AddComponent<ConfigurableJoint>();
        constraint.angularXMotion = ConfigurableJointMotion.Locked;
        constraint.angularZMotion = ConfigurableJointMotion.Locked;
        GameObject baseLink = GameObject.Find("base_link");
        constraint.connectedArticulationBody = baseLink.GetComponent<ArticulationBody>();

        //Remove Collisions
        Collider[] casterCollider = GameObject.Find("caster_back_right_link").GetComponentsInChildren<Collider>();
        foreach (Collider col in casterCollider)
            DestroyImmediate(col);

        Collider[] casterCollider2 = GameObject.Find("caster_back_left_link").GetComponentsInChildren<Collider>();
        foreach (Collider col in casterCollider2)
            DestroyImmediate(col);

        //TensorUpdate
        Vector3 inertiaup = new Vector3(1, 1, 1);
        GameObject wheel1 = GameObject.Find("wheel_left_link").gameObject;
        GameObject wheel2 = GameObject.Find("wheel_right_link").gameObject;
        InertiaTensorUpdate wheel1up = wheel1.gameObject.AddComponent<InertiaTensorUpdate>();
        InertiaTensorUpdate wheel2up = wheel2.gameObject.AddComponent<InertiaTensorUpdate>();
        wheel1up.inertiaTensor = wheel2up.inertiaTensor = inertiaup;

        //AddController
        RosSharp.Control.Goal goal = rob.AddComponent<RosSharp.Control.Goal>();
        RosSharp.Control.AGVController controller = rob.AddComponent<RosSharp.Control.AGVController>();
        controller.wheel1 = wheel1;
        controller.wheel2 = wheel2;
        controller.goalFunc = goal;
        controller.centerPoint = baseLink;

        return rob;
    }


}
