using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utility
{
    public const int MAX_HIT_COUNT = 70;


    #region Vector3 Extension Methods

    /// <summary>
    /// Returns the given vector rotated by the given angle
    /// 2017-02-21: copied from a post by wpennypacker: https://forum.unity3d.com/threads/vector-rotation.33215/
    /// </summary>
    /// <param name="v"></param>
    /// <param name="angle"></param>
    public static Vector2 RotateZ(this Vector2 v, float angle)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        float tx = (cos * v.x) - (sin * v.y);
        float ty = (cos * v.y) + (sin * v.x);

        return new Vector3(tx, ty);
    }
    public static Vector3 RotateZ(this Vector3 v, float angle)
    {
        return ((Vector2)v).RotateZ(angle);
    }
    /// <summary>
    /// Returns the angle of the given vector
    /// 2017-04-18: copied from an answer by Sigil: http://webcache.googleusercontent.com/search?q=cache:http://answers.unity3d.com/questions/162177/vector2angles-direction.html&num=1&strip=1&vwsrc=0
    /// </summary>
    /// <param name="v"></param>
    /// <param name="angle"></param>
    public static float RotationZ(this Vector3 v1, Vector3 v2)
    {
        float angle = Vector2.Angle(v1, v2);
        Vector3 cross = Vector3.Cross(v1, v2);
        if (cross.z > 0)
        {
            angle = 360 - angle;
        }
        return angle;
    }

    public static Vector2 PerpendicularRight(this Vector2 v)
    {
        return v.RotateZ(-Mathf.PI / 2);
    }
    public static Vector3 PerpendicularRight(this Vector3 v)
    {
        return ((Vector2)v).PerpendicularRight();
    }
    public static Vector2 PerpendicularLeft(this Vector2 v)
    {
        return v.RotateZ(Mathf.PI / 2);
    }
    public static Vector3 PerpendicularLeft(this Vector3 v)
    {
        return ((Vector2)v).PerpendicularLeft();
    }
    public static float distanceToObject(this Vector2 position, GameObject obj)
    {
        Vector2 center = obj.getCollectiveColliderCenter();
        Vector2 dir = (center - position).normalized;
        RaycastAnswer answer = Utility.RaycastAll(position, dir, (center - position).magnitude);
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            if (rch2d.collider.gameObject == obj)
            {
                return rch2d.distance;
            }
        }
        throw new UnityException("Object " + obj + "'s raycast not found! This should not be possible!");
    }
    public static bool inRange(this Vector2 v1, Vector2 v2, float range)
    {
        return (v1 - v2).sqrMagnitude <= range * range;
    }
    public static bool inRange(this Vector3 v1, Vector3 v2, float range)
    {
        return (v1 - v2).sqrMagnitude <= range * range;
    }
    public static Vector2 average(List<Vector2> vectors)
        => (vectors.Count > 0)
            ? vectors.Aggregate((result, v) => result + v) / vectors.Count
            : Vector2.zero;
    public static Vector3 setX(this Vector3 v, float x)
        => new Vector3(x, v.y, v.z);
    public static Vector3 setY(this Vector3 v, float y)
        => new Vector3(v.x, y, v.z);
    public static Vector3 setZ(this Vector3 v, float z)
        => new Vector3(v.x, v.y, z);

    public static Vector2 travelAlongCircle(this Vector2 pos, Vector2 center, float distance)
    {
        //2020-12-05: copied from http://answers.unity.com/answers/572675/view.html
        Vector3 axis = Vector3.back;
        Vector2 dir = pos - center;
        float circumference = 2.0f * Mathf.PI * dir.magnitude;
        float angle = distance / circumference * 360.0f;
        dir = Quaternion.AngleAxis(angle, axis) * dir;
        return dir + center;
    }
    #endregion

    #region Rigidbody2D Extension Methods

    /**
    * 2016-03-25: copied from "2D Explosion Force" Asset: https://www.assetstore.unity3d.com/en/#!/content/24077
    * 2016-03-29: moved here from PlayerController
    * 2017-03-09: moved here from ForceTeleportAbility
    */
    public static void AddExplosionForce(this Rigidbody2D body, float expForce, Vector3 expPosition, float expRadius)
    {
        var dir = (body.transform.position - expPosition);
        float calc = 1 - (dir.magnitude / expRadius);
        if (calc <= 0)
        {
            calc = 0;
        }

        body.AddForce(dir.normalized * expForce * calc);
    }
    /// <summary>
    /// Adds explosion force to the given Rigidbody2D based in part on its own mass
    /// </summary>
    /// <param name="body"></param>
    /// <param name="expForce"></param>
    /// <param name="expPosition"></param>
    /// <param name="expRadius"></param>
    /// <param name="maxForce">The maximum amount of force that can be applied</param>
    public static void AddWeightedExplosionForce(this Rigidbody2D body, float expForce, Vector2 expPosition, float expRadius, float maxForce = float.MaxValue)
    {
        Vector2 dir = ((Vector2)body.transform.position - expPosition).normalized;
        float distanceToEdge = expRadius - expPosition.distanceToObject(body.gameObject);
        if (distanceToEdge < 0)
        {
            distanceToEdge = 0;
        }
        float calc = (distanceToEdge / expRadius);
        if (calc <= 0)
        {
            calc = 0;
        }
        float force = body.mass * distanceToEdge * calc * expForce / Time.fixedDeltaTime;
        force = Mathf.Min(force, maxForce);
        body.AddForce(dir * force);
    }
    /// <summary>
    /// Sets the velocity of the RigidBody2D to move away from the explosion position at the given speed
    /// </summary>
    /// <param name="body"></param>
    /// <param name="expSpeed"></param>
    /// <param name="expPosition"></param>
    public static void SetExplosionVelocity(this Rigidbody2D body, float expSpeed, Vector2 expPosition)
    {
        Vector2 dir = ((Vector2)body.transform.position - expPosition).normalized;
        body.velocity = dir * expSpeed;
    }
    public static bool isMoving(this Rigidbody2D rb2d)
    {
        return !Mathf.Approximately(rb2d.velocity.sqrMagnitude, 0);
    }
    public static void nullifyMovement(this Rigidbody2D rb2d)
    {
        rb2d.velocity = Vector2.zero;
        rb2d.angularVelocity = 0;
    }
    #endregion

    #region GameObject Extension Methods

    public static bool isPlayer(this GameObject go)
    {
        return go.GetComponent<PlayerController>();
    }
    /// <summary>
    /// Returns true if the collider is of the player's and is not a trigger
    /// </summary>
    /// <param name="coll2d"></param>
    /// <returns></returns>
    public static bool isPlayerSolid(this Collider2D coll2d)
    {
        return coll2d.gameObject.GetComponent<PlayerController>()
            && !coll2d.isTrigger;
    }
    /// <summary>
    /// Returns true if the collider can be stood on,
    /// i.e. is not a trigger
    /// </summary>
    /// <param name="coll2d"></param>
    /// <returns></returns>
    public static bool isSolid(this Collider2D coll2d)
    {
        return !coll2d.isTrigger;
    }

    /// <summary>
    /// Returns true if the game object has state to save
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static bool isSavable(this GameObject go)
        => go.GetComponent<Rigidbody2D>() || go.GetComponent<SavableMonoBehaviour>();
    public static bool isMemory(this GameObject go)
        => go.GetComponent<MemoryMonoBehaviour>();

    public static bool containsSavables(this GameObject go)
        => go.GetComponent<ISavableContainer>() != null;

    public static bool hasKey(this GameObject go)
        => (go != null)
            ? go.GetComponent<ObjectInfo>() != null
            : false;
    /// <summary>
    /// Returns the unique inter-scene identifier for the object
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static int getKey(this GameObject go)
    {
        if (!go)
        {
            return -1;
        }
        ObjectInfo info = go.GetComponent<ObjectInfo>();
        if (!info)
        {
            throw new ArgumentException(
                "Object " + go.name + " does not have an ObjectInfo!"
                );
        }
        return info.Id;
    }

    /// <summary>
    /// Returns the size of the GameObject based on its sprite
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static Vector2 getSize(this GameObject go)
    {
        Vector2 prevUp = go.transform.up;
        go.transform.up = Vector2.up;
        SpriteRenderer goSR = go.GetComponent<SpriteRenderer>();
        Bounds b = goSR.bounds;
        go.transform.up = prevUp;
        return b.size;
    }

    /// <summary>
    /// Sums the centers of all non-trigger colliders
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Vector2 getCollectiveColliderCenter(this GameObject obj)
    {
        int count = 0;
        Vector2 sum = Vector2.zero;
        //Try only the non-trigger colliders first
        foreach (Collider2D c2d in obj.GetComponents<Collider2D>())
        {
            if (!c2d.isTrigger)
            {
                sum += (Vector2)c2d.bounds.center;
                count++;
            }
        }
        //If that doesn't work,
        if (count == 0)
        {
            //Try the trigger colliders
            foreach (Collider2D c2d in obj.GetComponents<Collider2D>())
            {
                if (c2d.isTrigger)
                {
                    sum += (Vector2)c2d.bounds.center;
                    count++;
                }
            }
        }
        return sum / count;
    }
    /// <summary>
    /// Determines whether the center of the first object has a direct line of sight to the center of the second object
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool lineOfSight(this GameObject first, GameObject second)
    {
        Vector2 pos1 = first.transform.position;
        Vector2 pos2 = second.transform.position;
        RaycastAnswer answer = Utility.RaycastAll(pos1, pos2 - pos1, Vector2.Distance(pos1, pos2));
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            if (rch2d.collider.isTrigger)
            {
                //don't process triggers
                continue;
            }
            GameObject collGO = rch2d.collider.gameObject;
            if (!collGO.equalsHierarchy(first) && !collGO.equalsHierarchy(second))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true if the given objects equal each other or each other's immediate parents
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool equalsHierarchy(this GameObject first, GameObject second)
    {
        return first == second
            || (first.transform.parent?.gameObject == second)
            || (first == second.transform.parent?.gameObject);
    }
    #endregion

    /// <summary>
    /// Loops the value around until it falls in the range of [min, max]
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float loopValue(float value, float min, float max)
    {
        float diff = max - min;
        while (value < min)
        {
            value += diff;
        }
        while (value > max)
        {
            value -= diff;
        }
        return value;
    }
    /// <summary>
    /// Converts the number from the range (curLow, curHigh) to the range (newLow, newHigh), inclusive.
    /// 2017-04-24: copied from Meowzart.Utility.convertToRange()
    /// </summary>
    /// <param name="number">A number between (curLow, curHigh), inclusive</param>
    /// <param name="curLow">The low end of the current range</param>
    /// <param name="curHigh">The high end of the current range</param>
    /// <param name="newLow">The low end of the new range</param>
    /// <param name="newHigh">The high end of the new range</param>
    /// <param name="autoClamp">True to automatically clamp the number between the current low and high before converting</param>
    /// <returns>A number between (newLow, newHigh), inclusive</returns>
    public static float convertToRange(float number, float curLow, float curHigh, float newLow, float newHigh, bool autoClamp = false)
    {
        if (autoClamp)
        {
            number = Mathf.Clamp(number, curLow, curHigh);
        }
        else
        {
            //Check that number is between curLow and curHigh
            if (number > curHigh || number < curLow)
            {
                throw new System.ArgumentException("number is " + number + " but it should be between (" + curLow + ", " + curHigh + ")");
            }
        }
        //Check the bounds in relation to each other
        if (curLow > curHigh)
        {
            throw new System.ArgumentException("curLow (" + curLow + ") is higher than curHigh (" + curHigh + ")!");
        }
        if (newLow > newHigh)
        {
            throw new System.ArgumentException("newLow (" + newLow + ") is higher than newHigh (" + newHigh + ")!");
        }
        //Conversion
        return (((number - curLow) * (newHigh - newLow) / (curHigh - curLow)) + newLow);
    }

    public static Vector2 convertToRange(Vector2 vector, Vector2 curStart, Vector2 curEnd, Vector2 newStart, Vector2 newEnd)
    {
        float startDistance = (curEnd - curStart).magnitude;
        float distancePercent = (vector - curStart).magnitude / startDistance;
        if (Vector3.Angle(vector - curStart, curEnd - curStart) > 90)
        {
            distancePercent = 0;
        }
        return convertPercentToVector2(distancePercent, newStart, newEnd);
    }
    public static Vector2 convertPercentToVector2(float distancePercent, Vector2 newStart, Vector2 newEnd)
    {
        return newStart + (newEnd - newStart) * distancePercent;
    }
    public static float convertToRange(Vector2 vector, Vector2 curStart, Vector2 curEnd, float newLow, float newHigh)
    {
        //Error checking
        if (newLow > newHigh)
        {
            throw new System.ArgumentException("newLow (" + newLow + ") is higher than newHigh (" + newHigh + ")!");
        }
        //Get distance percent
        float startDistance = (curEnd - curStart).magnitude;
        float distancePercent = (vector - curStart).magnitude / startDistance;
        if (Vector3.Angle(vector - curStart, curEnd - curStart) > 90)
        {
            distancePercent = 0;
        }
        //Use it find value within new range
        return newLow + (newHigh - newLow) * distancePercent;
    }

    /// <summary>
    /// Give it two points that define the line,
    /// and the X value you'd like to know,
    /// and it will give you the corresponing Y value
    /// </summary>
    public static float getLineSegmentY(Vector2 p1, Vector2 p2, float x)
    {
        // y = ax + b
        // a = rise / run
        float rise = p2.y - p1.y;
        float run = p2.x - p1.x;
        float a = rise / run;
        // b = y - ax
        float b = p2.y - a * p2.x;
        //Now find y
        float y = a * x + b;
        return y;
    }

    /// <summary>
    /// Instantiates a GameObject so that it can be rewound.
    /// Only works on game objects that are "registered" to be rewound
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static GameObject Instantiate(GameObject prefab)
    {
        //Checks to make sure it's rewindable
        bool isContainer = prefab.containsSavables();
        bool isSavable = prefab.isSavable();
        if (!isContainer)
        {
            if (!isSavable)
            {
                throw new UnityException("Prefab " + prefab.name + " cannot be instantiated as a rewindable object because it does not have a RigidBody2D or a SavableMonoBehaviour.");
            }
            bool hasInfo = prefab.GetComponent<ObjectInfo>();
            if (!hasInfo)
            {
                throw new UnityException("Prefab " + prefab.name + " cannot be instantiated as a rewindable object because it does not have an ObjectInfo.");
            }
        }
        //Instantiate
        GameObject newObj = GameObject.Instantiate(prefab);
        int baseId = (int)System.DateTime.Now.Ticks;
        string spawnTag = "---" + baseId;
        newObj.name += spawnTag;
        if (isSavable)
        {
            SavableObjectInfo soi = newObj.GetComponent<SavableObjectInfo>();
            soi.Id = getUniqueId(baseId, 0);
            soi.spawnStateId = Managers.Rewind.GameStateId;
            Managers.Object.addNewObject(newObj);
            Managers.Scene.registerObjectInScene(newObj);
        }
        //Container children
        if (isContainer)
        {
            ISavableContainer container = newObj.GetComponent<ISavableContainer>();
            int nextId = 1;
            container.Savables.ForEach(savable =>
            {
                savable.name += spawnTag;
                SavableObjectInfo soi = savable.GetComponent<SavableObjectInfo>();
                soi.Id = getUniqueId(baseId, nextId);
                soi.spawnStateId = Managers.Rewind.GameStateId;
                nextId++;
                Managers.Object.addNewObject(savable);
                Managers.Scene.registerObjectInScene(savable);
            });
        }
        //Return spawned object
        return newObj;
    }
    static int getUniqueId(int baseId, int index)
        => Mathf.Abs(Mathf.Abs(baseId * 10) + index);



    public class RaycastAnswer
    {
        public RaycastHit2D[] rch2ds;
        public int count;

        public RaycastAnswer(RaycastHit2D[] rch2ds, int count)
        {
            this.rch2ds = rch2ds;
            this.count = count;
        }
    }

    static int maxReturnedList = 0;
    static RaycastHit2D[] rch2dsNonAlloc = new RaycastHit2D[MAX_HIT_COUNT];
    /// <summary>
    /// Test method to see how many objects are typically returned in a raycast call
    /// </summary>
    public static RaycastAnswer RaycastAll(Vector2 origin, Vector2 direction, float distance)
    {
        int count = Physics2D.RaycastNonAlloc(origin, direction, rch2dsNonAlloc, distance);
        checkMaxReturnedList("Utility.RaycastAll", count);
        return new RaycastAnswer(rch2dsNonAlloc, count);
    }
    public static int Cast(Collider2D coll2d, Vector2 direction, RaycastHit2D[] results = null, float distance = 0, bool ignoreSiblingColliders = true)
    {
        if (results == null)
        {
            results = rch2dsNonAlloc;
        }
        if (results.Length != MAX_HIT_COUNT)
        {
            throw new UnityException("Script using collider on object " + coll2d.gameObject.name + " is using result array != MAX_HIT_COUNT: " +
                "results.count: " + results.Length + ", MAX_HIT_COUNT: " + MAX_HIT_COUNT);
        }
        int count = 0;
        count = coll2d.Cast(direction, results, distance, ignoreSiblingColliders);
        checkMaxReturnedList("Utility.Cast", count);
        return count;
    }
    /// <summary>
    /// Returns a count and a list of colliders that collide with the given coll2d.
    /// NOTE: NOT thread safe
    /// </summary>
    /// <param name="coll2d"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <param name="ignoreSiblingColliders"></param>
    /// <returns></returns>
    public static RaycastAnswer CastAnswer(this Collider2D coll2d, Vector2 direction, float distance = 0, bool ignoreSiblingColliders = true)
    {
        int count = coll2d.Cast(direction, rch2dsNonAlloc, distance, ignoreSiblingColliders);
        checkMaxReturnedList("Utility.CastAnswer", count);
        return new RaycastAnswer(rch2dsNonAlloc, count);
    }
    /// <summary>
    /// Returns the number of solid non-trigger objects in the collider area
    /// </summary>
    /// <param name="coll2d"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <param name="ignoreSiblingColliders"></param>
    /// <returns></returns>
    public static int CastCountSolid(Collider2D coll2d, Vector2 direction, float distance = 0, bool ignoreSiblingColliders = true)
    {
        RaycastHit2D[] results = rch2dsNonAlloc;
        int count = 0;
        count = coll2d.Cast(direction, results, distance, ignoreSiblingColliders);
        checkMaxReturnedList("Utility.CastCountSolid", count);
        int solidCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (!results[i].collider.isTrigger)
            {
                solidCount++;
            }
        }
        return solidCount;
    }

    /// <summary>
    /// Returns true if the two colliders overlap
    /// </summary>
    /// <param name=""></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool OverlapsCollider(this Collider2D coll, Collider2D other)
    {
        //If the bounds don't overlap
        if (!coll.bounds.Intersects(other.bounds))
        {
            //then these colliders definitely don't overlap
            return false;
        }
        //Cast the first one to find if it has it
        RaycastAnswer answer = coll.CastAnswer(Vector2.zero, 0, false);
        for (int i = 0; i < answer.count; i++)
        {
            if (answer.rch2ds[i].collider == other)
            {
                return true;
            }
        }
        return false;
    }

    public static void checkMaxReturnedList(string methodName, int count)
    {
#if UNITY_EDITOR
        if (count > maxReturnedList)
        {
            maxReturnedList = count;
            Logger.log(
                Managers.Game,
                methodName + ": max list count: " + maxReturnedList
                );
        }
#endif
    }

    public static void copyTransform(Transform fromTransform, ref GameObject toObject)
    {
        toObject.transform.position = fromTransform.position;
        toObject.transform.rotation = fromTransform.rotation;
        toObject.transform.localScale = fromTransform.localScale;
    }

    public static bool between(float value, float bound1, float bound2)
    {
        return (value >= bound1 && value <= bound2)
            || (value >= bound2 && value <= bound1);
    }

    public static Color adjustAlpha(this Color c, float alpha)
        => new Color(c.r, c.g, c.b, alpha);

    public static Plane raycastPlane = new Plane(Vector3.forward, Vector3.zero);
    public static Vector2 ScreenToWorldPoint(Vector3 screenPoint)
    {
        //2019-01-28: copied from an answer by Tomer-Barkan: https://answers.unity.com/questions/566519/camerascreentoworldpoint-in-perspective.html
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        float distance;
        raycastPlane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }
}
