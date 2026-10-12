using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
	public int Score { get; private set; }
	public int AssetsExplored { get; private set; }
	public int AssetsAnswered { get; private set; }

	public void RecordExplored()
	{
		AssetsExplored++;
		Debug.Log("Assets explored: " + AssetsExplored);
	}

	public void RecordAnswer(bool isCorrect, int points)
	{
		Score += points;

		// Count correct answers only.
		if (isCorrect)
			AssetsAnswered++;

		Debug.Log(
			"Correct: " + isCorrect +
			" | Score: " + Score +
			" | Correct answers: " + AssetsAnswered
		);
	}

	public void ResetProgress()
	{
		Score = 0;
		AssetsExplored = 0;
		AssetsAnswered = 0;
	}
}