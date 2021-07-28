using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;


public class PlacementConstraint
{
    /* Determines if a particular object placement on the x-z plane is valid. See
     subclasses for details. */

    public virtual bool Passes(float placementX, float placementZ, float objectRadius)
    {
        return true;
    }
}

public class CollisionConstraint : PlacementConstraint
{
    /* Checks if objects are placed far enough apart, so they cannot collide. */

    public float x;
    public float z;
    public float radius;

    public CollisionConstraint() { }

    public CollisionConstraint(float tx, float tz, float tradius)
    {
        this.x = tx;
        this.z = tz;
        this.radius = tradius;
    }

    public override bool Passes(float placementX, float placementZ, float objectRadius)
    {
        Vector2 placementPt = new Vector2(placementX, placementZ);
        Vector2 basePt = new Vector2(x, z);
        float distance = Vector2.Distance(placementPt, basePt);
        float minDistance = objectRadius + radius;
        return distance > minDistance;
    }
}

public class SurfaceObjectPlacer
{
    private Bounds bounds;
    private FloatParameter random; //[0, 1]
    private int maxPlacementTries = 100;


    private List<PlacementConstraint> collisionConstraints = new List<PlacementConstraint>();


    public SurfaceObjectPlacer(Bounds bounds, FloatParameter random, int maxPlacementTries)
    {
        this.bounds = bounds;
        this.random = random;
        this.maxPlacementTries = maxPlacementTries;
    }


    public void IterationStart()
    {
        collisionConstraints = new List<PlacementConstraint>();
    }

    public bool PlaceObject(GameObject obj)
    {
        if (obj.activeInHierarchy)
        {
            // try to sample a valid point
            Bounds objBounds = obj.GetComponent<Renderer>().bounds;
            float radius = objBounds.extents.magnitude;
            float heightAbovePlane = objBounds.extents.y;

            List<PlacementConstraint> constraints = GetAllConstraints();
            Vector3? point = SampleValidGlobalPointOnPlane(radius, constraints, bounds);

            if (point.HasValue)
            {
                // place object
                Vector3 foundPoint = point ?? Vector3.zero;
                obj.transform.position = new Vector3(foundPoint.x, foundPoint.y + heightAbovePlane, foundPoint.z);

                // update constraints so subsequently placed object cannot collide with this one
                CollisionConstraint newConstraint = new CollisionConstraint();
                newConstraint.x = foundPoint.x;
                newConstraint.z = foundPoint.z;
                newConstraint.radius = radius;
                collisionConstraints.Add(newConstraint);

            }
            else
            {
                return false;
            }
        }
        return true;

    }

    // PRIVATE HELPERS

    private Vector3? SampleValidGlobalPointOnPlane(float objectRadius, List<PlacementConstraint> constraints, Bounds bounds)
    {
        // return a valid point and if not found one it return null 
        int tries = 0;

        while (tries < maxPlacementTries)
        {
            Vector3 point = SampleGlobalPointOnPlane(objectRadius, bounds);
            bool valid = PassesConstraints(point, objectRadius, constraints);
            if (valid) { return point; }

            tries += 1;
        }
        return null;
    }

    private List<PlacementConstraint> GetAllConstraints()
    {
        // return a list of all the constraints: combination of permanent constraint and additional constraint like the maxReachabilityConstraint or the 
        // collision constraint 
        List<PlacementConstraint> allConstraints = new List<PlacementConstraint>();
        allConstraints.AddRange(collisionConstraints);
        return allConstraints;
    }

    private Vector3 SampleGlobalPointOnPlane(float minEdgeDistance, Bounds bounds)
    {
        Rect planePlacementZone = PlanePlacementZone(bounds, minEdgeDistance);

        Vector2 randomPlanePoint = RandomPointInRect(planePlacementZone);
        Vector3 globalPt = new Vector3(randomPlanePoint.x, bounds.center.y, randomPlanePoint.y);
        return globalPt;
    }

    private static Rect PlanePlacementZone(Bounds bounds, float minEdgeDistance)
    {
        float x = bounds.center.x - bounds.extents.x + minEdgeDistance;
        float z = bounds.center.z - bounds.extents.z + minEdgeDistance;
        float dx = (bounds.extents.x - minEdgeDistance) * 2;
        float dz = (bounds.extents.z - minEdgeDistance) * 2;
        return new Rect(x, z, dx, dz);
    }

    private Vector2 RandomPointInRect(Rect rect)
    {
        float x = random.Sample() * rect.width + rect.xMin;
        float y = random.Sample() * rect.height + rect.yMin;
        return new Vector2(x, y);
    }

    private static Rect Intersection(Rect rectA, Rect rectB)
    {
        float minX = Mathf.Max(rectA.xMin, rectB.xMin);
        float maxX = Mathf.Min(rectA.xMax, rectB.xMax);
        float minY = Mathf.Max(rectA.yMin, rectB.yMin);
        float maxY = Mathf.Min(rectA.yMax, rectB.yMax);

        bool xValid = (minX < maxX);
        bool yValid = (minY < maxY);
        bool valid = (xValid && yValid);
        if (!valid)
        {
            throw new Exception("Rectangles have no intersection!");
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);

    }

    private static bool PassesConstraints(Vector3 point, float objectRadius, List<PlacementConstraint> constraints)
    {
        /* Checks if sampled point on plane passes all provided constraints. */

        foreach (PlacementConstraint constraint in constraints)
        {
            bool pass = constraint.Passes(point.x, point.z, objectRadius);
            if (!pass) { return false; }
        }
        return true;
    }


}
