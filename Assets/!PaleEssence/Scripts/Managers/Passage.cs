using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.InputSystem;

public class Passage : MonoBehaviour
{
    [Header("Gate Settings")]
    [Tooltip("The transform of the child object that acts as the gate and will move.")]
    [SerializeField] private Transform gateTransfrom;

    [Tooltip("The height (in meters) to lift the gate to.")]
    [SerializeField] private float liftHeight = 5f;

    [Tooltip("The speed of the gate movement (in meters per second).")]
    [SerializeField] private float moveSpeed = 2f;

    public bool _isGateOpen = false;
    private bool _isMoving = false;
    private Vector3 _initialPosition;
    private Vector3 _targetPosition;
    private BoxCollider nonTriggerCollider;

    [Header("Player Transition Settings")]
    [Tooltip("Duration of the transition animation (opacity, collider disable)")]
    public float animationDuration = 1f;
    public float disableDuration = 2f;

    [SerializeField] private Material targetMaterial;
    [SerializeField] private Transform point1;
    [SerializeField] private Transform point2;
    private InputAction interactInput;
    private float interactTime = 0f;

    private CharacterController controller;
    private bool playerInTrigger = false;
    private Vector3 transitionDirection;


    void Start()
    {
        interactInput = InputSystem.actions.FindAction("Interact");

        if (gateTransfrom == null)
        {
            return;
        }
        _initialPosition = gateTransfrom.position;


        BoxCollider[] colliders = GetComponents<BoxCollider>();
        nonTriggerCollider = colliders.FirstOrDefault(col => !col.isTrigger);
    }

    void Update()
    {
        if (playerInTrigger && interactInput.IsPressed() && !_isMoving && interactTime < Time.time)
        {
            interactTime = Time.time + 5f;
            StartCoroutine(AnimateOpacity(0f, 1f));
            StartCoroutine(DisableAndEnableCollider());
            StartCoroutine(MovePlayerThrough(1.8f));
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MapGenerator.instance.currentActivePassage = gameObject;
            playerInTrigger = true;
            controller = other.GetComponent<CharacterController>();

            Vector3 playerPos = other.transform.position;

            if ((playerPos - point1.position).sqrMagnitude > (playerPos - point2.position).sqrMagnitude)
                transitionDirection = (point1.position - point2.position).normalized;
            else
                transitionDirection = (point2.position - point1.position).normalized;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }

    public void Open()
    {
        if (_isMoving || _isGateOpen) return;

        _targetPosition = _initialPosition + new Vector3(0, liftHeight, 0);
        _isGateOpen = true;
        if (GetComponent<AudioSource>()) GetComponent<AudioSource>().Play();
        StartCoroutine(MoveGate());
    }

    public void Close()
    {
        if (_isMoving || !_isGateOpen) return;

        _targetPosition = _initialPosition;
        _isGateOpen = false;
        if (GetComponent<AudioSource>()) GetComponent<AudioSource>().Play();
        StartCoroutine(MoveGate());
    }

    public void ToggleGate()
    {
        if (_isGateOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private IEnumerator MoveGate()
    {
        _isMoving = true;

        Vector3 startPosition = gateTransfrom.position;
        float journeyLength = Vector3.Distance(startPosition, _targetPosition);
        float journeyTime = journeyLength / moveSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < journeyTime)
        {
            float fractionOfJourney = elapsedTime / journeyTime;

            gateTransfrom.position = Vector3.Lerp(startPosition, _targetPosition, fractionOfJourney);

            elapsedTime += Time.deltaTime;
            yield return null;
        }


        gateTransfrom.position = _targetPosition;
        _isMoving = false;
    }


    IEnumerator MovePlayerThrough(float duration)
    {
        float timer = 0f;
        controller.gameObject.GetComponent<Animator>().applyRootMotion = true;
        controller.gameObject.GetComponent<Animator>().SetBool("Interacting", true);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        controller.gameObject.GetComponent<Animator>().applyRootMotion = false;
        controller.gameObject.GetComponent<Animator>().SetBool("Interacting", false);


        StartCoroutine(AnimateOpacity(1f, 0f));
    }

    IEnumerator AnimateOpacity(float startValue, float endValue)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);

            float currentOpacity = Mathf.Lerp(startValue, endValue, t);
            if (targetMaterial != null && targetMaterial.HasProperty("_Transperancy"))
            {
                targetMaterial.SetFloat("_Transperancy", currentOpacity);
            }

            yield return null;
        }

        if (targetMaterial != null && targetMaterial.HasProperty("_Transperancy"))
        {
            targetMaterial.SetFloat("_Transperancy", endValue);
        }
    }

    IEnumerator DisableAndEnableCollider()
    {
        if (nonTriggerCollider != null)
        {
            nonTriggerCollider.enabled = false;
            yield return new WaitForSeconds(disableDuration);
            nonTriggerCollider.enabled = true;
        }
    }


    [ContextMenu("Open Gate")]
    void TestOpen() => ToggleGate();
}