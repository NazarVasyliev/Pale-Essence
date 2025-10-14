using UnityEngine;

public class SwingingAxe : MonoBehaviour
{
    [Tooltip("The starting rotation angle of the axe from its rest axis. 0 = straight down.")]
    [Range(0f, 90f)]
    public float swingAngle = 45f;

    [Tooltip("The speed of the axe's rotation. Higher value means faster swinging.")]
    public float swingSpeed = 2f;

    [Tooltip("Delay before the first swing begins.")]
    public float startDelay = 0f;

    private float timer = 0f;
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < startDelay)
        {
            return;
        }


        float currentAngle = swingAngle * Mathf.Sin((timer - startDelay) * swingSpeed);



        transform.rotation = initialRotation * Quaternion.Euler(currentAngle, 0, 0);
    }


    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        Gizmos.color = Color.red;

        Vector3 pivot = transform.position;
        Vector3 start = pivot + (Quaternion.Euler(swingAngle, 0, 0) * Vector3.down * 0.5f);
        Vector3 end = pivot + (Quaternion.Euler(-swingAngle, 0, 0) * Vector3.down * 0.5f);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(pivot, 0.05f);
    }
}