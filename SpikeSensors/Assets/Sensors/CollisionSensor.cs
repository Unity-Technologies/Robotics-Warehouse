using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSGeometry;

public class CollisionSensor : MonoBehaviour
{
    public ROSConnection ros;
    public string topic;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Floor"))
            return;

        ros.Send(topic, (RosMessageTypes.Geometry.Point32)collision.contacts[0].point.To<FLU>());
    }
}
