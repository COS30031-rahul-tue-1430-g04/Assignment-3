using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings")]
	public float moveSpeed = 5f;
	public float turnSpeed = 100f;

	[Header("Camera Settings")]
	public bool thirdPersonView;
	public float firstPersonY = 1.5f;
	public float thirdPersonDistance = 3f;

	private Vector2 targetVelocity;

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
		moveAction.Enable();
		lookAction.Enable();
		jumpAction.Enable();
		sprintAction.Enable();
		interactAction.Enable();

		moveAction.performed += OnMove;
		moveAction.canceled += OnMove;
		lookAction.performed += OnLook;
		lookAction.canceled += OnLook;
		jumpAction.performed += OnJump;
		sprintAction.performed += OnSprint;
		interactAction.performed += OnInteract;
	}
	void OnDisable()
	{
		moveAction.Disable();
		lookAction.Disable();
		jumpAction.Disable();
		sprintAction.Disable();
		interactAction.Disable();
	}

	void OnValidate()
	{
		Camera.transform.localPosition = thirdPersonView ? Vector3.back * thirdPersonDistance : new Vector3(0, firstPersonY, 0);
	}

	private void FixedUpdate() =>
		Rb.linearVelocity = new(targetVelocity.x, Rb.linearVelocity.y, targetVelocity.y);

	void OnMove(CallbackContext context)
	{
		Vector2 input = context.ReadValue<Vector2>();
		Vector3 moveDirection = (transform.forward * input.y + transform.right * input.x);
		targetVelocity = new Vector2(moveDirection.x, moveDirection.z) * moveSpeed;
	}

	void OnLook(CallbackContext context)
	{
	}

	void OnJump(CallbackContext context)
	{
	}

	void OnSprint(CallbackContext context)
	{
	}

	void OnInteract(CallbackContext context)
	{
	}
}