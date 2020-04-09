using UnityEngine;
using System;
using System.Collections.Generic;

public class MathUtils
{
    public static float CompareEpsilon = 0.00001f;

    //Eases from the start to the end.  This is meant to be called over many frames.  The
    //values will change fast at first and gradually slow down.
    public static float LerpTo(float easeSpeed, float start, float end, float dt)
    {
        float diff = end - start;

        diff *= Mathf.Clamp(dt * easeSpeed, 0.0f, 1.0f);

        return diff + start;
    }

    //Eases angles from the start to the end.  This is meant to be called over many frames.  The
    //values will change fast at first and gradually slow down. 
    //This function was written assuming start and end are between 0 and 360
    public static float LerpToAngle(float easeSpeed, float start, float end, float dt)
    {
        float diff = end - start;

        if (diff > 180.0f)
        {
            diff = diff - 360.0f;
        }

        diff *= Mathf.Clamp(dt * easeSpeed, 0.0f, 1.0f);

        return diff + start;
    }

    //Eases from the start to the end.  This is meant to be called over many frames.  The
    //values will change fast at first and gradually slow down.
    public static Vector3 LerpTo(float easeSpeed, Vector3 start, Vector3 end, float dt)
    {
        Vector3 diff = end - start;

        diff *= Mathf.Clamp(dt * easeSpeed, 0.0f, 1.0f);

        return diff + start;
    }

    //Eases from the start to the end.  This is meant to be called over many frames.  The
    //values will change fast at first and gradually slow down.
    public static Vector3 SlerpTo(float easeSpeed, Vector3 start, Vector3 end, float dt)
    {
        float percent = Mathf.Clamp(dt * easeSpeed, 0.0f, 1.0f);

        return Vector3.Slerp(start, end, percent);
    }

    //Eases from the start to the end.  This is meant to be called over many frames.  The
    //values will change fast at first and gradually slow down.
    public static Vector3 SlerpToHoriz(float easeSpeed, Vector3 start, Vector3 end, Vector3 slerpCenter, float dt)
    {
        Vector3 startOffset = start - slerpCenter;
        Vector3 endOffset = end - slerpCenter;

        startOffset.y = 0.0f;
        endOffset.y = 0.0f;

        float percent = Mathf.Clamp(dt * easeSpeed, 0.0f, 1.0f);

        Vector3 result = Vector3.Slerp(startOffset, endOffset, percent) + slerpCenter;
        result.y = start.y;

        return result;
    }

    //Use this to compare floating point numbers, when you want to allow for a small degree of error
    public static bool AlmostEquals(float v1, float v2, float epsilon)
    {
        return Mathf.Abs(v2 - v1) <= epsilon;
    }

    //Use this to compare floating point numbers, when you want to allow for a small degree of error
    public static bool AlmostEquals(float v1, float v2)
    {
        return AlmostEquals(v1, v2, CompareEpsilon);
    }

    //Clamps a vector along the x-z plane
    public static Vector3 HorizontalClamp(Vector3 v, float maxLength)
    {
        float horizLengthSqrd = v.x * v.x + v.z * v.z;

        if (horizLengthSqrd <= maxLength * maxLength)
        {
            return v;
        }

        float horizLength = Mathf.Sqrt(horizLengthSqrd);

        v.x *= maxLength / horizLength;
        v.z *= maxLength / horizLength;

        return v;
    }

    //This function will project a point inside a capsule to the bottom of the capsule. 
    //The capsule is assumed to be oriented along the y-axis.
    public static Vector3 ProjectToBottomOfCapsule(
        Vector3 ptToProject,
        Vector3 capsuleCenter,
        float capsuleHeight,
        float capsuleRadius
        )
    {
        //Calculating the length of the line segment part of the capsule
        float lineSegmentLength = capsuleHeight - 2.0f * capsuleRadius;

        //Clamp line segment length
        lineSegmentLength = Math.Max(lineSegmentLength, 0.0f);
        
        //Calculate the line segment that goes along the capsules "Height"
        Vector3 bottomLineSegPt = capsuleCenter;
        bottomLineSegPt.y -= lineSegmentLength * 0.5f;

        //Get displacement from bottom of line segment
        Vector3 ptDisplacement = ptToProject - bottomLineSegPt;

        //Calculate needed distances
        float horizDistSqrd = ptDisplacement.x * ptDisplacement.x + ptDisplacement.z * ptDisplacement.z;
        
        float radiusSqrd = capsuleRadius * capsuleRadius;

        //The answer will be undefined if the pt is horizontally outside of the capsule
        if (horizDistSqrd > radiusSqrd)
        {
            return ptToProject;
        }

        //Calc projected pt
        float heightFromSegPt = -Mathf.Sqrt(radiusSqrd - horizDistSqrd);

        Vector3 projectedPt = ptToProject;
        projectedPt.y = bottomLineSegPt.y + heightFromSegPt;

        return projectedPt;
    }

