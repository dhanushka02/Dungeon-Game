using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ChaseState : State
{
    public bool isInAttackRange; 
    public AttackState attackState; 

    public override State RunCurrentState() 
    {
        if (isInAttackRange) {
            return attackState;
        }
        
        // Logic di chuyển đuổi theo player viết ở đây
        Debug.Log("Found Target..."); 
        return this;
    }
}
