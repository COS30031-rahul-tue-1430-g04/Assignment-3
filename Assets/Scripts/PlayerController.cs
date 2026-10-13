using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings")]
	[Min(0f)] public float walkSpeed = 5f;
	[Min(0f)] public float sprintSpeed = 10f;
	[Min(0f)] public float jumpForce = 5f;
	[Min(0f)] public float groundedDistance = 0.1f;

	[Header("Camera Settings")]
	[Min(0f)] public float cameraDistance = 3f;
	[Min(0f)] public float mouseSensitivity = 5f;
	[Range(-90f, 0f)] public float minVerticalLookAngle = -90f;
	[Range(0f, 90f)] public float maxVerticalLookAngle = 90f;

	private Vector2 moveInput;
	private bool isSprinting;
	private Vector2 targetVelocity;
	private float currentVerticalLookAngle;

	private Camera _camera;
	private Camera Camera
	{
		get
		{
			if (_camera == null)
				_camera = GetComponentInChildren<Camera>(includeInactive: false);
			if (_camera == null)
				Debug.LogError("Camera not found in children of PlayerController.");
			return _camera;
		}
	}
	private Transform _head;
	private Transform Head
	{
		get
		{
			if (_head == null)
				_head = Camera.transform.parent;
			if (_head == null || _head == transform)
				Debug.LogError("Head (Camera parrent) not found in PlayerController.");
			return _head;
		}
	}
	private Transform _feet;
	private Transform Feet
	{
		get
		{
			if (_feet == null)
				_feet = transform.Find("Feet");
			if (_feet == null)
				Debug.LogError("Feet not found.");
			return _feet;
		}
	}
	private Rigidbody _rb;
	private Rigidbody Rb
	{
		get
		{
			if (_rb == null && !TryGetComponent<Rigidbody>(out _rb))
				Debug.LogError("Rigidbody not found on PlayerController.");
			return _rb;
		}
	}
	private Interactor _interactor;
	private Interactor Interactor
	{
		get
		{
			if (_interactor == null && !Head.TryGetComponent(out _interactor))
				Debug.LogError("Interactor not found on PlayerController.");
			return _interactor;
		}
	}

	private InputAction moveAction;
	private InputAction lookAction;
	private InputAction jumpAction;
	private InputAction sprintAction;
	private InputAction interactAction;


	void Awake()
    {
		moveAction = InputSystem.actions.FindAction("Move");
		lookAction = InputSystem.actions.FindAction("Look");
		jumpAction = InputSystem.actions.FindAction("Jump");
		sprintAction = InputSystem.actions.FindAction("Sprint");
		interactAction = InputSystem.actions.FindAction("Interact");
	}

	void OnEnable()
	{
		GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
		OnGameStateChanged(GameManager.Instance.CurrentState);
	}
	void OnDisable()
	{
		GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
		DisablePlayerControl();
	}

	void OnValidate()
	{
		Camera.transform.localPosition = Vector3.back * cameraDistance;
	}

	private void FixedUpdate() =>
		Rb.linearVelocity = new(targetVelocity.x, Rb.linearVelocity.y, targetVelocity.y);

	void OnGameStateChanged(GameManager.GameState newState)
	{
		if (newState == GameManager.GameState.Gameplay)
			EnablePlayerControl();
		else
			DisablePlayerControl();
	}

	void EnablePlayerControl()
	{
		Cursor.lockState = CursorLockMode.Locked;

		moveAction.Enable();
		lookAction.Enable();
		jumpAction.Enable();
		sprintAction.Enable();
		interactAction.Enable();

		moveAction.performed += OnMoveInput;
		moveAction.canceled += OnMoveInput;
		lookAction.performed += OnLookInput;
		lookAction.canceled += OnLookInput;
		jumpAction.performed += OnJumpInput;
		sprintAction.performed += OnSprintInput;
		sprintAction.canceled += OnSprintInput;
		interactAction.performed += OnInteractInput;
	}
	void DisablePlayerControl()
	{
		Cursor.lockState = CursorLockMode.None;

		moveAction.Disable();
		lookAction.Disable();
		jumpAction.Disable();
		sprintAction.Disable();
		interactAction.Disable();

		moveAction.performed -= OnMoveInput;
		moveAction.canceled -= OnMoveInput;
		lookAction.performed -= OnLookInput;
		lookAction.canceled -= OnLookInput;
		jumpAction.performed -= OnJumpInput;
		sprintAction.performed -= OnSprintInput;
		sprintAction.canceled -= OnSprintInput;
		interactAction.performed -= OnInteractInput;
	}

	void OnInteractInput(CallbackContext _) => Interactor.Interact();

	void OnMoveInput(CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
		UpdateVelocity();
	}

	void OnLookInput(CallbackContext context)
	{
		Vector2 input = context.ReadValue<Vector2>();

		transform.Rotate(Vector3.up, input.x * mouseSensitivity * Time.fixedDeltaTime);

		currentVerticalLookAngle -= input.y * mouseSensitivity * Time.fixedDeltaTime;
		currentVerticalLookAngle = Mathf.Clamp(currentVerticalLookAngle, minVerticalLookAngle, maxVerticalLookAngle);
		Head.localRotation = Quaternion.Euler(currentVerticalLookAngle, 0f, 0f);

		UpdateVelocity();
	}

	void UpdateVelocity()
	{
		Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x);
		float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;
		targetVelocity = new Vector2(moveDirection.x, moveDirection.z) * moveSpeed;
	}

	void OnJumpInput(CallbackContext context)
	{
		if (!Physics.Raycast(Feet.position, Vector3.down, groundedDistance))
			return;
		Rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
	}

	void OnSprintInput(CallbackContext context)
	{
		isSprinting = context.ReadValueAsButton();
		UpdateVelocity();
	}
}