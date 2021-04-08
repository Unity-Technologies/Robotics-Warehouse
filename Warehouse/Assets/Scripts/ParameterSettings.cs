using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Simulation.Warehouse {
    public class ParameterSettings : MonoBehaviour
    {
        public List<PhysicMaterial> PhysicMaterials;
        public List<Material> FloorTextures;
        
        public PhysicMaterial GetPhysicMaterial(FloorType type){
            switch (type){
                case FloorType.PolishedCement:
                    return PhysicMaterials[0];
                case FloorType.Hardwood:
                    return PhysicMaterials[1];
                case FloorType.LowPileCarpet:
                    return PhysicMaterials[2];
                case FloorType.HighPileCarpet:
                    return PhysicMaterials[3];
                default:
                    Debug.LogError($"Invalid Floor Type {type}!");
                    return PhysicMaterials[0];
            }
        }

        public Material GetMaterial(FloorType type){
            switch (type){
                case FloorType.PolishedCement:
                    return FloorTextures[0];
                case FloorType.Hardwood:
                    return FloorTextures[1];
                case FloorType.LowPileCarpet:
                    return FloorTextures[2];
                case FloorType.HighPileCarpet:
                    return FloorTextures[3];
                default:
                    Debug.LogError($"Invalid Floor Type {type}!");
                    return FloorTextures[0];
            }
        }
    }
}
