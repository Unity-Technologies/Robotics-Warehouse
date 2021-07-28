# Installation
1. In your Unity project (version 2020.3 or later), open the package manager from `Window` -> `Package Manager` and select "Add package from git URL..."

    ![image](https://user-images.githubusercontent.com/29758400/110989310-8ea36180-8326-11eb-8318-f67ee200a23d.png)

2. Enter the following URL.
`https://github.com/Unity-Technologies/Robotics-Warehouse.git?path=/com.unity.robotics.warehouse`
3. Click `Add`.

# Generating a Warehouse

1. Using the project as a package, you can navigate to the sample scene in your Project window via `Packages/com.unity.robotics.warehouse/Scenes/Warehouse`. As the scene is provided in a read-only package, you can copy the scene into your own project's Assets. Double click the copied scene to open it.
2. You can generate the warehouse in Editor Mode or Play Mode. The scene contains `Main Camera`, `Directional Light`, and `Warehouse Manager` GameObjects in the Hierarchy. Select the `Warehouse Manager` object, and click `Generate`.

   ![](img/warehousemanager.png)

   1. Expand the `App Param` member on the `WarehouseManager` component. This defines the length and width of the warehouse, and how many rows and columns of shelves are instantiated. Set these values as you want the warehouse to look.
   2. Click the `Generate` button on the WarehouseManager to generate the warehouse with the specified parameters.
   3. Click `Save prefab` to save this version of the warehouse to `Assets/Prefabs`. Note: the spawned boxes and debris will not appear in this saved prefab.
   4. Use this spawned prefab in a new scene, or however you'd like!

## Scenario

- The **Scenario Shim** on the `Warehouse Manager` defines the core logic for randomization. Assign the values as desired (usage defined below):
  - **SunAngleRandomizer** - Directional Light angle and location
  - **LocalRotationRandomizerShim** - Assigns local rotation, used only on the shelves. It is suggested to keep the `X` and `Z` values at 0.
  <!-- - **MaterialRandomizer** - Assigns a material and [physic material](https://docs.unity3d.com/Manual/class-PhysicMaterial.html) friction value to the floor. -->
  - **ShelfBoxRandomizerShim** - Randomizes the number of boxes on the shelves. Use `Box Spawn Chance` to define the percent chance of a box spawning at each possible location.
  - **FloorBoxRandomizerShim** - Spawns boxes on the floor of the warehouse. Use `Num Box To Spawn` to define how many boxes are spawned.
- This can be re-run by incrementing the scenario iteration. This can be done via the `WarehouseManager` component, which shows an `Increment Iteration` button in the Inspector.

> Learn more about the Perception Package [Randomization](https://github.com/Unity-Technologies/com.unity.perception/blob/master/com.unity.perception/Documentation~/Randomization/Index.md).

## Rigidbody Spawning

This feature is only implemented in Play mode.

- During Play Mode, a UI overlay will appear in the top-right corner of the Game view. This can be used to spawn box towers or piles of debris with specified parameters.
  - **Show Location Picker**: This toggle enables a visualizer that displays where the objects will spawn.
  - **Spawn boxes**: The text field to the right of this button expects an input with the format `width,length,height`, e.g. `2,3,4`. Pressing the `Spawn boxes` button will create a tower of boxes with the given dimensions on the Location Picker location.
  - **Debris Size**: This slider defines the maximum scale of the debris spawned.
  - **Debris is kinematic**: This toggle defines if the debris spawned is kinematic or not.
  - **Spawn debris**: The text field to the right of this button expects an integer input that describes the number of objects to spawn. Pressing the `Spawn debris` button will instantiate that number of random primitives of randomized scale (up to the `Debris Size`).

    ![](img/debris.gif)