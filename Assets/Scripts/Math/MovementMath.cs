using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementMath
{
    /// <summary>
    /// used to determine how fast the player should be going, given the terrain. IE it will slow you down on slopes
    /// </summary>
    /// <param name="angleToGround">Your current angle relative to the ground (use AngleInDirection to get)</param>
    /// <param name="defaultSpeed">How fast do you want to be able to go if there is no slope in any direction. Used as the base speed that gets increased or decreased based on slope.</param>
    /// <param name="slopeLimit">The maximum slope the character can ever traverse.</param>
    /// <param name="minAngleToSlow">What angles do you care about. This will ignore any angle less than this angle or greater than this angle negative.</param>
    /// <returns></returns>
    public static float DetermineSpeedFromTerrain(float angleToGround, float defaultSpeed, float slopeLimit, float minAngleToSlow = 25.0f)
    {
        float currentSpeed = 0.0f;

        if (angleToGround > minAngleToSlow)
        {
            //Decrease speed if going uphill
            float diff = angleToGround / (slopeLimit * 2.0f);

            diff = Mathf.Clamp(diff, 0.0f, 1.0f);
            currentSpeed = defaultSpeed * (1 - diff);
        }
        else if (angleToGround < -minAngleToSlow)
        {
            //https://forum.unity.com/threads/tilt-floor-based-on-players-position.383548/
            //https://forum.unity.com/threads/character-controller-slide-down-how.383714/

            //Decrease speed if going uphill
            float diff = angleToGround * (slopeLimit * 2.0f);

            diff = Mathf.Clamp(diff, 1.0f, 1.09f);
            currentSpeed = defaultSpeed * (diff);
        }
        else
        {
            currentSpeed = defaultSpeed;
        }

        return currentSpeed;
    }

    /// <summary>
    /// Are you on a slope (up or down) based on the minimum angle you care to consider a slope.
    /// </summary>
    /// <param name="currentAngle">Your current angle relative to the ground (use AngleInDirection to get)</param>
    /// <param name="minAngleToCare">What angles do you care about. This will ignore any angle less than this angle or greater than this angle negative.</param>
    /// <returns>true if on a slope, false if not.</returns>
    public static bool IsOnSlope(float currentAngle, float minAngleToCare)
    {
        if (currentAngle > minAngleToCare || currentAngle < -minAngleToCare)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Uses your movement direction and the Slope direction to give you back a vector using the slope
    /// Highly recommended for movement down a slope, up is less important.
    /// </summary>
    /// <param name="moveDirection">The direction you are moving</param>
    /// <param name="slopeDirection">The direction of the slope you're on relative to your movedirection (use AngleInDirection to get)</param>
    /// <returns>A Vector3 with your new movement direction with the slope applied to it, normalized.</returns>
    public static Vector3 ApplySlopeToDirection(Vector3 moveDirection, Vector3 slopeDirection)
    {
        moveDirection = Vector3.Project(moveDirection, slopeDirection);
        moveDirection.Normalize();

        return moveDirection;
    }

    /// <summary>
    /// Does 2 Raycasts, 1 directly down from the GameObject and the other slightly forward in the direction passed in. Used to determine what angle the ground in the direction is.
    /// </summary>
    /// <param name="slopeDir">a variable so you can keep track of the slope.(needed for ApplySlopeToDirection</param>
    /// <param name="movableObject">The Gameobject we're testing on. It MUST have a collider.</param>
    /// <param name="direction">Whatever direction you want to test in. For a player that would be RelativeMoveDirection</param>
    /// <returns> a float that represents the angle of the ground relative to the GameObject based on the current movement direction. </returns>
    public static float AngleInDirection(out Vector3 slopeDir, GameObject movableObject, Vector3 direction)
    {
        Vector3 objectPos = movableObject.transform.position;

        float bottom = movableObject.GetComponent<Collider>().bounds.min.y;

        //center ray cast
        float dist = objectPos.y - bottom;
        RaycastHit centerHitInfo;
        Physics.Raycast(objectPos, -movableObject.transform.up, out centerHitInfo, dist + 0.2f);

        //Cast based on where the player is trying to move.
        Vector3 pos = movableObject.transform.position + (direction * 0.5f);
        RaycastHit directionHit;
        Physics.Raycast(pos, -movableObject.transform.up, out directionHit, dist + 0.6f);

        //set the slope
        Vector3 dir = directionHit.point - centerHitInfo.point;
        dir.Normalize();
        slopeDir = dir;

        float heightdif = directionHit.point.y - centerHitInfo.point.y;

        float angle = Vector3.Angle(movableObject.transform.up, directionHit.normal);

        if (heightdif < 0)
        {
            angle *= -1;
        }

        //positive if going up, negative if going down
        return angle;
    }
}
