using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;
public class PauseManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject pausePanel;

	private bool isPaused = false;
	private InputAction pauseAction;
	private GameManager.GameState previousState = GameManager.GameState.Gameplay;

	bool CheckSerializedFields()
	{
		if (pausePanel == null)
		{
			Debug.LogError("PauseManager: One or more serialized fields are not assigned.");
			return false;
		}
		return true;
	}

	void Awake()
	{
		if (!CheckSerializedFields()) return;

		pausePanel.SetActive(false);
		pauseAction = InputSystem.actions.FindAction("Pause");
		Time.timeScale = 1f;
	}

	void OnEnable()
	{
		pauseAction.Enable();
		pauseAction.performed += TogglePause;
		WebGLRouter.Instance.OnPointerUnlock += Pause;
	}
	void OnDisable()
	{
		pauseAction.performed -= TogglePause;
		pauseAction.Disable();
		WebGLRouter.Instance.OnPointerUnlock -= Pause;
	}
	void TogglePause(CallbackContext _) => TogglePause();

    public void TogglePause()
    {
        if (!CheckSerializedFields()) return;
		if (isPaused) Resume(); else Pause();
    }

	private void Pause()
	{
		if (!CheckSerializedFields()) return;
		isPaused = true;
		pausePanel.SetActive(true);
		previousState = GameManager.Instance.CurrentState;
		GameManager.Instance.CurrentState = GameManager.GameState.Paused;
		AudioManager.Instance.PlayPause();
		AudioManager.Instance.SetMusicPaused(true);
	}
	private void Resume()
	{
		if (!CheckSerializedFields()) return;
		isPaused = false;
		pausePanel.SetActive(false);
		GameManager.Instance.CurrentState = previousState;
		AudioManager.Instance.PlayPause();
		AudioManager.Instance.SetMusicPaused(false);
	}

    public void RestartLevel()
	{
		GameManager.Instance.CurrentState = GameManager.GameState.Gameplay;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	public void MainMenu()
	{
		GameManager.Instance.CurrentState = GameManager.GameState.Menu;
		SceneManager.LoadScene(1);
	}
}