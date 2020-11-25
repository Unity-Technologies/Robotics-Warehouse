using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Warehouse;

public class ParameterSettings : MonoBehaviour
{
    public List<PhysicMaterial> PhysicMaterials;
    
    public PhysicMaterial GetMaterial(FloorType type){
        switch (type){
            case FloorType.PolishedCement:
                return PhysicMaterials[0];
            case FloorType.Hardwood:
                return PhysicMaterials[1];
            case FloorType.LowPlyCarpet:
                return PhysicMaterials[2];
            case FloorType.ShagCarpet:
                return PhysicMaterials[3];
            default:
                Debug.LogError($"Invalid Floor Type {type}!");
                return PhysicMaterials[0];
        }
    }
}
