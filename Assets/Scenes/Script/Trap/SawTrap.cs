using UnityEngine;

public class SawTrap : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public float rotationSpeed = 300f;

    private Vector3 startPosition;
    private Vector3 moveDirection;

    private bool moveRight = true;

    void Start()
    {
        startPosition = transform.position;

        // Save the original movement direction
        moveDirection = transform.right;
    }

    void Update()
    {
        MoveSaw();
        RotateSaw();
    }

    void MoveSaw()
    {
        if (moveRight)
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position -= moveDirection * moveSpeed * Time.deltaTime;
        }

        float distance = Vector3.Distance(
            startPosition,
            transform.position
        );

        if (distance >= moveDistance)
        {
            moveRight = !moveRight;
        }
    }

    void RotateSaw()
    {
        transform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }
}