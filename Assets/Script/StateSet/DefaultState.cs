using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class IdleState : State 
{
    public bool canSeePlayer; 
    public ChaseState chaseState;

    public override State RunCurrentState()
    {
        if (canSeePlayer) {
            return chaseState;
        }
        return this; 
    }
}