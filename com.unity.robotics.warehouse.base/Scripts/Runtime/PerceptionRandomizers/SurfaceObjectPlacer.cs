using System;
using System.Collections.Generic;
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
    public float radius;
    /* Checks if objects are placed far enough apart, so they cannot collide. */

    public float x;
    public float z;

    public CollisionConstraint() { }

    public CollisionConstraint(float tx, float tz, float tradius)
    {
        x = tx;
        z = tz;
        radius = tradius;
    }

    public override bool Passes(float placementX, float placementZ, float objectRadius)
    {
        var placementPt = new Vector2(placementX, placementZ);
        var basePt = new Vector2(x, z);
        var distance = Vector2.Distance(placementPt, basePt);
        var minDistance = objectRadius + radius;
        return distance > minDistance;
    }
}

public class SurfaceObjectPlacer
{
    Bounds bounds;

    List<PlacementConstraint> collisionConstraints = new List<PlacementConstraint>();
    int maxPlacementTries = 100;
    FloatParameter random; //[0, 1]

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
            var objBounds = obj.GetComponent<Renderer>().bounds;
            var radius = objBounds.extents.magnitude;
            var heightAbovePlane = objBounds.extents.y;

            var constraints = GetAllConstraints();
            var point = SampleValidGlobalPointOnPlane(radius, constraints, bounds);

            if (point.HasValue)
            {
                // place object
                var foundPoint = point ?? Vector3.zero;
                obj.transform.position = new Vector3(foundPoint.x, foundPoint.y + heightAbovePlane, foundPoint.z);

                // update constraints so subsequently placed object cannot collide with this one
                var newConstraint = new CollisionConstraint();
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

    Vector3? SampleValidGlobalPointOnPlane(float objectRadius, List<PlacementConstraint> constraints, Bounds bounds)
    {
        // return a valid point and if not found one it return null
        var tries = 0;

        while (tries < maxPlacementTries)
        {
            var point = SampleGlobalPointOnPlane(objectRadius, bounds);
            var valid = PassesConstraints(point, objectRadius, constraints);
            if (valid)
            {
                return point;
            }

            tries += 1;
        }

        return null;
    }

    List<PlacementConstraint> GetAllConstraints()
    {
        // return a list of all the constraints: combination of permanent constraint and additional constraint like the maxReachabilityConstraint or the
        // collision constraint
        var allConstraints = new List<PlacementConstraint>();
        allConstraints.AddRange(collisionConstraints);
        return allConstraints;
    }

    Vector3 SampleGlobalPointOnPlane(float minEdgeDistance, Bounds bounds)
    {
        var planePlacementZone = PlanePlacementZone(bounds, minEdgeDistance);

        var randomPlanePoint = RandomPointInRect(planePlacementZone);
        var globalPt = new Vector3(randomPlanePoint.x, bounds.center.y, randomPlanePoint.y);
        return globalPt;
    }

    static Rect PlanePlacementZone(Bounds bounds, float minEdgeDistance)
    {
        var x = bounds.center.x - bounds.extents.x + minEdgeDistance;
        var z = bounds.center.z - bounds.extents.z + minEdgeDistance;
        var dx = (bounds.extents.x - minEdgeDistance) * 2;
        var dz = (bounds.extents.z - minEdgeDistance) * 2;
        return new Rect(x, z, dx, dz);
    }

    Vector2 RandomPointInRect(Rect rect)
    {
        var x = random.Sample() * rect.width + rect.xMin;
        var y = random.Sample() * rect.height + rect.yMin;
        return new Vector2(x, y);
    }

    static Rect Intersection(Rect rectA, Rect rectB)
    {
        var minX = Mathf.Max(rectA.xMin, rectB.xMin);
        var maxX = Mathf.Min(rectA.xMax, rectB.xMax);
        var minY = Mathf.Max(rectA.yMin, rectB.yMin);
        var maxY = Mathf.Min(rectA.yMax, rectB.yMax);

        var xValid = minX < maxX;
        var yValid = minY < maxY;
        var valid = xValid && yValid;
        if (!valid)
        {
            throw new Exception("Rectangles have no intersection!");
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    static bool PassesConstraints(Vector3 point, float objectRadius, List<PlacementConstraint> constraints)
    {
        /* Checks if sampled point on plane passes all provided constraints. */

        foreach (var constraint in constraints)
        {
            var pass = constraint.Passes(point.x, point.z, objectRadius);
            if (!pass)
            {
                return false;
            }
        }

        return true;
    }
}
