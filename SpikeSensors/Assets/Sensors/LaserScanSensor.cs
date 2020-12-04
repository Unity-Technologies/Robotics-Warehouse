using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ROSGeometry;
using RosMessageTypes.Sensor;

public class LaserScanSensor : MonoBehaviour
{
    public string topic;
    public ROSConnection ros;
    public float time_between_scans = 0.1f;

    public float min_range = 0;
    public float max_range = 1000;
    public float angle_deg_min = -45;
    public float angle_deg_max = 45;
    public float measurements_per_scan = 10;
    public float time_between_measurements = 0.01f;

    List<RosMessageTypes.Geometry.Point32> collectedPoints = new List<RosMessageTypes.Geometry.Point32>();
    List<GameObject> markers = new List<GameObject>();
    List<GameObject> inactiveMarkers = new List<GameObject>();
    public GameObject markerPrefab;
    float nextScanTimestamp = -1;
    List<float> ranges = new List<float>();

    bool isScanning = false;
    float scanBegunTimestamp = -1;

    protected virtual void Start()
    {
        ros.RegisterPublisher(topic, LaserScan.RosMessageName);
        nextScanTimestamp = Time.time;
    }

    void BeginScan()
    {
        isScanning = true;
        scanBegunTimestamp = Time.time;
        nextScanTimestamp = Time.time + time_between_scans;
    }

    public void EndScan()
    {
        LaserScan msg = new LaserScan();
        msg.header = new RosMessageTypes.Std.Header();
        msg.scan_time = scanBegunTimestamp;
        msg.range_min = min_range;
        msg.range_max = max_range;
        msg.angle_min = angle_deg_min * Mathf.Deg2Rad;
        msg.angle_max = angle_deg_max * Mathf.Deg2Rad;
        msg.angle_increment = (msg.angle_max - msg.angle_min) / measurements_per_scan;
        msg.time_increment = time_between_measurements;
        msg.scan_time = time_between_scans;
        msg.intensities = new float[0];
        msg.ranges = ranges.ToArray();

        ros.Send(topic, msg);

        foreach (GameObject obj in markers)
        {
            inactiveMarkers.Add(obj);
            obj.SetActive(false);
        }

        ranges.Clear();
        isScanning = false;
    }

    public void Update()
    {
        if (!isScanning)
        {
            if (Time.time < nextScanTimestamp)
                return; // do nothing while waiting for the next scan

            BeginScan();
        }

        int measurementsSoFar = 1 + Mathf.FloorToInt( (Time.time - scanBegunTimestamp) / time_between_measurements);
        if (measurementsSoFar > measurements_per_scan)
            measurementsSoFar = (int)measurements_per_scan;

        while(ranges.Count < measurementsSoFar)
        {
            float angle_rad = Mathf.Lerp(angle_deg_min, angle_deg_max, ranges.Count/measurements_per_scan) *Mathf.Deg2Rad;
            Vector3 forward = transform.forward * Mathf.Cos(angle_rad);
            Vector3 right = transform.right * Mathf.Sin(angle_rad);
            RaycastHit hit;
            if(Physics.Raycast(new Ray(transform.position, forward + right), out hit, 1000))
            {
                ranges.Add(hit.distance);

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

        if(ranges.Count >= measurements_per_scan)
        {
            EndScan();
        }
    }
}
