using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

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
		Active = false;

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

		Debug.Log(Mouse.current);
		StartCoroutine(CheckMouseCurrent());

		static IEnumerator CheckMouseCurrent()
		{
			yield return null;
			Debug.Log($"Next frame Mouse.current = {Mouse.current?.name}");
		}


		Debug.Log("=== MOUSE DEVICES ===");
		foreach (Mouse mouse in InputSystem.devices.Where(d => d is Mouse).Cast<Mouse>())
		{
			Debug.Log($"Mouse: {mouse.name}, Display Name: {mouse.displayName}, ID: {mouse.deviceId}, Layout: {mouse.layout}");
		}
		Debug.Log($"Mouse.current: {Mouse.current?.name}");
		Debug.Log($"Virtual mouse ID: {virtualMouse?.deviceId}");

		Debug.Log("=== Bindings ===");
		foreach (var binding in defaultPoint.action.bindings)
		{
			Debug.Log($"defaultPoint Binding: {binding.path}, Action: {binding.action}, Groups: {binding.groups}");
		}
		Debug.Log(defaultPoint.action.controls.Count);
		foreach (var control in defaultPoint.action.controls)
		{
			Debug.Log($"defaultPoint Control: {control.path}, Device: {control.device.name}, Layout: {control.device.layout}");
		}
	}

	void OnDestroy() =>
		GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;

	void OnGameStateChanged(GameManager.GameState newState)
	{
		bool inGameplay = newState == GameManager.GameState.Gameplay;
		if (inGameplay)
		{
			Active = false;
			return;
		}
		bool paused = newState == GameManager.GameState.Paused;
		if (!paused)
		{
			Active = true;
			return;
		}

		bool inWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
		bool pointerLocked = WebGLRouter.Instance.IsPointerLocked;

		Active = !inWebGL || pointerLocked;
	}


	void OnEnable()
	{
		move.action.Enable();
		click.action.Enable();

		click.action.performed += OnClick;
		move.action.performed += OnMovePerformed;
		move.action.canceled += OnMovePerformed;
		defaultPoint.action.performed += OnPointerMove;

		WebGLRouter.Instance.OnPointerLock += OnWebGlPointerLock;
	}

	void OnDisable()
	{
		move.action.Disable();
		click.action.Disable();

		click.action.performed -= OnClick;
		move.action.performed -= OnMovePerformed;
		move.action.canceled -= OnMovePerformed;
		defaultPoint.action.performed -= OnPointerMove;

		WebGLRouter.Instance.OnPointerLock -= OnWebGlPointerLock;
	}

	void OnWebGlPointerLock()
	{
		OnGameStateChanged(GameManager.Instance.CurrentState);
		OnClick();
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
		Debug.Log("OnClick called");
		if (!Active) return;
		if (TryGetButtonAt(virtualPos, out Button button))
			button.onClick.Invoke();
	}

	private void OnPointerMove(InputAction.CallbackContext ctx)
	{
		Debug.Log("=== POINTER MOVE ===");
		Debug.Log($"Device: {ctx.control.device.name}");
		Debug.Log($"Device ID: {ctx.control.device.deviceId}");
		Debug.Log($"Layout: {ctx.control.device.layout}");
		Debug.Log($"Control Path: {ctx.control.path}");
		Debug.Log($"Action Control: {ctx.action.activeControl?.path}");
		Debug.Log($"Value: {ctx.ReadValue<Vector2>()}");

		Debug.Log(InputControlPath.ToHumanReadableString(ctx.control.path));

		if (ctx.control.device == virtualMouse) return;
		Debug.Log("OnPointerMove called");
		Debug.Log("The pointer currently has the lockstate: " + WebGLRouter.Instance.IsPointerLocked);
		Debug.Log("The current platform is: " + Application.platform);
		Debug.Log("The current virtual mouse position is: " + virtualPos);
		Debug.Log("The current pointer value is: " + ctx.ReadValue<Vector2>());
		if (WebGLRouter.Instance.IsPointerLocked || Application.platform != RuntimePlatform.WebGLPlayer) return;
		virtualPos = ctx.ReadValue<Vector2>();
		Debug.Log("Therefore, we are updating the virtual mouse position to: " + virtualPos);
		InputState.Change(virtualMouse.position, virtualPos);
	}
}
