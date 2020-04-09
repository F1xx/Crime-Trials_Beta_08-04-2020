using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EndPoint : BaseObject
{
    [Header("Ribbon Stuff")]
    public GameObject Ribbon = null;
    public Material RibbonUntouchedMaterial = null;
    public Material RibbonTouchedMaterial = null;

    protected override void Awake()
    {
        base.Awake();
        Ribbon.GetComponent<Renderer>().material = RibbonUntouchedMaterial;
    }

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();

        if (player)
        {
            Ribbon.GetComponent<Renderer>().material = RibbonTouchedMaterial;
            PauseManager.Pause();

            OnLevelCompleteParam param = new OnLevelCompleteParam(WorldTimeManager.TimePassed);
            EventManager.TriggerEvent("OnLevelComplete", param);
        }
    }
}
