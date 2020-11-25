using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotAgent : MonoBehaviour
{
    public enum RobotState {
        ToShelf,
        Pickup,
        ToDropoff,
        PlacingBox
    }

    public GameObject m_box;
    public GameObject m_boxPhysics;

    public RobotState m_state = RobotState.ToShelf;
    public NavMeshPath m_path;

    WarehouseManager manager;
    List<GameObject> _dropoffs = new List<GameObject>();
    List<GameObject> _shelves = new List<GameObject>();
    GameObject _targetShelf;
    NavMeshAgent _agent;
    Vector3 _target;
    Vector3 _spawn;
    float _distThreshold = 4.0f;
    float _dropoffWait = 2.0f;

    float DROPOFF_TIME = 2.0f;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        m_path = new NavMeshPath();
    }

    void Start(){
        manager = GameObject.FindObjectOfType<WarehouseManager>();
    }

    void Update(){
        // State machine for robot pathing
        switch(m_state){
            case RobotState.ToShelf: 
                if (Vector3.Distance(transform.position, _target) < _distThreshold) {
                    m_state = RobotState.Pickup; // begin "pickup up item"
                    _agent.isStopped = true;

                    // Remove box from shelf
                    var boxes = _targetShelf.GetComponentInChildren<Shelve>().GetBoxes();
                    if (boxes.Count > 0){
                        var rand = Random.Range(0, boxes.Count);
                        if (boxes[rand] != null){
                            Destroy(boxes[rand].gameObject);
                        }
                    }
                }
                break;
            case RobotState.Pickup: // reached shelf
                _dropoffWait -= Time.deltaTime;
                Vector3.RotateTowards(transform.forward, _target, Time.deltaTime * 300f, 0);
                if (_dropoffWait <= 0) { // Done "dropping off"
                    m_state = RobotState.ToDropoff;

                    // add box to robot
                    var box = Instantiate(m_box, new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z), Quaternion.identity);
                    box.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    box.transform.parent = transform;

                    bool found = false;
                    float dist = 9999;
                    foreach (var point in _dropoffs){ // find nearest dropoff
                        if (Vector3.Distance(transform.position, point.transform.position) < dist){
                            found = true;
                            dist = Vector3.Distance(transform.position, point.transform.position);
                            _target = point.transform.position;

                        }
                    }
                    if (!found){
                        _target = _dropoffs[0].transform.position;
                    }
                    m_state = RobotState.ToDropoff;
                    _agent.isStopped = false;
                    _agent.SetDestination(_target);

                    _dropoffWait = DROPOFF_TIME;
                }
                break;
            case RobotState.ToDropoff: // on way to DROPOFF area
                if (Vector3.Distance(transform.position, _target) < _distThreshold / 2f){ // reached 
                    m_state = RobotState.PlacingBox;
                    _agent.isStopped = true;
                }
                break;
            case RobotState.PlacingBox:
                _dropoffWait -= Time.deltaTime;
                if (_dropoffWait <= 0) { // Done "dropping off"

                    // remove box from robot, create one at dropoff
                    Destroy(transform.GetChild(0).gameObject);
                    var box = Instantiate(m_boxPhysics, new Vector3(_target.x, _target.y + 2f, _target.z + 1), Quaternion.identity);
                    box.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                    m_state = RobotState.ToShelf;
                    _targetShelf = _shelves[Random.Range(0, _shelves.Count)];
                    _target = _targetShelf.transform.position;
                    _agent.SetDestination(_target);
                    _agent.isStopped = false;
                    _dropoffWait = DROPOFF_TIME;
                }
                break;
            default: 
                return;
        }

        // Draw robot paths
        CalculatePath();
        
        for (int i = 0; i < m_path.corners.Length - 1; i++){
            Debug.DrawLine(m_path.corners[i], m_path.corners[i + 1], Color.red);
        }
    }

    // Sample locations on the NavMesh as appropriate to calculate path of Agent
    private void CalculatePath(){
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position, out hit, 100.0f, NavMesh.AllAreas);

        NavMeshHit targetHit;
        NavMesh.SamplePosition(_target, out targetHit, 100.0f, NavMesh.AllAreas);

        NavMesh.CalculatePath(hit.position, targetHit.position, NavMesh.AllAreas, m_path);
    }

    #region Setters
    public void SetSpawn(Vector3 pos){
        _spawn = pos;
        transform.position = pos;
        _agent.enabled = true;
        CalculatePath();
    }
    
    public void SetDropoffs(List<GameObject> l){
        _dropoffs = l;
    }

    public void SetShelves(List<GameObject> shelves){
        _shelves = shelves;
        _targetShelf = _shelves[Random.Range(0, _shelves.Count)];
        _target = _targetShelf.transform.position;
        _agent.SetDestination(_target);
    }

    #endregion //Setters
}
