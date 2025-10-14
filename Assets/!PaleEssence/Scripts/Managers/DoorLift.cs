using UnityEngine;

public class DoorLift : MonoBehaviour
{
    public GameObject objectToMove;

    public float liftHeight = 5f;

    public float moveSpeed = 2f;

    [Tooltip("Player gameObject tag")]
    public string playerTag = "Player";

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool isMovingUp = false;
    private bool isMovingDown = false;

    void Start()
    {
        if (objectToMove == null)
        {
            enabled = false;
            return;
        }

        initialPosition = objectToMove.transform.position;
        targetPosition = initialPosition + new Vector3(0, liftHeight, 0);
    }

    void Update()
    {
        if (isMovingUp)
        {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (objectToMove.transform.position == targetPosition)
            {
                isMovingUp = false;
            }
        }
        else if (isMovingDown)
        {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, initialPosition, moveSpeed * Time.deltaTime);

            if (objectToMove.transform.position == initialPosition)
            {
                isMovingDown = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isMovingUp = true;
            isMovingDown = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isMovingDown = true;
            isMovingUp = false; 
        }
    }

    void OnDrawGizmosSelected()
    {
        if (objectToMove != null)
        {
            Vector3 currentInitialPos = Application.isPlaying ? initialPosition : objectToMove.transform.position;
            Vector3 currentTargetPos = Application.isPlaying ? targetPosition : objectToMove.transform.position + new Vector3(0, liftHeight, 0);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentInitialPos, 0.2f); 

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentTargetPos, 0.2f); 

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(currentInitialPos, currentTargetPos); 
        }
    }
}