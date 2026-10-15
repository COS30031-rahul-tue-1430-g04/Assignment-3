using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(RectTransform))]
public class CursorUI : MonoBehaviour
{
	[Header("Input Actions")]
	public InputActionReference move;
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
		Cursor.lockState = CursorLockMode.Locked;

		rectTransform = GetComponent<RectTransform>();
		canvasRect = canvas.GetComponent<RectTransform>();
		canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera
			? canvas.worldCamera
			: null;

		foreach (InputDevice device in InputSystem.devices)
		{
			if (device.name == "VirtualMouse")
			{
				virtualMouse = (Mouse)device;
				break;
			}
		}
		virtualMouse ??= (Mouse)InputSystem.AddDevice("VirtualMouse");
		InputSystem.EnableDevice(virtualMouse);

		virtualPos = new Vector2(Screen.width / 2f, Screen.height / 2f);

		GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
	}
	void OnDestroy() =>
		GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;

	void OnGameStateChanged(GameManager.GameState newState) =>
		gameObject.SetActive(newState != GameManager.GameState.Gameplay);


	void OnEnable()
	{
		move.action.Enable();
		click.action.Enable();

		click.action.canceled += OnClick;
		move.action.performed += OnMovePerformed;
		move.action.canceled += OnMovePerformed;
	}

	void OnDisable()
	{
		click.action.canceled -= OnClick;
		move.action.performed -= OnMovePerformed;
		move.action.canceled -= OnMovePerformed;
	}

	void OnMovePerformed(InputAction.CallbackContext ctx)
	{
		Vector2 delta = ctx.ReadValue<Vector2>();
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

	private bool TryGetButtonAt(Vector2 screenPos, out Button button)
	{
		GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
		PointerEventData eventData = new(EventSystem.current)
		{ position = screenPos };

		List<RaycastResult> results = new();
		raycaster.Raycast(eventData, results);
		foreach (RaycastResult result in results)
			if (result.gameObject.TryGetComponent(out button))
				return true;

		button = null;
		return false;
	}

	private void OnClick(InputAction.CallbackContext ctx)
	{
		if (TryGetButtonAt(virtualPos, out Button button))
			button.onClick.Invoke();
	}
}
