using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(RectTransform))]
public class CursorUI : MonoBehaviour
{
	[Header("Input Actions")]
	public InputActionReference look;
	public InputActionReference click;

	[Header("Cursor Settings")]
	public float sensitivity = 1.0f;

	[Header("References")]
	public Canvas canvas;

	private RectTransform rectTransform;
	private RectTransform canvasRect;
	private Camera canvasCamera;

	private Mouse virtualMouse;
	private Vector2 virtualPos;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		canvasRect = canvas.GetComponent<RectTransform>();
		canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera
			? canvas.worldCamera
			: null;

		virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
		InputSystem.EnableDevice(virtualMouse);

		virtualPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
	}

	void OnEnable()
	{
		look.action.Enable();
		click.action.Enable();

		click.action.started += OnClickStarted;
		click.action.canceled += OnClickCanceled;
	}

	void OnDisable()
	{
		click.action.started -= OnClickStarted;
		click.action.canceled -= OnClickCanceled;
	}

	void Update()
	{
		Vector2 delta = look.action.ReadValue<Vector2>();
		virtualPos += delta * sensitivity;

		virtualPos.x = Mathf.Clamp(virtualPos.x, 0, Screen.width);
		virtualPos.y = Mathf.Clamp(virtualPos.y, 0, Screen.height);

		InputState.Change(virtualMouse.position, virtualPos);

		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			canvasRect,
			virtualPos,
			canvasCamera,
			out Vector2 localPoint))
		{
			rectTransform.anchoredPosition = localPoint;
		}
	}

	private void OnClickStarted(InputAction.CallbackContext ctx) =>
		InputSystem.QueueStateEvent(virtualMouse, new MouseState { buttons = 1 });
	private void OnClickCanceled(InputAction.CallbackContext ctx) =>
		InputSystem.QueueStateEvent(virtualMouse, new MouseState { buttons = 0 });
}
