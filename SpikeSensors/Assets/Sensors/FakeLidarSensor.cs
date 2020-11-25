using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ROSGeometry;

public class FakeLidarSensor : IntervalPublisher
{
    public string topic;
    List<RosMessageTypes.Geometry.Point32> collectedPoints = new List<RosMessageTypes.Geometry.Point32>();
    List<GameObject> markers = new List<GameObject>();
    List<GameObject> inactiveMarkers = new List<GameObject>();
    public GameObject markerPrefab;
    public override void DoPublish()
    {
        ros.Send(topic, new RosMessageTypes.Sensor.PointCloud(
            new RosMessageTypes.Std.Header(),
            collectedPoints.ToArray(),
            new RosMessageTypes.Sensor.ChannelFloat32[0]
        ));

        foreach (GameObject obj in markers)
        {
            inactiveMarkers.Add(obj);
            obj.SetActive(false);
        }
        markers.Clear();
        collectedPoints.Clear();
    }

    public void FixedUpdate()
    {
        for (float spread = -1; spread <= 1; spread += 0.1f)
        {
            RaycastHit hit;
            if(Physics.Raycast(new Ray(transform.position, transform.forward + transform.right*spread), out hit, 1000))
            {
                collectedPoints.Add(hit.point.To<FLU>());
                if (inactiveMarkers.Count > 0)
                {
                    GameObject inactiveMarker = inactiveMarkers[inactiveMarkers.Count - 1];
                    inactiveMarkers.RemoveAt(inactiveMarkers.Count - 1);
                    inactiveMarker.SetActive(true);
                    inactiveMarker.transform.position = hit.point;
                    markers.Add(inactiveMarker);
                }
                else if (markerPrefab != null)
                {
                    markers.Add(GameObject.Instantiate(markerPrefab, hit.point, Quaternion.identity));
                }
            }
        }
    }
}
