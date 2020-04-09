using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : BaseObject
{

    private void Start()
    {
        m_Controller = GameObject.Find("Controller").GetComponent<BreakoutController>();
        m_Rigidbody = GetComponent<Rigidbody2D>();

        m_InitialPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (GameStateTracker.Instance().CanUpdate())
        {
            Vector3 moveInput = m_Controller.GetMoveInput();

            Vector2 adjustedMoveInput = new Vector2(moveInput.x * m_Speed, moveInput.z * m_Speed);

            m_Rigidbody.velocity = adjustedMoveInput;
        }
        else
        {
            m_Rigidbody.velocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            BoxCollider2D boxCol = gameObject.GetComponent<BoxCollider2D>();
            CircleCollider2D circleCol = collision.gameObject.GetComponent<CircleCollider2D>();
            Ball ballObj = collision.gameObject.GetComponent<Ball>();

            RectTransform trans = gameObject.GetComponent<RectTransform>();
            RectTransform coltrans = collision.gameObject.GetComponent<RectTransform>();


            //Get the distance between the center of the ball and the right size of the paddle.
            float distance = trans.position.x - coltrans.position.x;

            float perc = distance / (boxCol.size.x * 0.5f);
            float angleInRadians;

            //Clamping rightest and leftest bounce, the value it can reach without any weird shit is -0.5 to 0.5 which is doubled after to get a full percent
            if (perc > 0.45f)
                perc = 0.45f;
            else if (perc < -0.45f)
                perc = -0.45f;


            perc *= 2.0f;

            //Generate angle based on percentage. Since Percent goes from (-1) - (1) theres a bit of fuckery here to handle quadrants
            if (perc > 0.0f)
            {
                angleInRadians = Mathf.PI + perc * Mathf.PI;
            }
            else
            {
                angleInRadians = Mathf.PI - Mathf.Abs(perc * Mathf.PI);
            }

            //get current ball speed
            float speed = ballObj.GetCurrentSpeed();

            //Adjust ball velocity accordingly
            ballObj.AssignVelocity(new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)).normalized * speed);
        }
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        transform.position = m_InitialPosition;
    }

    BreakoutController m_Controller;
    Rigidbody2D m_Rigidbody;

    public float m_Speed = 300;

    private Vector3 m_InitialPosition;
}
