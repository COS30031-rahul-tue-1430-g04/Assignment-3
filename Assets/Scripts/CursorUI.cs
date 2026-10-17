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
	public InputActionReference defaultPoint;

	[Header("Cursor Settings")]
	public float sensitivity = 1.0f;

	[Header("References")]
	public Image cursorImage;

	private RectTransform rectTransform;
	private RectTransform cursorCanvasRect;

	private Mouse virtualMouse;
	private Vector2 virtualPos;

	private bool Active { get => cursorImage.enabled; set => cursorImage.enabled = value; }

	void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;

		rectTransform = GetComponent<RectTransform>();
		cursorCanvasRect = rectTransform.parent as RectTransform;

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
		InputState.Change(virtualMouse.position, virtualPos);

		GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
	}

	void OnDestroy() =>
		GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;

	void OnGameStateChanged(GameManager.GameState newState)
	{
		switch (newState)
		{
			case GameManager.GameState.Gameplay:
				Active = false;
				Cursor.lockState = CursorLockMode.Locked;
				break;
			case GameManager.GameState.Paused:
				Active = false;
				Cursor.lockState = CursorLockMode.None;
				break;
			case GameManager.GameState.AssetMenu:
			case GameManager.GameState.Menu:
				Active = true;
				Cursor.lockState = CursorLockMode.Locked;
				break;
		}
	}


	void OnEnable()
	{
		move.action.Enable();
		click.action.Enable();

		click.action.performed += OnClick;
		move.action.performed += OnMovePerformed;
		move.action.canceled += OnMovePerformed;

		WebGLRouter.Instance.OnPointerLock += OnPointerLock;
		WebGLRouter.Instance.OnPointerUnlock += OnPointerUnlock;
	}

	void OnDisable()
	{
		move.action.Disable();
		click.action.Disable();

		click.action.performed -= OnClick;
		move.action.performed -= OnMovePerformed;
		move.action.canceled -= OnMovePerformed;

		WebGLRouter.Instance.OnPointerLock -= OnPointerLock;
		WebGLRouter.Instance.OnPointerUnlock -= OnPointerUnlock;
	}

	void OnMovePerformed(InputAction.CallbackContext ctx)
	{
		if (!Active) return;
		Vector2 delta = ctx.ReadValue<Vector2>();
		virtualPos += delta * sensitivity;
		virtualPos.x = Mathf.Clamp(virtualPos.x, 0, Screen.width);
		virtualPos.y = Mathf.Clamp(virtualPos.y, 0, Screen.height);
		InputState.Change(virtualMouse.position, virtualPos);
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			cursorCanvasRect,
			virtualPos,
			null,
			out Vector2 localPoint))
		{
			rectTransform.anchoredPosition = localPoint;

			if (TryGetButtonAt(virtualPos, out Button button) && EventSystem.current != null)
				button.Select();
			else if (EventSystem.current != null)
				EventSystem.current.SetSelectedGameObject(null);
		}
	}

	private bool TryGetButtonAt(Vector2 screenPos, out Button button)
	{
		button = null;
		if (EventSystem.current == null)
			return false;

		IEnumerable<GraphicRaycaster> raycasters =
			from raycaster in FindObjectsByType<GraphicRaycaster>(
				FindObjectsInactive.Exclude,
				FindObjectsSortMode.None
			)
			let canvas = raycaster.GetComponent<Canvas>()
			where raycaster.isActiveAndEnabled
				&& canvas != null
				&& canvas.name != "CursorCanvas"
			orderby canvas.sortingOrder descending
			select raycaster;

		PointerEventData eventData = new(EventSystem.current) { position = screenPos };
		List<RaycastResult> results = new();

		foreach (GraphicRaycaster raycaster in raycasters)
		{
			results.Clear();
			raycaster.Raycast(eventData, results);
			button = results.Select(r => r.gameObject.GetComponentInParent<Button>())
				.FirstOrDefault(b => b != null && b.isActiveAndEnabled && b.interactable);
			if (button != null) return true;
		}

		return false;
	}

	private void OnClick(InputAction.CallbackContext _) => OnClick();
	private void OnClick()
	{
		if (!Active) return;
		if (TryGetButtonAt(virtualPos, out Button button))
			button.onClick.Invoke();
	}
	public void OnPointerLock()
	{
		OnClick();
		defaultPoint.action.performed -= OnPointerMove;
	}
	public void OnPointerUnlock()
	{
		defaultPoint.action.performed += OnPointerMove;
	}

	private void OnPointerMove(InputAction.CallbackContext ctx)
	{
		virtualPos = ctx.ReadValue<Vector2>();
		InputState.Change(virtualMouse.position, virtualPos);
	}
}
