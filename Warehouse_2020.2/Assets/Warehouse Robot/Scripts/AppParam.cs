using System;
using UnityEngine;
using Warehouse;

[System.Serializable]
public class AppParam
{
    public int m_width = 50;
    public int m_length = 50;
    public int m_rows = 4;
    public int m_cols = 3;
    public bool m_horizontal = true;
    public int m_numBots = 3;
    public int m_dropoff = 10;
    public string m_floorType = "PolishedCement";
    public string m_lighting = "Afternoon";
    public float m_generateFloorBoxes = 0f;
    public float m_generateFloorDebris = 0f;
    public float m_percentLight = 1.0f;
    public int m_quitAfterSeconds = 60;

    public override string ToString()
    {
        return "AppParam: " +
               "\nm_width: " + m_width +
               "\nm_length: " + m_length + 
               "\nm_rows: " + m_rows +
               "\nm_cols: " + m_cols +
               "\nm_horizontal: " + m_horizontal +
               "\nm_numBots: " + m_numBots +
               "\nm_dropoff: " + m_dropoff +
               "\nm_floorType: " + m_floorType +
               "\nm_lighting: " + m_lighting +
               "\nm_generateFloorBoxes: " + m_generateFloorBoxes +
               "\nm_generateFloorDebris: " + m_generateFloorDebris +
               "\nm_percentLight: " + m_percentLight +
               "\nm_quitAfterSeconds: " + m_quitAfterSeconds;
    }
}