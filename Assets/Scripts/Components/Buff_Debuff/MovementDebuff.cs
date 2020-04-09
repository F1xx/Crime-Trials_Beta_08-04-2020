using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementDebuff : Debuff
{
    public MovementDebuff(float duration, float amountToEffect, DebuffSystem system, GameObject cause, System.Action functiontocallOndeath = null) 
        : base(duration, system, cause, functiontocallOndeath)
    {
        SpeedEffect = amountToEffect;
    }

    public float SpeedEffect { get; private set; }
}
