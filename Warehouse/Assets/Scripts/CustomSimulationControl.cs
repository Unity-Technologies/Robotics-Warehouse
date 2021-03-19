using UnityEngine;
using Unity.Robotics.SimulationControl;

public class CustomSimulationControl : SimulationControlBuilder
{
    public float distanceToMove = 1f;
    SimulationManager simulationManager;

    public override INode Build()
    {
        simulationManager = GameObject.FindObjectOfType<SimulationManager>();

        Parallel root = new Parallel();

        Sequence robot = new Sequence("Turtlebot");
        root.children.Add(robot);
        // robot.children.Add(new Skip());

        Sequence scenarios = new Sequence();

        Repeat repeat = new Repeat(scenarios, 4);
        robot.children.Add(repeat);
        robot.children.Add(new QuitSimulation());

        // Randomize scene
        Sequence firstScenario = new Sequence("Scene Generation");
        firstScenario.children.Add(new PerceptionRandomize(GameObject.FindObjectOfType<PerceptionRandomizeScenario>()));
        // firstScenario.children.Add(new Randomize(simulationManager));
        firstScenario.children.Add(new RealtimeWait(500));
        scenarios.children.Add(firstScenario);

    //     // Move bot, wait for movement, check for collision
    //     Sequence secondScenario = new Sequence("Move Turtlebot");
    //     secondScenario.children.Add(new MoveTurtleBot(simulationManager, 2.0f));

    //     // // Check for goal task
    //     // var race = new Race();
    //     // race.children.Add(new CollideWithGoal(GameObject.Find("Goal")));
    //     // race.children.Add(new Not(new RealtimeWait(10000)));
    //     // secondScenario.children.Add(race);

    //     secondScenario.children.Add(new CollideWithGoal(GameObject.Find("Goal")));

    //     scenarios.children.Add(secondScenario);

    //     // Fallback fallback = new Fallback();
    //     // fallback.children.Add(root);
    //     // fallback.children.Add(new QuitSimulation());

        return root;
    }
}

// public class CollideWithGoal : TaskNode
// {
//     private TestRobotCollision cubeCollisions;

//     public CollideWithGoal(GameObject goal)
//     {
//         cubeCollisions = goal.GetComponent<TestRobotCollision>();
//     }

//     protected override void Task()
//     {
//         if (cubeCollisions.collision.Count > 1)
//         {
//             if (cubeCollisions.collision.Contains("Box") || cubeCollisions.collision.Contains("right_tire_0") || cubeCollisions.collision.Contains("left_tire_0")){
//                 Debug.Log($"Task complete: collision detected");
//                 Succeed();
//             }
//             // else if (cubeCollisions.collision.Contains("Wall")){
//             //     Debug.Log($"Ran into a wall, failing");
//             //     Fail();
//             // }
//             Debug.Log("Clearing goal; starting new collision check task");
//             cubeCollisions.collision.Clear();
//         }
//     }
// }

// public class Randomize : TaskNode
// {
//     private SimulationManager simulationManager;

//     public Randomize(SimulationManager simulationManager)
//     {
//         this.simulationManager = simulationManager;
//     }

//     protected override void Task()
//     {
//         simulationManager.GenerateEnvironment();
//         Debug.Log($"Task complete: generated environment");
//         Succeed();
//     }
// }

// public class MoveTurtleBot : TaskNode
// {
//     private SimulationManager simulationManager;
//     private float distance;
//     private Vector3[] signals;
//     private int counter = 0;

//     public MoveTurtleBot(SimulationManager simulationManager, float distance)
//     {
//         this.simulationManager = simulationManager;
//         this.distance = distance;
//         signals = new Vector3[] {new Vector3(0, distance), new Vector3(0, -distance), new Vector3(distance, 0), new Vector3(-distance, 0)};
//     }

//     protected override void Task()
//     {
//         simulationManager.MoveTurtleBot(signals[counter]);
//         Debug.Log($"Task {counter} complete: Sent move {signals[counter]} to turtlebot");
//         Succeed();
//         counter++;
//     }
// }

// public class Skip : TaskNode
// {
//     bool isSkipped = false;

//     protected override void Task()
//     {
//         if (!isSkipped)
//         {
//             isSkipped = true;
//         }
//         else
//         {
//             Succeed();
//         }
//     }
// }