using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpDebuff : Debuff
{
    public JumpDebuff(float duration, float amountToEffect, DebuffSystem system, GameObject cause, System.Action functiontocallOndeath = null) 
        : base(duration, system, cause, functiontocallOndeath)
    {
        JumpEffect = amountToEffect;
    }

    public float JumpEffect { get; private set; }
}