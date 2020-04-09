using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFinder : MonoBehaviour
{
    [Tooltip("Ignores function calls and just looks at the player")]
    public bool ShouldTrackPlayer = false;
    [Tooltip("Speed of rotation")]
    public float TrackingSpeed = 100.0f;

    Player Player;
    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        Player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldTrackPlayer)
        {
            FindPlayer();
        }
    }

    //you can pass in a range to specify, if not range is pre set, 
    //DO NOT USE THIS FUNCTION UNLESS ITS BEING CALLED BY THIS SCRIPTS UPDATE
    public void FindPlayer(float range = 10.0f)
    {
        Collider outhit = new Collider();
        Collider[] Hits = Physics.OverlapSphere(this.gameObject.transform.position, range);
        foreach (Collider hit in Hits)
        {
            if (hit.tag == "Player")
            {
                TrackPlayer();
            }
        }
    }
    //you can pass in a range to specify, if not range is pre set
    public static bool FindPlayer(out Collider outhit, GameObject obj, float range = 10.0f)
    {
        outhit = new Collider();
        Collider[] Hits = Physics.OverlapSphere(obj.transform.position, range);
        foreach (Collider hit in Hits)
        {
            if (hit.tag == "Player")
            {
                Vector3 TargetPos = hit.transform.position;
                TargetPos.y = obj.transform.position.y;
                Vector3 targetDir = TargetPos - obj.transform.position;
                float angle = Vector3.Angle(targetDir, obj.transform.forward);
                if (Mathf.Abs(angle) < obj.GetComponent<BasicTurret>().FOV)
                {
                    outhit = hit;
                    return true;
                }
            }      
        }
        return false;
    }

    public void TrackPlayer()
    {
        Vector3 targetRot = (Player.transform.position - this.gameObject.transform.position).normalized;
        Quaternion LookRotation = Quaternion.LookRotation(targetRot);
        this.gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, LookRotation, Time.deltaTime * TrackingSpeed);
    }
}
