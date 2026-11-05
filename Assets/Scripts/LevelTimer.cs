using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class LevelTimer : MonoBehaviour
{
	[Header("Level Settings")]
	[Min(1)]
	[SerializeField] private int timeLimitSeconds = 180;

	[Header("UI References")]
	[SerializeField] private TMP_Text timerText;
	[SerializeField] private GameObject gameOverPanel;

	private float timeRemaining;
	private bool shouldTick;

	private void Start()
	{
		timeRemaining = timeLimitSeconds;

		if (gameOverPanel != null)
			gameOverPanel.SetActive(false);

		UpdateTimerDisplay();
		shouldTick = true;

	}
	private void OnEnable() => Instance.OnGameStateChanged += OnGameStateChanged;
	private void OnDisable() => Instance.OnGameStateChanged -= OnGameStateChanged;
	private void OnGameStateChanged(GameState newState) => shouldTick = newState == GameState.Gameplay;

	private void Update()
	{
		if (!shouldTick) return;

		timeRemaining -= Time.deltaTime;
		timeRemaining = Mathf.Max(0f, timeRemaining);

		UpdateTimerDisplay();

		if (timeRemaining <= 0f)
			HandleTimeUp();
	}

	private void UpdateTimerDisplay()
	{
		if (timerText == null) return;

		int seconds = Mathf.CeilToInt(timeRemaining);
		int minutes = seconds / 60;
		seconds %= 60;

		timerText.text = $"{minutes:00}:{seconds:00}";
	}

	private void HandleTimeUp()
	{
		shouldTick = false;
		Instance.OnGameStateChanged -= OnGameStateChanged;

		timeRemaining = 0f;
		UpdateTimerDisplay();

		// Disable normal gameplay controls through the existing state.
		if (Instance != null)
			Instance.CurrentState = GameState.Menu;

		if (gameOverPanel != null)
			gameOverPanel.SetActive(true);
		else
			Debug.LogWarning("LevelTimer: Game Over Panel is not assigned.", this);
	}
	public void GoToMainMenu()
	{
		Time.timeScale = 1f;

		if (Instance != null)
		{
			Instance.CurrentState = GameState.Menu;
		}

		SceneManager.LoadScene(1);
	}

	public void RetryLevel()
	{
		// Restore normal time and state before reloading the scene.
		Time.timeScale = 1f;

		if (Instance != null)
			Instance.CurrentState = GameState.Gameplay;

		SceneManager.LoadScene(
			SceneManager.GetActiveScene().buildIndex);
	}
}