    //Returns the angle from the horizontal plane of the direction in degrees.  
    //NOTE: This assumes the direction is normalized
    public static float CalcVerticalAngle(Vector3 dir)
    {
        //                 /|
        //                / |
        //               /  |
        //            h /   |
        //             /    | o
        //            /     |
        //           /ang   |
        //          /_______|
        //
        //sin(ang) = o/h, but since the dir is a unit vector h = 1
        //The angle will be: Asin(o)
        return Mathf.Rad2Deg * Mathf.Asin(dir.y);
    }

    public static Vector3 HorizontalClamp(Vector3 v, float minLength, float maxLength)
    {
        float horizLengthSqrd = v.x * v.x + v.z * v.z;

        if (horizLengthSqrd < minLength * minLength)
        {
            if (horizLengthSqrd > 0.0f)
            {
                float horizLength = Mathf.Sqrt(horizLengthSqrd);

                v.x *= minLength / horizLength;
                v.z *= minLength / horizLength;
            }
            else
            {
                //The direction of the vector is undefined in this case.  Choosing the z axis and using 
                //that.  
                v = Vector3.forward * minLength;
            }
        }
        else if (horizLengthSqrd > maxLength * maxLength)
        {
            float horizLength = Mathf.Sqrt(horizLengthSqrd);

            v.x *= maxLength / horizLength;
            v.z *= maxLength / horizLength;
        }

        return v;
    }

    public static bool CheckForGroundBelow(out RaycastHit hit, Vector3 ownerPos, Vector3 basePos, float sphereRadius, float minSurfaceAngle, int mask)
    {
        //Check for the ground below the player
        float halfCapsuleHeight = ownerPos.y - basePos.y;
        if(MathUtils.FloatCloseEnough(halfCapsuleHeight, 0.0f, 0.01f))
        {
            halfCapsuleHeight = basePos.y;
        }

        Vector3 rayStart = ownerPos;

        Vector3 rayDir = Vector3.down;

        //float rayDist = halfCapsuleHeight - sphereRadius;
        //float rayDist = Mathf.Abs(halfCapsuleHeight - sphereRadius);
        float rayDist = 1.0f;
        Debug.DrawLine(rayStart, rayStart + rayDir * rayDist);

        //Find all of the surfaces overlapping the sphere cast
        RaycastHit[] hitInfos = Physics.SphereCastAll(rayStart, sphereRadius, rayDir, rayDist, mask);

        //Get the closest surface that is acceptable to walk on.  The order of the 
        RaycastHit groundHitInfo = new RaycastHit();
        bool validGroundFound = false;
        float minGroundDist = float.MaxValue;

        foreach (RaycastHit hitInfo in hitInfos)
        {
            //Check the surface angle to see if it's acceptable to walk on.  
            //Also checking if the distance is zero I ran into a case where the sphere cast was hitting a wall and
            //returning weird results in the hit info.  Checking if the distance is greater than 0 eliminates this 
            //case. 
            float surfaceAngle = MathUtils.CalcVerticalAngle(hitInfo.normal);
            if (surfaceAngle < minSurfaceAngle || hitInfo.distance <= 0.0f)
            {
                continue;
            }

            if (hitInfo.distance < minGroundDist)
            {
                minGroundDist = hitInfo.distance;

                groundHitInfo = hitInfo;

                validGroundFound = true;
            }
        }

        hit = groundHitInfo;
        return validGroundFound;
    }

