using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Player Movement")]
    [Tooltip("Move speed of the character in m/s.")]
    public float MoveSpeed = 4.0f;
    [Tooltip("Sprint speed of the character in m/s.")]
    public float SprintSpeed = 6.0f;
    [Tooltip("How fast the character turns to face movement direction.")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [Tooltip("Acceleration and deceleration.")]
    public float SpeedChangeRate = 10.0f;

    [Header("Player Jumping & Gravity")]
    [Tooltip("The height the player can jump.")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f.")]
    public float Gravity = -9.81f;
    [Tooltip("Time required to pass before being able to jump again.")]
    public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs.")]
    public float FallTimeout = 0.15f;
    private float targetTimeout = 0f;

    [Header("Player Grounded Check")]
    [Tooltip("If the character is grounded or not.")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground.")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController.")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground.")]
    public LayerMask GroundLayers;

    [Header("Audio")]
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float LandAudioVolume = 0.5f;
    [Range(0, 1)] public float FootstepAudioVolume = 0.2f;
    [Range(0, 1)] public float SprintStepAudioVolume = 0.5f;

    [Header("Targeting")]
    [Tooltip("Radius for enemy detection.")]
    public float detectionRadius = 10f;
    [Tooltip("Layer mask for enemies.")]
    public LayerMask enemyLayerMask;
    [Tooltip("Delay between switching targets.")]
    public float targetTimeDelay = 1f;

    [Header("Dodge")]
    [SerializeField] private float _dodgeSpeed = 5.0f;
    [SerializeField] private float _dodgeDuration = 1.1f;
    [SerializeField] private AnimationCurve _dodgeSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);


    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;
    private CinemachineOrbitalFollow _cmOF;
    private PlayerStats _playerStats;
    private Melee _melee;

    // Player state
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private bool _hasAnimator;
    public bool canMove = true;
    private bool _jumping = false;

    // Timers
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // Targeting
    private bool _targeted = false;
    private float _targetDelay = 0;
    [SerializeField] private GameObject _closestEnemy;
    private GameObject _targetObject;

    // Dodge
    private bool _dodging = false;
    private float _dodgeCooldownTimer = 0.0f;
    private float _dodgeStartTime = 0;
    private Vector3 _dodgeDirection;

    // Dash Attack
    public bool _dashing = false;
    private bool _breakRoll = false;
    private float _dashStartTime;
    private Vector3 _dashDirection;

    // Camera
    public GameObject orbitalCamera;
    public GameObject lockCamera;
    public bool LockCameraPosition = false;
    private bool _isRecentering = false;

    // Animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDDirectionX;
    private int _animIDDirectionY;
    private int _animIDRoll;

    // Input Actions
    private InputAction _moveInput;
    private InputAction _lookInput;
    private InputAction _jumpInput;
    private InputAction _sprintInput;
    private InputAction _interactInput;
    private InputAction _targetInput;
    private InputAction _dodgeInput;

    // Input Values
    private Vector2 _moveValue;
    private Vector2 _lookValue;
    private bool _jumpValue;
    private bool _sprintValue;
    private bool _targetValue;
    private bool _dodgeValue;
    private bool _interactValue;

    #endregion

    #region Unity Lifecycle Methods

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        _controller = GetComponent<CharacterController>();
        _hasAnimator = TryGetComponent(out _animator);
        _playerStats = GetComponent<PlayerStats>();
        _melee = GetComponentInChildren<Melee>();
    }

    private void Start()
    {
        // Input setup
        _moveInput = InputSystem.actions.FindAction("Move");
        _lookInput = InputSystem.actions.FindAction("Look");
        _jumpInput = InputSystem.actions.FindAction("Jump");
        _sprintInput = InputSystem.actions.FindAction("Sprint");
        _interactInput = InputSystem.actions.FindAction("Interact");
        _targetInput = InputSystem.actions.FindAction("Target");
        _dodgeInput = InputSystem.actions.FindAction("Dodge");

        // Cinemachine setup
        orbitalCamera = GameObject.Find("FreeLook Camera");
        orbitalCamera.GetComponent<CinemachineCamera>().Follow = transform;
        orbitalCamera.GetComponent<CinemachineCamera>().LookAt = transform;

        lockCamera = GameObject.Find("HardLookAt Camera");
        lockCamera.GetComponent<CinemachineCamera>().Follow = transform;
        lockCamera.GetComponent<CinemachineCamera>().LookAt = transform;

        _cmOF = orbitalCamera.GetComponent<CinemachineOrbitalFollow>();

        // Initialization
        AssignAnimationIDs();
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        ReadInputs();
        GroundedCheck();
        JumpAndGravity();
        TargetLock();
        DodgeController();
        DodgeRoll();

        if (canMove)
        {
            Move();
        }
    }

    private void LateUpdate()
    {
        // Camera logic that should run after all other updates
        // Recentering(); // not working
    }

    #endregion

    #region Initialization

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDDirectionX = Animator.StringToHash("DirectionX");
        _animIDDirectionY = Animator.StringToHash("DirectionY");
        _animIDRoll = Animator.StringToHash("Roll");
    }

    #endregion

    #region Input Handling

    private void ReadInputs()
    {
        _moveValue = _moveInput.ReadValue<Vector2>();
        _lookValue = _lookInput.ReadValue<Vector2>();
        _jumpValue = _jumpInput.IsPressed();
        _sprintValue = _sprintInput.IsPressed();
        _targetValue = _targetInput.IsPressed();
        _interactValue = _interactInput.IsPressed();
        _dodgeValue = _dodgeInput.IsPressed();
    }

    #endregion

    #region Core Mechanics

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void Move()
    {
        float targetSpeed = (_sprintValue && _playerStats.TryUseStamina(12f * Time.deltaTime)) ? SprintSpeed : MoveSpeed;
        if (_moveValue == Vector2.zero) targetSpeed = 0.0f;

        // Update Blend Tree for strafing in target lock mode
        if (_targeted && _speed <= MoveSpeed)
        {
            _animator.SetFloat(_animIDDirectionX, Mathf.Lerp(_animator.GetFloat("DirectionX"), _moveValue.x, Time.deltaTime * 10f));
            _animator.SetFloat(_animIDDirectionY, Mathf.Lerp(_animator.GetFloat("DirectionY"), _moveValue.y, Time.deltaTime * 10f));
        }
        else
        {
            _animator.SetFloat(_animIDDirectionX, 0);
            _animator.SetFloat(_animIDDirectionY, 1);
        }

        // Smooth speed change
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = _moveValue.magnitude;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // Rotation and movement
        Vector3 inputDirection = new Vector3(_moveValue.x, 0.0f, _moveValue.y).normalized;
        if (_moveValue != Vector2.zero || _targeted)
        {
            Rotation(inputDirection);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // Movement in strafe mode or normal mode
        if (_targeted && _speed <= MoveSpeed)
        {
            _controller.Move(Quaternion.Euler(0.0f, _targetRotation, 0.0f) * inputDirection * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else
        {
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        // Update animator
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded && !_dodging && !_dashing)
        {
            _fallTimeoutDelta = FallTimeout;
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            if (_jumpTimeoutDelta <= 0) _jumping = false;

            // Jump
            if (_jumpValue && _jumpTimeoutDelta <= 0.0f && _playerStats.TryUseStamina(25f) && !_jumping)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _jumping = true;
                _jumpTimeoutDelta = JumpTimeout;
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else if (!_dodging && !_dashing)
        {
            _jumpTimeoutDelta = JumpTimeout;
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else if (_hasAnimator)
            {
                _animator.SetBool(_animIDFreeFall, true);
            }
            _jumpValue = false;
        }

        // Apply gravity
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void Rotation(Vector3 inputDirection)
    {
        if (_targeted && _speed <= MoveSpeed)
        {
            // Turn to face the camera (for strafing)
            _targetRotation = _mainCamera.transform.eulerAngles.y;
        }
        else
        {
            // Turn in the direction of movement
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
        }

        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
    }

    #endregion

    #region Combat & Targeting

    private void TargetLock()
    {
        if (_targetValue && targetTimeout < Time.time || (_lookValue != Vector2.zero && _targeted))
        {
            targetTimeout = Time.time + .5f;
            if (_targetValue && _targeted)
            {
                UnlockTarget();
            }
            else
            {
                EnemyDetection();
            }
            _targetValue = false;
        }
        if (_targeted && lockCamera.GetComponent<CinemachineCamera>()?.LookAt == null)
        {
            UnlockTarget();
        }
    }

    private void UnlockTarget()
    {
        if (lockCamera.GetComponent<CinemachineCamera>()?.LookAt != null)
        {
            lockCamera.GetComponent<CinemachineCamera>().LookAt.gameObject.GetComponent<Outline>().enabled = false;
        }
        lockCamera.GetComponent<CinemachineCamera>().Priority = 2; // Return priority to the normal camera
        _targeted = false;
    }

    private void EnemyDetection()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayerMask);
        GameObject foundEnemy = null;

        if (!_targetValue)
        {
            foundEnemy = FindNextClosestEnemy(hitColliders);
        }
        else
        {
            foundEnemy = FindClosestEnemyToCenter(hitColliders);
        }

        if (foundEnemy != null)
        {
            _closestEnemy = foundEnemy;
            lockCamera.GetComponent<CinemachineCamera>().LookAt = _closestEnemy.transform;
            _targetObject = _closestEnemy;
            lockCamera.GetComponent<CinemachineCamera>().Priority = 6; // Increase priority of the lock-on camera
            _closestEnemy.GetComponent<Outline>().enabled = true;
            _targeted = true;
        }
    }

    private GameObject FindClosestEnemyToCenter(Collider[] hitColliders)
    {
        GameObject closest = null;
        float closestDistanceToCenter = float.MaxValue;
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Vector3 viewportPosition = _mainCamera.GetComponent<Camera>().WorldToViewportPoint(hitCollider.transform.position);

                if (viewportPosition.x >= 0 && viewportPosition.x <= 1 && viewportPosition.y >= 0 && viewportPosition.y <= 1 && viewportPosition.z > 0)
                {
                    float distanceToCenter = Vector2.Distance(new Vector2(viewportPosition.x, viewportPosition.y), new Vector2(0.5f, 0.5f));
                    if (distanceToCenter < closestDistanceToCenter)
                    {
                        closestDistanceToCenter = distanceToCenter;
                        closest = hitCollider.gameObject;
                    }
                }
            }
        }
        return closest;
    }


    private GameObject FindNextClosestEnemy(Collider[] hitColliders)
    {
        if (Time.time < _targetDelay) return null;

        GameObject nextEnemy = null;
        float closestDistanceToCenter = float.MaxValue;

        foreach (Collider hitCollider in hitColliders)
        {
            GameObject enemy = hitCollider.gameObject;
            if (enemy.CompareTag("Enemy") && enemy.transform != lockCamera.GetComponent<CinemachineCamera>().LookAt)
            {
                Vector3 viewportPosition = _mainCamera.GetComponent<Camera>().WorldToViewportPoint(enemy.transform.position);
                bool isInView = viewportPosition.y >= 0 && viewportPosition.y <= 1 && viewportPosition.z > 0;
                bool isRight = viewportPosition.x > 0.5f && _lookValue.x > 0;
                bool isLeft = viewportPosition.x < 0.5f && _lookValue.x < 0;

                if (isInView && (isRight || isLeft))
                {
                    float distanceToCenter = Mathf.Abs(viewportPosition.x - 0.5f);
                    if (distanceToCenter < closestDistanceToCenter)
                    {
                        closestDistanceToCenter = distanceToCenter;
                        nextEnemy = enemy;
                    }
                }
            }
        }

        if (nextEnemy != null)
        {
            if (lockCamera.GetComponent<CinemachineCamera>().LookAt != null)
            {
                lockCamera.GetComponent<CinemachineCamera>().LookAt.gameObject.GetComponent<Outline>().enabled = false;
            }
            _targetDelay = Time.time + targetTimeDelay;
        }

        return nextEnemy;
    }


    #endregion

    #region Dodging & Dashing

    private void DodgeController()
    {
        if (_dodgeValue && !_dodging && (_breakRoll || !_dashing) && Grounded && _playerStats.TryUseStamina(33f))
        {
            if (_breakRoll) _dashing = false;
            _dodgeDirection = (_moveValue != Vector2.zero) ? new Vector3(_moveValue.x, 0.0f, _moveValue.y).normalized : new Vector3(0, 0, -1);
            _dodgeStartTime = Time.time;
            _dodging = true;
            _animator.SetTrigger(_animIDRoll);
            canMove = false;
        }
    }

    private void DodgeRoll()
    {
        if (!_dodging) return;

        float elapsedTime = Time.time - _dodgeStartTime;
        if (elapsedTime > _dodgeDuration)
        {
            _dodging = false;
            canMove = true;
            return;
        }

        // Turn in the direction of the dodge
        float targetAngle = Mathf.Atan2(_dodgeDirection.x, _dodgeDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0.0f, targetAngle, 0.0f);

        // Update Blend Tree for animation
        if (_targeted)
        {
            _animator.SetFloat(_animIDDirectionX, _dodgeDirection.x);
            _animator.SetFloat(_animIDDirectionY, _dodgeDirection.z);
        }

        // Movement using Animation Curve
        float t = elapsedTime / _dodgeDuration;
        float dynamicSpeed = _dodgeSpeed * _dodgeSpeedCurve.Evaluate(t);
        Vector3 moveDirection = transform.forward;
        _controller.Move(moveDirection.normalized * (dynamicSpeed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    public void InitiateDash()
    {
        _animator.applyRootMotion = true;
        _animator.SetBool("Jump", false);
        _animator.SetBool("FreeFall", false);
        _dashing = true;
        _dashDirection = Vector3.forward;
        _dashStartTime = Time.time;
        canMove = false;
    }

    public void EndDash()
    {
        if (!_dodging)
        {
            _animator.applyRootMotion = false;
            canMove = true;
            _dashing = false;
        }
    }

    public void Dash(AnimationCurve dashCurve, float dashSpeed, float dashDuration)
    {
        if (_dashing)
        {
            float targetAngle = _mainCamera.transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0.0f, targetAngle, 0.0f);

            float t = (Time.time - _dashStartTime) / dashDuration;
            float dynamicSpeed = dashSpeed * dashCurve.Evaluate(t);

            Vector3 targetDirection = transform.forward;
            _controller.Move(targetDirection.normalized * (dynamicSpeed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
    }

    public void BlockRolling() => _breakRoll = false;
    public void AllowRolling() => _breakRoll = true;

    #endregion

    #region Animation Event Handlers

    public void StartAttack() => _melee.StartAttack();
    public void EndAttack() => _melee.EndAttack();

    public void IncrementAttackSeries()
    {
        if (_melee.attackSeries >= 2) _melee.attackSeries = 0;
        else _melee.attackSeries++;
        _melee.isAttacking = false;
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && !_dashing && !_dodging)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                float audioVolume = _sprintValue ? SprintStepAudioVolume : FootstepAudioVolume;
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), audioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), LandAudioVolume);
        }
    }

    private void OnSwing()
    {
        _melee.gameObject.GetComponents<AudioSource>()[0].Play();
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = Grounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }

    #endregion
}