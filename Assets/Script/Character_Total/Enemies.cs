using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform _TransFormToFollow;
    public NavMeshAgent _MyNavAgent;

    // void Start()
    // {
        
    // }
    // void Update()
    // {
    //     _MyNavAgent.SetDestination(_TransFormToFollow.position);

    // }
    void Start()
    {
        if (_TransFormToFollow == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _TransFormToFollow = playerObj.transform;
            }
        }
        
        // Khởi tạo NavMeshAgent (nếu chưa gán)
        if (_MyNavAgent == null)
        {
            _MyNavAgent = GetComponent<NavMeshAgent>();
        }
    }   
    void Update()
    {
        if (_MyNavAgent == null || !_MyNavAgent.isOnNavMesh || !_MyNavAgent.isActiveAndEnabled)
        {
            return; 

        if (_TransFormToFollow != null)
        {
            _MyNavAgent.SetDestination(_TransFormToFollow.position);
        }
        }
    }
}

/////Sample for Second MileStones - can be delete or recycle if others finished