    /// <summary>
    /// A simple function that determines if 2 floats are close enough to be considered equal
    /// </summary>
    /// <param name="first">float 1</param>
    /// <param name="target">float 2</param>
    /// <param name="epsilon">How close they have to be to each other</param>
    /// <returns>true if they are close enough, false otherwise.</returns>
    public static bool FloatCloseEnough(float first, float target, float epsilon)
    {
        float dist = Mathf.Abs(first - target);
        return dist <= epsilon;
    }

    /// <summary>
    /// Check if a wall is nearby. Requires an object transform, a direction to check, how far to check and an optional filter tag.
    /// </summary>
    /// <param name="ObjectTransform"></param>
    /// <param name="Direction"></param>
    /// <param name="DistanceCheck"></param>
    /// <param name="TagToCompare"></param>
    /// <returns></returns>
    public static RaycastHit PerformWallCheck(Transform ObjectTransform, Vector3 Direction, float DistanceCheck, string TagToCompare = "WallRunnable")
    {
        RaycastHit ray;

        Debug.DrawLine(ObjectTransform.position, ObjectTransform.position + Direction * DistanceCheck, Color.magenta);

        Physics.Raycast(ObjectTransform.position, Direction, out ray, DistanceCheck);

        //We can only wallrun on things that have the right tag.
        if (ray.collider != null)
        {
            if (TagToCompare.Length != 0)
            {
                if (ray.collider.tag != TagToCompare)
                {
                    return new RaycastHit();
                }
            }
        }

        return ray;
    }

    /// <summary>
    /// Checks the 5 directions predetermined to see if the player is touching a wall that is viable for wallrunning.
    /// </summary>
    /// <param name="PlayerTransform">The transform of the player.</param>
    /// <param name="DistanceToCheck">The distance from the wall to check.</param>
    /// <param name="InRay">The ray returned by the check that will contain the wall to run on.</param>
    /// <param name="IsRight">Whether or not the wall is to the right or left of the player.</param>
    /// <param name="IsOnCooldown">Whether or not cooldown needs to be considered.</param>
    /// <param name="LastWallRanOn">The last object we ran on.</param>
    /// <param name="TagToCompare">The tag to compare against for considering wallrunning.</param>
    /// <returns></returns>
    public static bool CheckPlayerWallCollisions(Transform PlayerTransform, float DistanceToCheck, out RaycastHit InRay, out bool IsRight, Vector3 LastWallNormal, bool IsOnCooldown = false, string TagToCompare = "WallRunnable")
    {
        //our list of viable candidates
        List<RaycastHit> raycasts = new List<RaycastHit>();

        //Checking directions for wallrunning
        raycasts.Add(PerformWallCheck(PlayerTransform, (PlayerTransform.right).normalized, DistanceToCheck, TagToCompare));//Right prong
        raycasts.Add(PerformWallCheck(PlayerTransform, ((PlayerTransform.forward + PlayerTransform.right).normalized), DistanceToCheck, TagToCompare));//Forward-right prong
        raycasts.Add(PerformWallCheck(PlayerTransform, ((-PlayerTransform.right).normalized), DistanceToCheck, TagToCompare));//Left prong
        raycasts.Add(PerformWallCheck(PlayerTransform, ((PlayerTransform.forward - PlayerTransform.right).normalized), DistanceToCheck, TagToCompare));//Forward-left prong

        //filter our results
        float closestWall = DistanceToCheck;
        RaycastHit closestCast = new RaycastHit();

        //If both forward facing prongs have a viable wall then we're too close to a wall for wallrunning so we're going to exit out.
        if (IsViableWall(raycasts[1], IsOnCooldown, LastWallNormal) && IsViableWall(raycasts[3], IsOnCooldown, LastWallNormal))
        {
            InRay = closestCast;
            IsRight = false;
            return false;
        }

        foreach (var ray in raycasts)
        {
            //Ignore all rays that didn't hit shit
            if (IsViableWall(ray, IsOnCooldown, LastWallNormal) == false)
            {
                continue;
            }
            //track the closest collider
            else
            {
                //is this closer than the closest wall? Keep track of the latest distance and 
                if (closestWall > ray.distance)
                {
                    closestWall = ray.distance;
                    closestCast = ray;
                }
                else
                {
                    continue;
                }
            }
        }

        //Update our attached object
        var AttachedWallObject = closestCast;
        bool onRight = true;

        //Did we hit a wall or find our closest wall? lets do some wallrunning
        if (AttachedWallObject.collider != null)
        {
            //We only want to calculate the right side once so the player cant infinitely wallrun by turning back and forth.
            for (int i = 0; i < raycasts.Count; i++)
            {
                if (raycasts[i].point == AttachedWallObject.point)
                {
                    //All right side casts are in the first half of the list
                    onRight = i < (raycasts.Count * 0.5) ? true : false;
                    break;
                }
            }

            //Viable Wall. Switch to WallRunning and sets some WallRunning variables.
            IsRight = onRight;
            InRay = AttachedWallObject;
            return true;
        }

        IsRight = onRight;
        InRay = AttachedWallObject;
        return false;
    }

