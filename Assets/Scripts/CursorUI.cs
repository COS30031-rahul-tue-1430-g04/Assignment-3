using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

[RequireComponent(typeof(RectTransform))]
public class CursorUI : MonoBehaviour
{
	[Header("Input Actions")]
	public InputActionReference move;
	public InputActionReference click;

	[Header("Cursor Settings")]
	public float sensitivity = 1.0f;

	private Canvas canvas;

	private RectTransform rectTransform;
	private RectTransform canvasRect;
	private Camera canvasCamera;

	private Mouse virtualMouse;
	private Vector2 virtualPos;

	void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		Cursor.lockState = CursorLockMode.Locked;

		rectTransform = GetComponent<RectTransform>();

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
	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		canvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None)
			.FirstOrDefault(c => c.name != "CursorCanvas");
		if (canvas == null) return;
		
		canvasRect = canvas.GetComponent<RectTransform>();
		canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera
			? canvas.worldCamera
			: null;
	}

	void OnDestroy()
	{
		GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnGameStateChanged(GameManager.GameState newState)
	{
		switch (newState)
		{
			case GameManager.GameState.Gameplay:
				gameObject.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				break;
			case GameManager.GameState.Paused:
				gameObject.SetActive(false);
				Cursor.lockState = CursorLockMode.None;
				break;
			case GameManager.GameState.AssetMenu:
			case GameManager.GameState.Menu:
				gameObject.SetActive(true);
				Cursor.lockState = CursorLockMode.Locked;
				break;
		}
	}


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
		move.action.Disable();
		click.action.Disable();

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

			if (TryGetButtonAt(virtualPos, out Button button))
				button.Select();
			else
				EventSystem.current.SetSelectedGameObject(null);
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
