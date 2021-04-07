using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class LaserScanSensor : MonoBehaviour
{
    public string topic;
    public bool ConnectToRos;
    public float TimeBetweenScansSeconds = 0.1f;
    public float RangeMetersMin = 0;
    public float RangeMetersMax = 1000;
    public float ScanAngleDegMin = -45;
    public float ScanAngleDegMax = 45;
    public float NumMeasurementsPerScan = 10;
    public float TimeBetweenMeasurementsSeconds = 0.01f;
    public GameObject markerPrefab;
    public string LayerMaskName = "TurtleBot3Manual";

    List<RosMessageTypes.Geometry.Point32> collectedPoints = new List<RosMessageTypes.Geometry.Point32>();
    Queue<GameObject> m_MarkersActive = new Queue<GameObject>();
    Queue<GameObject> m_MarkersInactive = new Queue<GameObject>();
    
    ROSConnection m_Ros;
    float m_TimeNextScanSeconds = -1;
    private int m_NumMeasurementsTaken;
    List<float> ranges = new List<float>();
    LayerMask m_SelfMask;
    
    bool isScanning = false;
    float m_TimeLastScanBeganSeconds = -1;

    protected virtual void Start()
    {
        if (ConnectToRos)
        {
            m_Ros = ROSConnection.instance;
            m_Ros.RegisterPublisher(topic, LaserScan.RosMessageName);
        }
        m_SelfMask = LayerMask.GetMask(LayerMaskName);

        m_TimeNextScanSeconds = Time.time + TimeBetweenScansSeconds;
    }

    void BeginScan()
    {
        isScanning = true;
        m_TimeLastScanBeganSeconds = Time.time;
        m_TimeNextScanSeconds = Time.time + TimeBetweenScansSeconds;
        m_NumMeasurementsTaken = 0;
        ResetMarkers();
    }

    void ResetMarkers()
    {
        while(m_MarkersActive.Count > 0)
        {
            var marker = m_MarkersActive.Dequeue();
            marker.SetActive(false);
            m_MarkersInactive.Enqueue(marker);
        }
    }

    public void EndScan()
    {
        if (ranges.Count == 0)
        {
            Debug.LogWarning($"Took {m_NumMeasurementsTaken} measurements but found no valid ranges");
        }

        LaserScan msg = new LaserScan();
        msg.header = new RosMessageTypes.Std.Header();
        msg.scan_time = m_TimeLastScanBeganSeconds;
        msg.range_min = RangeMetersMin;
        msg.range_max = RangeMetersMax;
        msg.angle_min = ScanAngleDegMin * Mathf.Deg2Rad;
        msg.angle_max = ScanAngleDegMax * Mathf.Deg2Rad;
        msg.angle_increment = (msg.angle_max - msg.angle_min) / NumMeasurementsPerScan;
        msg.time_increment = TimeBetweenMeasurementsSeconds;
        msg.scan_time = TimeBetweenScansSeconds;
        msg.intensities = new float[ranges.Count];
        msg.ranges = ranges.ToArray();

        if (ConnectToRos)
            m_Ros.Send(topic, msg);

        m_NumMeasurementsTaken = 0;
        ranges.Clear();
        isScanning = false;
    }

    public void Update()
    {
        if (!isScanning)
        {
            if (Time.time < m_TimeNextScanSeconds)
                return; // do nothing while waiting for the next scan

            BeginScan();
        }
        
        var measurementsSoFar = TimeBetweenMeasurementsSeconds == 0 ? NumMeasurementsPerScan :
            1 + Mathf.FloorToInt( (Time.time - m_TimeLastScanBeganSeconds) / TimeBetweenMeasurementsSeconds);
        if (measurementsSoFar > NumMeasurementsPerScan)
            measurementsSoFar = (int)NumMeasurementsPerScan;

        var yawBaseDegrees = transform.rotation.eulerAngles.y;
        while(m_NumMeasurementsTaken < measurementsSoFar)
        {
            var t = m_NumMeasurementsTaken / NumMeasurementsPerScan;
            var yawSensorDegrees = Mathf.Lerp(ScanAngleDegMin, ScanAngleDegMax, t);
            var yawDegrees = yawBaseDegrees + yawSensorDegrees;
            var directionVector = Quaternion.Euler(0f, yawDegrees, 0f) * Vector3.forward;
            var measurementStart = RangeMetersMin * directionVector + transform.position;
            var measurementRay = new Ray(measurementStart, directionVector);
            var foundValidMeasurement = Physics.Raycast(measurementRay, out var hit, RangeMetersMax);
            // Only record measurement if it's within the sensor's operating range
            if (foundValidMeasurement)
            {
                ranges.Add(hit.distance);

                if (m_MarkersInactive.Count > 0)
                {
                    var inactiveMarker = m_MarkersInactive.Dequeue();
                    inactiveMarker.SetActive(true);
                    inactiveMarker.transform.position = hit.point;
                    m_MarkersActive.Enqueue(inactiveMarker);
                }
                else if (markerPrefab != null)
                {
                    m_MarkersActive.Enqueue(Instantiate(markerPrefab, hit.point, Quaternion.identity));
                }
            }

            // Even if Raycast didn't find a valid hit, we still count it as a measurement
            ++m_NumMeasurementsTaken;
        }

        if(m_NumMeasurementsTaken >= NumMeasurementsPerScan)
        {
            if (m_NumMeasurementsTaken > NumMeasurementsPerScan)
            {
                Debug.LogError($"LaserScan has {m_NumMeasurementsTaken} measurements but we expected {NumMeasurementsPerScan}");
            }
            EndScan();
        }

    }
}
