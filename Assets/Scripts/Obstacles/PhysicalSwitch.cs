using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioChannel))]
public class PhysicalSwitch : BaseSwitch
{
    [Tooltip("The object that will have its mesh changed (like the button part of the switch")]
    public GameObject MeshToChange = null;

    protected Renderer m_Renderer = null;

    [SerializeField, Tooltip("The Material the object will have before it is shot.")]
    protected Material ActiveMat = null;
    [SerializeField, Tooltip("The Material the object will have After it is shot.")]
    protected Material AfterShotMat = null;
    [SerializeField, Tooltip("The Material the object will have After it is turned off.")]
    protected Material DeactivatedMat = null;

    [SerializeField]
    protected ScriptableAudio OnPressAudio = null;
    protected AudioChannel m_AudioChannel = null;

    protected override void Awake()
    {
        base.Awake();
        m_Renderer = MeshToChange.GetComponent<Renderer>();

        if (ActiveMat == null)
        {
            Debug.LogError(gameObject.name + "'s ActiveMat has not been set");
        }
        else
        {
            m_Renderer.material = ActiveMat;
        }

        if (AfterShotMat == null)
        {
            Debug.LogWarning(gameObject.name + "'s AfterShotMat has not been set");
        }
        if (DeactivatedMat == null)
        {
            Debug.LogWarning(gameObject.name + "'s DeactivatedMat has not been set");
        }

        m_AudioChannel = GetComponent<AudioChannel>();
    }

    public override bool ToggleState()
    {
        if (base.ToggleState())
        {
            if (HasBeenActivated && ShouldBreakAfterActivation)
            {
                MeshToChange.GetComponent<Renderer>().material = DeactivatedMat;
            }
            else if(m_IsActive)
            {
                m_Renderer.material = ActiveMat;
            }
            else
            {
                m_Renderer.material = AfterShotMat;
            }

            if(OnPressAudio)
            {
                m_AudioChannel.PlayAudio(OnPressAudio);
            }

            return true;
        }

        return false;
    }

    protected void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.collider.gameObject);
    }

    private void HandleCollision(GameObject obj)
    {
        ShootBullet bullet = obj.GetComponent<ShootBullet>();

        if(bullet)
        {
            //make it so even turrets can shoot these for some reason
            bool didtoggle = ToggleState();

            //if we can't toggle in then it didn't get "hit" so don't continue, if it did toggle find out if the player was the one
            if(didtoggle)
            {
                if(bullet.ParentObject.name.ToUpper() == "PLAYER")
                {
                    CanBeShot.TriggerShotEvent();
                }
            }
        }

        //if (other.tag.ToUpper().Contains("PROJECTILE"))
        //{
        //    ToggleState();
        //}
    }


    protected override void Activate()
    {
        base.Activate();
    }

    protected override void Deactivate()
    {
        base.Deactivate();
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();
        MeshToChange.GetComponent<Renderer>().material = ActiveMat;
    }
}
