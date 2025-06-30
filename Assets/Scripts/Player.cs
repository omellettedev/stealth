using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;


// ----- BASED ON STARTER ASSET ThirdPersonController.cs -----
/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Player")]
    [Space(10)]
    [Tooltip("The default relative position of the center of the player hitbox capsule, relative to feet")]
    [SerializeField] private Vector3 defaultCenter;

    [Tooltip("The default height of the player hitbox capsule")]
    [SerializeField] private float defaultHeight;

    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Crouch speed of the character in m/s")]
    [SerializeField] private float crouchSpeed = 1.0f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;

    private bool canDash = true;
    private bool isDashing = false;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    [Space(10)]
    [Header("Stats")]
    [Tooltip("The player's max inventory size")]
    [SerializeField] private int maxInventorySize = 8;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private PlayerInput _playerInput;
    private Animator _animator;
    private CharacterController _controller;
    private PlayerInputHandler _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    [Header("Crouch")]
    [Tooltip("Solid object layers (for uncrouch detection)")]
    [SerializeField] private LayerMask solidLayers;

    // the method will calculate the crouch center and height based on this metric and the base center and height
    [Tooltip("The crouching height proportion (crouch height / normal height)")]
    [SerializeField] private float crouchProportion = 0.7f;

    [Tooltip("Player Render GameObject")]
    [SerializeField] private GameObject playerRender;

    // trying to uncrouch
    private bool tryingToUncrouch = false;

    // the solid object the player is looking at
    private GameObject objectLookingAt;

    [Header("Interaction")]
    [Tooltip("Interaction range")]
    [SerializeField] private float interactionRange = 5;

    [Tooltip("Solid + interactable object layers (for interaction)")]
    [SerializeField] private LayerMask solidAndInteractableLayers;

    [Tooltip("Player chest object")]
    [SerializeField] private GameObject playerChest;

    // player inventory
    private List<Item> inventory = new List<Item>();
    private int currentItemIndex = -1; // -1 means no item selected
    public Item HeldItem => (currentItemIndex >= 0 && currentItemIndex < inventory.Count) ? inventory[currentItemIndex] : null;

    public event Action<Sprite> PickedUpItemEvent;
    public event Action<int> LostItemEvent;
    public event Action<int, int> SelectedItemEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputHandler>();
        _playerInput = GetComponent<PlayerInput>();

        _input.CrouchEvent += Crouch;
        _input.UncrouchEvent += Uncrouch;
        _input.InteractEvent += Interact;
        _input.UninteractEvent += Uninteract;
        _input.ItemEvent += (i) => SelectItem(i - 1);
        _input.DropItemEvent += DropItem;

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        JumpAndGravity();
        GroundedCheck();
        CheckUncrouch();
        Move();
        if (_input.Dash && canDash && !isDashing && Grounded)
        {
            StartCoroutine(Dash());
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.Look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            deltaTimeMultiplier *= 3;
            _cinemachineTargetYaw += _input.Look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.Look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);

        CheckLook();
    }

    private void Move()
    {
        // set target speed
        float targetSpeed = (_input.Crouch || tryingToUncrouch) ? crouchSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.Movement == Vector2.zero) targetSpeed = 0.0f;

        if (isDashing) return;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.Movement.magnitude;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.Movement.x, 0.0f, _input.Movement.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.Movement != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.Jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            _input.Jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void Crouch()
    {
        tryingToUncrouch = false;
        // keeps the base the same
        _controller.center = new Vector3(defaultCenter.x, defaultCenter.y - (0.5f * (1 - crouchProportion) * defaultHeight), defaultCenter.z);
        _controller.height = crouchProportion * defaultHeight;
        playerRender.transform.localScale = new Vector3(1, crouchProportion, 1);
    }

    private void Uncrouch()
    {
        tryingToUncrouch = true;
    }

    private void CheckUncrouch()
    {
        if (!tryingToUncrouch) return;
        Vector3 currentTop = transform.position + _controller.center + Vector3.up * (_controller.height / 2);
        Vector3 newTop = transform.position + defaultCenter + Vector3.up * (defaultHeight / 2);
        Vector3 radiusOffset = Vector3.up * _controller.radius;
        if (!Physics.CheckCapsule(currentTop - radiusOffset, newTop - radiusOffset, _controller.radius, solidLayers))
        {
            tryingToUncrouch = false;
            _controller.center = defaultCenter;
            _controller.height = defaultHeight;
            playerRender.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float startTime = Time.time;
        Vector3 dashDirection = transform.forward;

        while (Time.time < startTime + dashDuration)
        {
            _controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }

    private void CheckLook()
    {
        Vector3 playerChestPosition = playerChest.transform.position;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, solidAndInteractableLayers) // get what a raycast from the camera hits
            && (Vector3.Distance(playerChestPosition, hit.point) < interactionRange) // check if the hit is within interaction range
            && (Vector3.Dot(transform.forward, hit.point - playerChestPosition) > 0)) // check if the hit is in front of the player
        {
            bool somethingInBetween = Physics.Linecast(playerChestPosition, hit.point, out RaycastHit rangeHit, solidAndInteractableLayers); // check if there is something in between the player and the hit point
            if (!somethingInBetween || rangeHit.collider == hit.collider) // if nothing in between or the object in between is the target
            {
                GameObject target = hit.collider.gameObject;
                if (objectLookingAt != target)
                {
                    if (objectLookingAt != null) { LookAwayFromObject(); }
                    objectLookingAt = target;
                    if (target.TryGetComponent(out IInteractable interactable))
                    {
                        interactable.OnLookAt();
                    }
                }
            }
            else if (objectLookingAt != null) {
                LookAwayFromObject();
            }
        }
        else if (objectLookingAt != null) { LookAwayFromObject(); }
    }

    private void LookAwayFromObject()
    {
        if (objectLookingAt.TryGetComponent(out IInteractable interactable))
        {
            interactable.OnLookAway();
        }
        objectLookingAt = null;
    }

    void Interact()
    {
        if (objectLookingAt != null && objectLookingAt.TryGetComponent(out IInteractable interactable))
        {
            interactable.OnInteract();
        }
    }

    void Uninteract()
    {
        if (objectLookingAt != null && objectLookingAt.TryGetComponent(out IInteractable interactable))
        {
            interactable.OnUninteract();
        }
    }

    void SelectItem(int index)
    {
        int previousItemIndex = currentItemIndex; // store previous item index for event
        if (index < -1 || index >= inventory.Count)
        {
            if (currentItemIndex != -1)
            {
                Uninteract(); // stop interacting with the current object if any
            }
            currentItemIndex = -1; // invalid index, deselect item
            SelectedItemEvent?.Invoke(previousItemIndex, currentItemIndex);
            return;
        }
        if (currentItemIndex == index) return; // already selected
        currentItemIndex = index;
        SelectedItemEvent?.Invoke(previousItemIndex, currentItemIndex);
        Uninteract(); // stop interacting with the current object if any
    }

    void DropItem()
    {
        if (currentItemIndex == -1) return; // no item selected
        inventory.RemoveAt(currentItemIndex);
        LostItemEvent?.Invoke(currentItemIndex);
        currentItemIndex = -1;
    }

    public void TryPickupItem(Item item)
    {
        if (inventory.Count >= maxInventorySize) return; // inventory is full
        inventory.Add(item);
        PickedUpItemEvent?.Invoke(item.ItemIcon);
    }
}