using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings")]
	[Min(0f)] public float walkSpeed = 5f;
	[Min(0f)] public float sprintSpeed = 10f;
	[Min(0f)] public float jumpForce = 5f;
	[Min(0f)] public float gravity = 9.81f;

	[Header("Camera Settings")]
	[Min(0f)] public float cameraDistance = 3f;
	[Min(0f)] public float mouseSensitivity = 5f;
	[Range(-90f, 0f)] public float minVerticalLookAngle = -90f;
	[Range(0f, 90f)] public float maxVerticalLookAngle = 90f;

	private Vector2 moveInput;
	private bool isSprinting;
	private float verticalVelocity;
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
				Debug.LogError("Head (Camera parent) not found in PlayerController.");
			return _head;
		}
	}
	private CharacterController _characterController;
	private CharacterController CharacterControllerComponent
	{
		get
		{
			if (_characterController == null && !TryGetComponent<CharacterController>(out _characterController))
				Debug.LogError("CharacterController not found on PlayerController.");
			return _characterController;
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

		verticalVelocity = 0f;
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

	void FixedUpdate()
	{
		if (CharacterControllerComponent.isGrounded && verticalVelocity < 0f)
			verticalVelocity = -2f;
		else
			verticalVelocity -= gravity * Time.deltaTime;


		Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
		float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;

		Vector3 displacement = moveDirection * moveSpeed + Vector3.up * verticalVelocity;
		CharacterControllerComponent.Move(displacement * Time.deltaTime);
	}

	void OnGameStateChanged(GameManager.GameState newState)
	{
		if (newState == GameManager.GameState.Gameplay)
			EnablePlayerControl();
		else
			DisablePlayerControl();
	}

	void EnablePlayerControl()
	{
		moveAction.Enable();
		jumpAction.Enable();
		sprintAction.Enable();
		interactAction.Enable();
		lookAction.Enable();

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
		moveAction.Disable();
		jumpAction.Disable();
		sprintAction.Disable();
		interactAction.Disable();
		lookAction.Disable();

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

	void OnMoveInput(CallbackContext context) =>
		moveInput = context.ReadValue<Vector2>();

	void OnLookInput(CallbackContext context)
	{
		Vector2 input = context.ReadValue<Vector2>();

		transform.Rotate(Vector3.up, input.x * mouseSensitivity);

		currentVerticalLookAngle -= input.y * mouseSensitivity;
		currentVerticalLookAngle = Mathf.Clamp(currentVerticalLookAngle, minVerticalLookAngle, maxVerticalLookAngle);
		Head.localRotation = Quaternion.Euler(currentVerticalLookAngle, 0f, 0f);
	}

	void OnJumpInput(CallbackContext context)
	{
		if (!CharacterControllerComponent.isGrounded) return;
		verticalVelocity = jumpForce;
	}
	void OnSprintInput(CallbackContext context) =>
		isSprinting = context.ReadValueAsButton();
}