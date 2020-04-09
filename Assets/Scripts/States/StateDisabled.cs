using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Disabled", order = 1)]
public class StateDisabled : StateBase
{

    public override void LateUpdate()
    {
        //throw new NotImplementedException();
    }
}
