using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;

public class RobotPlacementTag : RandomizerTag
{
    public void PlaceRobot(Vector3 position)
    {
        gameObject.transform.Find("base_footprint").Find("base_link")
            .GetComponent<ArticulationBody>()
            .TeleportRoot(position, Quaternion.identity);
    }
    
}
