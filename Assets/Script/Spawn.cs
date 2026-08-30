using UnityEngine;
using Unity.Cinemachine;

public class Spawner : MonoBehaviour
{
    [Header("Setting Entities")]
    public GameObject dungeon_room2; 
    public Transform Spawn_Point;   
    public CinemachineCamera virtualCamera;

    void Start()
    {
        SpawnEntity();
    }

    public void SpawnEntity()
    {
        if (dungeon_room2 != null && Spawn_Point != null)
        {
            GameObject SP = Instantiate(dungeon_room2, Spawn_Point.position, Spawn_Point.rotation);
        
            if (virtualCamera != null)
                {
                    virtualCamera.Follow = SP.transform;
                    virtualCamera.LookAt = SP.transform;
                }
        }
        else
        {
            Debug.LogWarning("Handling Error");
        }
    }
}