    /// <summary>
    /// Checks if the wall is a valid for wallrunning.
    /// </summary>
    /// <param name="HitInfo"></param>
    /// <param name="IsOnCooldown"></param>
    /// <param name="LastObjectNormal"></param>
    /// <returns></returns>
    public static bool IsViableWall(RaycastHit HitInfo, bool IsOnCooldown, Vector3 LastObjectNormal)
    {
        //nothing hit. not a wall
        if (HitInfo.collider == null)
            return false;

        //if the colliding object is the last wall we ran on and we're still on cooldown then ignore it.
        if (HitInfo.normal == LastObjectNormal)
        {
            if (IsOnCooldown)
            {
                return false;
            }
        }

        //collider isnt perpendicular to the ground then it isn't a wall.
        //if (Vector3.Dot(HitInfo.normal, Vector3.up) != 0)
        //{
        //    return false;
        //}

        //should be a wall
        return true;
    }


    /// <summary>
    /// Tries to convert a string to a Vector3. Format MUST be 0,0,0 as an example. any extra letter or numbers will fail.
    /// </summary>
    /// <param name="sVector">the string you want to convert</param>
    /// <param name="vec">the vector it converts. Will be 0,0,0 if fails to convert</param>
    /// <returns>true if successfully converts, false otherwise</returns>
    public static bool StringToVector3(string sVector, out Vector3 vec)
    {
        vec = Vector3.zero;

        string[] bits = sVector.Split(',');

        if (bits.Length != 3)
        {
            return false;
        }

        Vector3 tempvec;

        try
        {
            tempvec.x = float.Parse(bits[0]);
            tempvec.y = float.Parse(bits[1]);
            tempvec.z = float.Parse(bits[2]);
        }
        catch//failed to parse so user fucked up
        {
            return false;
        }

        vec = tempvec;

        return true;
    }

    //This formula was taken from here: http://en.wikipedia.org/wiki/Spherical_cap
    public static float CalcSphereCapVolume(float sphereRadius, float capHeight)
    {
        return (Mathf.PI * capHeight * capHeight / 3) * (3 * sphereRadius - capHeight);
    }

    public static float CalcSphereVolume(float sphereRadius)
    {
        return (Mathf.PI * 4.0f / 3.0f) * sphereRadius * sphereRadius * sphereRadius;
    }

    public static class TweenFuncs
    {

        public static float TweenFunc_Linear(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            float timeperc = (float)(timeElapsed / totalTime);
            if (timeperc > 1)
                timeperc = 1;
            return startValue + valueRange * timeperc;
        }

        public static float TweenFunc_LinearToTargetValue (float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            float difference = valueRange - startValue;

            float timeperc = (float)(timeElapsed / totalTime);
            if (timeperc > 1)
                timeperc = 1;
            return startValue + difference * timeperc;
        }

        public static float TweenFunc_SineEaseIn(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            float timeperc = (float)(timeElapsed / totalTime);
            if (timeperc > 1)
                timeperc = 1;
            return startValue + -valueRange * Mathf.Cos(timeperc * Mathf.PI / 2) + valueRange;
        }

        public static float TweenFunc_SineEaseOut(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            float timeperc = (float)(timeElapsed / totalTime);
            if (timeperc > 1)
                timeperc = 1;
            return startValue + valueRange * Mathf.Sin(timeperc * Mathf.PI / 2);
        }

        public static float TweenFunc_SineEaseInOut(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            float timeperc = (float)(timeElapsed / totalTime);
            if (timeperc > 1)
                timeperc = 1;
            return startValue + -valueRange / 2 * (Mathf.Cos(timeperc * Mathf.PI) - 1);
        }

