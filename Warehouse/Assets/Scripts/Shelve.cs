using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Simulation.Warehouse {
    public class Shelve : MonoBehaviour
    {
        public bool generateBoxes = false; 

        [Header("Box Spawn Points")]
        public List<Transform> m_layer0;
        public List<Transform> m_layer1;
        public List<Transform> m_layer2;
        public List<Transform> m_layer3;

        [Header("Box Prefabs")]
        public List<Transform> m_boxes; // box prefabs

        List<Transform> _boxes = new List<Transform>(); // box instances

        const int _maxNumPerLayer = 3; // max number of boxes on each layer 

        // number of boxes on each layer
        int _numLayer0;
        int _numLayer1;
        int _numLayer2;
        int _numLayer3;

        // Start is called before the first frame update
        void Start() {
            if (generateBoxes) GenRandom();
        }

        // generate the boxes on shelve randomly
        public void GenRandom() {
            _numLayer0 = Random.Range(0, 1+_maxNumPerLayer);
            _numLayer1 = Random.Range(0, 1+_maxNumPerLayer);
            _numLayer2 = Random.Range(0, 1+_maxNumPerLayer);
            _numLayer3 = Random.Range(0, 1+_maxNumPerLayer);
            Gen(_numLayer0, _numLayer1, _numLayer2, _numLayer3);
        }

        // generate the boxes on shelve
        public void Gen(int numLayer0, int numLayer1, int numLayer2, int numLayer3) {
            _numLayer0 = numLayer0;
            _numLayer1 = numLayer1;
            _numLayer2 = numLayer2;
            _numLayer3 = numLayer3;
            GenLayer(_numLayer0, m_layer0);
            GenLayer(_numLayer1, m_layer1);
            GenLayer(_numLayer2, m_layer2);
            GenLayer(_numLayer3, m_layer3);
        }

        // generate num boxes on the layer
        void GenLayer(int num, List<Transform> layer) {
            int[] arr = {0, 1, 2};
            for (var i = 0; i < num; i++) {
                var j = Random.Range(i, arr.Length); // knuth shuffling
                var tmp = arr[i];
                arr[i] = arr[j];
                arr[j] = tmp;
                GenBox(layer[arr[i]]);
            }
        }

        // generate a box under the parent transform
        void GenBox(Transform parent) {
            var i = Random.Range(0, m_boxes.Count);
            var box = Instantiate(m_boxes[i], parent);
            box.localScale = Vector3.one;
            _boxes.Add(box);
        }

        public List<Transform> GetBoxes(){
            return _boxes;
        }
    }
}