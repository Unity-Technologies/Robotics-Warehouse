using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Param
{
    public uint seed;
    public SunAngle sunAngle;
    public ObjectRotation objectRotation;
    public float robotPlacementDist;
    public float boxSpawnChance;

    public override string ToString()
    {
        return  "Param: " +
                "\nSeed: " + seed +
                "\nSunAngle: " + sunAngle +
                "\nRotation: " + objectRotation +
                "\nRobotPlacement: " + robotPlacementDist +
                "\nBoxSpawnChance: " + boxSpawnChance;
    }
}

[System.Serializable]
public class SunAngle
{
    public float[] hour;
    public float[] dayOfTheYear;
    public float[] latitude;

    public override string ToString()
    {
        Debug.Log(hour);
        Debug.Log(dayOfTheYear);
        Debug.Log(latitude);
        return $"Hour: [{hour[0]}, {hour[1]}]\nDayOfTheYear: [{dayOfTheYear[0]}, {dayOfTheYear[1]}]\nLatitude: [{latitude[0]}, {latitude[1]}]";
    }
}

[System.Serializable]
public class ObjectRotation
{
    public float[] x;
    public float[] y;
    public float[] z;

    public override string ToString()
    {
        return $"X: [{x[0]}, {x[1]}]\nY: [{y[0]}, {y[1]}]\nZ: [{z[0]}, {z[1]}]";
    }
}