        public static float TweenFunc_BounceEaseIn(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            return (float)BounceEaseIn(timeElapsed, startValue, valueRange, totalTime);
        }

        public static float TweenFunc_BounceEaseOut(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            return (float)BounceEaseOut(timeElapsed, startValue, valueRange, totalTime);
        }

        public static float TweenFunc_BounceEaseInOut(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            return (float)BounceEaseInOut(timeElapsed, startValue, valueRange, totalTime);
        }

        public static float TweenFunc_ElasticEaseIn(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            return (float)ElasticEaseIn(timeElapsed, startValue, valueRange, totalTime, 3);
        }

        public static float TweenFunc_ElasticEaseOut(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            return (float)ElasticEaseOut(timeElapsed, startValue, valueRange, totalTime, 3);
        }

        public static float TweenFunc_ElasticEaseInOut(float startValue, float valueRange, float timeElapsed, float totalTime)
        {
            return (float)ElasticEaseInOut(timeElapsed, startValue, valueRange, totalTime, 3);
        }

        public static float BounceEaseIn(float t, float b, float c, float d)
        {
            return c - BounceEaseOut(d - t, 0, c, d) + b;
        }

        public static float BounceEaseOut(float t, float b, float c, float d)
        {
            if ((t /= d) < (1 / 2.75f))
            {
                return c * (7.5625f * t * t) + b;
            }
            else if (t < (2 / 2.75f))
            {
                float postFix = t -= (1.5f / 2.75f);
                return c * (7.5625f * (postFix) * t + .75f) + b;
            }
            else if (t < (2.5 / 2.75))
            {
                float postFix = t -= (2.25f / 2.75f);
                return c * (7.5625f * (postFix) * t + .9375f) + b;
            }
            else
            {
                float postFix = t -= (2.625f / 2.75f);
                return c * (7.5625f * (postFix) * t + .984375f) + b;
            }
        }

        public static float BounceEaseInOut(float t, float b, float c, float d)
        {
            if (t < d / 2)
                return BounceEaseIn(t * 2, 0, c, d) * .5f + b;
            else
                return BounceEaseOut(t * 2 - d, 0, c, d) * .5f + c * .5f + b;
        }

