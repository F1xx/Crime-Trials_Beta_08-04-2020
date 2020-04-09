using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCrosshair : MonoBehaviour
{
    public Sprite[] Crosshairs = null;

    [Tooltip("The distance of the Raycast which will change the crosshair if aiming at a shootable.")]
    public float DetectionRange = 200.0f;
    [Tooltip("The radius of the Raycast which will change the crosshair if aiming at a shootable.")]
    public float DetectionRadius = 0.2f;
    [Tooltip("The color the crosshair normally is.")]
    public Color CrosshairDefaultColor = Color.white;
    [Tooltip("The color the crosshair is if aiming at something the player can shoot.")]
    public Color CrosshairDetectionColor = Color.red;

    public int CrosshairIndex = 0;

    Image CrosshairImage = null;
    public static RectTransform AimLocation { get; private set; }
    public static float Radius = 0.2f;

    private void Awake()
    {
        CrosshairImage = GetComponent<Image>();
        AimLocation = CrosshairImage.GetComponent<RectTransform>();
        CrosshairImage.sprite = Crosshairs[CrosshairIndex];
    }

    private void OnValidate()
    {
        Radius = DetectionRadius;
    }

    /// <summary>
    /// Used to set the crosshair to a different one in the array.
    /// Called when settings change.
    /// </summary>
    /// <param name="index">The array index</param>
    public void ChangeCrosshair(int index)
    {
        if(index > Crosshairs.Length)
        {
            Debug.LogError("Trying to access out of range crosshair");
            return;
        }
        CrosshairIndex = index;
        CrosshairImage.sprite = Crosshairs[CrosshairIndex];
    }

    void Update()
    {
        ChangeCrosshairIfAimingAtShootable();
    }

    void ChangeCrosshairIfAimingAtShootable()
    {
        //SphereCast forward from center of Crosshair to see if it is aiming at something we can shoot
        Ray ray = Camera.main.ViewportPointToRay(AimLocation.pivot);

        //we need to make it not collide with the player
        int layerMask = 1 << 9;
        layerMask = ~layerMask;
        List<RaycastHit> hitInfo = new List<RaycastHit>();
        if (MathUtils.SphereCastAllFromScreenCenterIgnoringNonVisibleObjects(Camera.main, ray, DetectionRadius, out hitInfo, DetectionRange, layerMask, "Shootable"))
        {
            //check if we hit a switch. If we did and we cannot activate it then do not turn red.
            foreach(RaycastHit hit in hitInfo)
            {
               Shootable shootable = hit.collider.transform.root.gameObject.GetComponentInChildren<Shootable>();

                if(shootable)
                {
                    if(shootable.GetCanBeShot() == false)
                    {
                        CrosshairImage.color = Color.white;
                        return;
                    }
                }
                else
                {
                    CrosshairImage.color = Color.white;
                    return;
                }
            }

            CrosshairImage.color = Color.red;
            return;
        }
        CrosshairImage.color = Color.white;
    }
}
