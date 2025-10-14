using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;

public class ShrinkAndDestroy : MonoBehaviour
{
    [Tooltip("Delay in seconds before the shrink process begins")]
    public float delayBeforeShrink = 2.0f;

    [Tooltip("Duration of the shrinking process in seconds")]
    public float shrinkDuration = 1.5f;

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    public void StartShrink()
    {
        var obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle != null) obstacle.carving = false;
        StartCoroutine(Shrink());
    }
    private IEnumerator Shrink()
    {
        yield return new WaitForSeconds(delayBeforeShrink);

        float timer = 0f;
        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, timer / shrinkDuration);
            yield return null;
        }

        Destroy(gameObject);
    }
}