        public static float ElasticEaseIn(float t, float b, float c, float d, float elasticity)
        {
            if (t == 0)
                return b;
            if (t > d)
                return b + c;
            if ((t /= d) == 1)
                return b + c;

            float p = d * 0.3f;
            float a = c;
            float s = p / 4;

            float postFix = a * Mathf.Pow(10, elasticity * (t -= 1));

            return -(postFix * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + b;
        }

        public static float ElasticEaseOut(float t, float b, float c, float d, float elasticity)
        {
            if (t == 0)
                return b;
            if (t > d)
                return b + c;
            if ((t /= d) == 1)
                return b + c;

            float p = d * 0.3f;
            float a = c;
            float s = p / 4;

            return (a * Mathf.Pow(10, -elasticity * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + c + b);
        }

        public static float ElasticEaseInOut(float t, float b, float c, float d, float elasticity)
        {
            if (t == 0)
                return b;
            if (t > d)
                return b + c;
            if ((t /= d / 2) == 2)
                return b + c;

            float p = d * (0.3f * 1.5f);
            float a = c;
            float s = p / 4;
            float postFix;

            if (t < 1)
            {
                postFix = a * Mathf.Pow(10, elasticity * (t -= 1));
                return -0.5f * (postFix * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + b;
            }

            postFix = a * Mathf.Pow(10, -elasticity * (t -= 1));
            return postFix * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) * 0.5f + c + b;
        }
    }

    /// <summary>
    /// returns true if values are different
    /// </summary>
    /// <typeparam name="T">A type where T is of type System.IComparable</typeparam>
    /// <param name="first">The first of 2 values</param>
    /// <param name="second">The second of 2 values</param>
    /// <returns>true if they are different</returns>
    public static bool CheckIfValuesAreDifferent<T>(T first, T second) where T : System.IComparable<T>
    {
        if (first.Equals(second))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// uses given information to SphereCast from the center of the screen outwards from the main camera
    /// </summary>
    /// <param name="ray">The Ray to be used with the cast</param>
    /// <param name="radius">radius of the spherecast</param>
    /// <param name="hitInfo">a variable that will hold information if we hit anything</param>
    /// <param name="distance">length of the cast</param>
    /// <param name="tagToCompare">if you only want to hit things with a certain tag thats this</param>
    /// <returns>true if hit</returns>
    public static bool SphereCastFromScreenCenter(Ray ray, float radius, out RaycastHit hitInfo, float distance = float.MaxValue, string tagToCompare = "")
    {
        if (Physics.SphereCast(ray, radius, out hitInfo, distance))
        {
            //if they passed in the tag return if we hit something with said tag
            if (string.IsNullOrEmpty(tagToCompare) == false)
            {
                return CheckForTagInParentAndChildren(hitInfo.collider.gameObject, tagToCompare);
            }

            //otherwise return true as we did collide.
            return true;
        }
        //hit nothing
        return false;
    }

    public static bool SphereCastFromScreenCenter(Ray ray, float radius, float distance = float.MaxValue, string tagToCompare = "")
    {
        RaycastHit hitInfo = new RaycastHit();
        return SphereCastFromScreenCenter(ray, radius, out hitInfo, distance, tagToCompare);
    }



    /// <summary>
    /// uses given information to spherecastAll from the center of the screen outwards from the main camera.
    /// Note this is the version that does NOT stop when it first hits something
    /// </summary>
    /// <param name="ray">The Ray to be used with the cast</param>
    /// <param name="radius">radius of the spherecast</param>
    /// <param name="hitInfo">a List variable that will hold information if we hit anything. 
    /// Holds every relevant hit (if you use a tag it will only hold tagged objects)</param>
    /// <param name="distance">length of the cast</param>
    /// <param name="tagToCompare">if you only want to hit things with a certain tag thats this</param>
    /// <returns>true if hit</returns>
    public static bool SphereCastAllFromScreenCenter(Ray ray, float radius, out List<RaycastHit> hitInfo, float distance = float.MaxValue, string tagToCompare = "")
    {
        hitInfo = new List<RaycastHit>();
        RaycastHit[] tempHitInfo;

        tempHitInfo = Physics.SphereCastAll(ray, radius, distance);

        if (tempHitInfo.Length > 0)
        {
            //if they passed in the tag return if we hit something with said tag
            if (string.IsNullOrEmpty(tagToCompare) == false)
            {
                bool hitTag = false;
                foreach (RaycastHit hit in tempHitInfo)
                {
                    //if it had the tag we're searching for add it to the list
                    if (CheckForTagInParentAndChildren(hit.collider.gameObject, tagToCompare))
                    {
                        hitTag = true;
                        hitInfo.Add(hit);
                    }
                }

                return hitTag;
            }

            //otherwise return true as we did collide.
            foreach (RaycastHit hit in tempHitInfo)
            {
                hitInfo.Add(hit);
            }
            return true;
        }
        //hit nothing
        return false;
    }
    public static bool SphereCastAllFromScreenCenter(Ray ray, float radius, float distance = float.MaxValue, string tagToCompare = "")
    {
        List<RaycastHit> hit = new List<RaycastHit>();
        return SphereCastAllFromScreenCenter(ray, radius, out hit, distance, tagToCompare);
    }


    /// <summary>
    /// Same as SphereCastAllFromScreenCenter but also checks that what it counts as "hits" are visible on-screen
    /// </summary>
    /// <param name="camera">The camera that the objects in question must be visible to</param>
    /// <param name="ray">The Ray to be used with the cast</param>
    /// <param name="radius">radius of the spherecast</param>
    /// <param name="hitInfo">a List variable that will hold information if we hit anything. 
    /// Holds every relevant hit (if you use a tag it will only hold tagged objects)</param>
    /// <param name="distance">length of the cast</param>
    /// <param name="layerMask">Since this needs to do a standard Raycast to detect if its 
    /// on screen you may need to ignore certain layers to ensure the Raycast isn't needlessly blocked. For example you often do NOT want to hit the Player.</param>
    /// <param name="tagToCompare">if you only want to hit things with a certain tag thats this</param>
    /// <returns>true if hit</returns>
    /// <returns></returns>
    public static bool SphereCastAllFromScreenCenterIgnoringNonVisibleObjects(Camera camera, Ray ray, float radius, out List<RaycastHit> hitInfo, [UnityEngine.Internal.DefaultValue("Mathf.Infinity")] float distance, [UnityEngine.Internal.DefaultValue("DefaultRaycastLayers")] int layerMask, string tagToCompare = "")
    {
        hitInfo = new List<RaycastHit>();
        RaycastHit[] tempHitInfo;

        tempHitInfo = Physics.SphereCastAll(ray, radius, distance);

        if (tempHitInfo.Length > 0)
        {
            //if they passed in the tag return if we hit something with said tag
            if (string.IsNullOrEmpty(tagToCompare) == false)
            {
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
                bool hitTag = false;
                foreach (RaycastHit hit in tempHitInfo)
                {
                    //if it had the tag we're searching for add it to the list
                    if (CheckForTagInParentAndChildren(hit.collider.gameObject.transform.root.gameObject, tagToCompare))
                    {
                        //is it in the view plane of the camera?
                        if (GeometryUtility.TestPlanesAABB(planes, hit.collider.bounds))
                        {
                            Ray OnScreenRay = new Ray(ray.origin, hit.point - ray.origin);
                            RaycastHit OnScreenHit = new RaycastHit();
                            //Raycast at the point we hit it to see if it is blocked
                            if(Physics.Raycast(OnScreenRay, out OnScreenHit, distance, layerMask))
                            {
                                if (OnScreenHit.collider.gameObject == hit.collider.gameObject)
                                {
                                    hitTag = true;
                                    hitInfo.Add(hit);
                                }
                            }
                        }
                    }
                }

                return hitTag;
            }

            //otherwise return true as we did collide.
            foreach (RaycastHit hit in tempHitInfo)
            {
                hitInfo.Add(hit);
            }
            return true;
        }
        //hit nothing
        return false;
    }

    /// <summary>
    /// Same as SphereCastAllFromScreenCenter but also checks that what it counts as "hits" are visible on-screen
    /// </summary>
    /// <param name="camera">The camera that the objects in question must be visible to</param>
    /// <param name="ray">The Ray to be used with the cast</param>
    /// <param name="radius">radius of the spherecast</param>
    /// <param name="distance">length of the cast</param>
    /// <param name="layerMask">Since this needs to do a standard Raycast to detect if its 
    /// on screen you may need to ignore certain layers to ensure the Raycast isn't needlessly blocked. For example you often do NOT want to hit the Player.</param>
    /// <param name="tagToCompare">if you only want to hit things with a certain tag thats this</param>
    /// <returns>true if hit</returns>
    /// <returns></returns>
    public static bool SphereCastAllFromScreenCenterIgnoringNonVisibleObjects(Camera camera, Ray ray, float radius, [UnityEngine.Internal.DefaultValue("Mathf.Infinity")] float distance, [UnityEngine.Internal.DefaultValue("DefaultRaycastLayers")] int layerMask, string tagToCompare = "")
    {
        List<RaycastHit> hitInfo = new List<RaycastHit>();
        return SphereCastAllFromScreenCenterIgnoringNonVisibleObjects(camera, ray, radius, out hitInfo, distance, layerMask, tagToCompare);
    }

    public static bool CheckForTagInParentAndChildren(GameObject target, string tag)
    {
        //check if the parent was tagged
        if (target.CompareTag(tag))
        {
            return true;
        }
        //otherwise check if any child was tagged as that counts
        else
        {
            foreach (Transform child in target.transform)
            {
                if (child.CompareTag(tag))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Pass in any int and this will pass you out a string with that int followed by the appropriate ordinal like 1st, 2nd, 3rd, 4th
    /// </summary>
    /// <param name="num">Int to be ordinalized</param>
    /// <returns>a string with that int followed by the appropriate ordinal like 1st, 2nd, 3rd, 4th</returns>
    public static string AddOrdinal(int num)
    {
        //negatives have no ordinal but also should never happen
        if (num <= 0) return num.ToString();

        //mod 100 to ensure we always get any of these numbers even if its 984654611 (we just need the 11 at the end)
        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return num + "th";
        }

        //Mod 10 to just get the digit in the 1's place. 1,2,3 have special endings
        switch (num % 10)
        {
            case 1:
                return num + "st";
            case 2:
                return num + "nd";
            case 3:
                return num + "rd";
            default:
                return num + "th";
        }

    }
}
