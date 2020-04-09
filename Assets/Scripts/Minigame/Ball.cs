using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : BaseObject
{

    private void Start()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_CurrentDirection = Vector3.zero;
        m_CurrentSpeed = 0;

        m_RigidBody.gravityScale = 0.0f;
        m_RigidBody.velocity = Vector2.zero;

        m_InitialPosition = gameObject.transform.position;

        Listen("ToggleMinigameEasterEgg", ToggleBallActiveInactive);
    }

    private void FixedUpdate()
    {
        if (GameStateTracker.Instance().CanUpdate())
        {
            AssignVelocity(m_RigidBody.velocity.normalized * m_CurrentSpeed);
        }
    }

    private void ToggleBallActiveInactive(EventParam param)
    {
        MinigameParam eventParam = (MinigameParam)param;

        if (eventParam.GameState == GameStateTracker.eMinigameState.Playing)
        {
            GenerateRandomDirection();
            m_CurrentSpeed = SpeedOnStart;

            Vector2 convertedVelocity = new Vector2(m_CurrentDirection.x * m_CurrentSpeed, m_CurrentDirection.z * m_CurrentSpeed);

            AssignVelocity(convertedVelocity);
        }
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        gameObject.transform.position = m_InitialPosition;
        AssignVelocity(Vector2.zero);
        BounceCounter = 0;
    }

    private void GenerateRandomDirection()
    {
        float angle = Random.Range(randomLimit, 180.0f - randomLimit);
        float radians = angle * Mathf.Deg2Rad;
        m_CurrentDirection = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag != "Paddle")
        {
            BounceCounter++;
            m_CurrentSpeed = SpeedOnStart + (float)(SpeedIncreaseOnBounce * BounceCounter);

            Vector2 normal = collision.contacts[0].normal;
            Vector2 currentVelocity = LastKnownVelocity.normalized * (m_CurrentSpeed);

            if (normal.x > 0.0f && normal.x <= 1.0f)
            {
                currentVelocity.x = -currentVelocity.x;
            }
            else if (normal.x < 0.0f && normal.x >= -1.0f)
            {
                currentVelocity.x = -currentVelocity.x;
            }

            if (normal.y > 0.0f && normal.y <= 1.0f)
            {
                currentVelocity.y = -currentVelocity.y;
            }
            else if (normal.y < 0.0f && normal.y >= -1.0f)
            {
                currentVelocity.y = -currentVelocity.y;
            }

            AssignVelocity(currentVelocity);
        }
    }

    public void AssignVelocity(Vector2 newVelocity)
    {
        LastKnownVelocity = newVelocity;
        m_RigidBody.velocity = LastKnownVelocity;
    }

    public float GetCurrentSpeed()
    {
        return SpeedOnStart + (float)(SpeedIncreaseOnBounce * BounceCounter);
    }


    Vector3 m_CurrentDirection;
    Rigidbody2D m_RigidBody;
    float m_CurrentSpeed;

    public Vector2 LastKnownVelocity { get; private set; }
    public float SpeedOnStart = 5.0f;
    public float SpeedIncreaseOnBounce = 0.1f;
    int BounceCounter = 0;

    public Vector3 m_InitialPosition;
    
    //The max and min random starting angle.
    public float randomLimit = 20.0f;
}
