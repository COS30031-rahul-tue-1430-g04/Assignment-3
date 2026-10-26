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
	private bool hasEnded;

	private void Start()
	{
		timeRemaining = timeLimitSeconds;
		hasEnded = false;

		if (gameOverPanel != null)
			gameOverPanel.SetActive(false);

		UpdateTimerDisplay();
	}

	private void Update()
	{
		if (hasEnded)
			return;

		// Count down only during normal gameplay.
		if (GameManager.Instance == null ||
			GameManager.Instance.CurrentState != GameState.Gameplay)
			return;

		timeRemaining -= Time.deltaTime;
		timeRemaining = Mathf.Max(0f, timeRemaining);

		UpdateTimerDisplay();

		if (timeRemaining <= 0f)
			HandleTimeUp();
	}

	private void UpdateTimerDisplay()
	{
		if (timerText == null)
			return;

		int seconds = Mathf.CeilToInt(timeRemaining);
		int minutes = seconds / 60;
		seconds %= 60;

		timerText.text = $"{minutes:00}:{seconds:00}";
	}

	private void HandleTimeUp()
	{
		if (hasEnded)
			return;

		hasEnded = true;
		timeRemaining = 0f;
		UpdateTimerDisplay();

		// Disable normal gameplay controls through the existing state.
		if (GameManager.Instance != null)
			GameManager.Instance.CurrentState = GameState.Paused;

		if (gameOverPanel != null)
			gameOverPanel.SetActive(true);
		else
			Debug.LogWarning(
				"LevelTimer: Game Over Panel is not assigned.", this);
	}
	public void GoToMainMenu()
	{
		Time.timeScale = 1f;

		if (GameManager.Instance != null)
		{
			GameManager.Instance.CurrentState = GameState.Menu;
		}

		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}

	public void RetryLevel()
	{
		// Restore normal time and state before reloading the scene.
		Time.timeScale = 1f;

		if (GameManager.Instance != null)
			GameManager.Instance.CurrentState = GameState.Gameplay;

		SceneManager.LoadScene(
			SceneManager.GetActiveScene().buildIndex);
	}
}