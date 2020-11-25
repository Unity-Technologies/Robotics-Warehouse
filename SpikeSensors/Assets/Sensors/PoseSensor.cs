using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSGeometry;

public class PoseSensor : IntervalPublisher
{
    public string topic;

    public override void DoPublish()
    {
        ros.Send(topic, transform.To<FLU>());
    }
}
