using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CotcSdk;
using System;
using System.Text;

public class Leaderboards : MonoBehaviour {

	// Set this to true in the inspector in order to fetch the leaderboards automatically upon instantiation.
	// Unless this is set to true, you need to call UpdateLeaderboards at least once
	public bool AutoFetch;
	public Text RankNameText, RankScoreText, YourScoreText;
	// Used to perform social functions
	public Social Social;

	// Use this for initialization
	void Start () {
		RankNameText.text = "";
		RankScoreText.text = "";
		YourScoreText.text = "";
		if (AutoFetch) {
			UpdateLeaderboards();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void PostScoreAndUpdateLeaderboards(uint coins, float runtime) {
		Social.PostScore(coins, runtime).Then(dummy => {
			YourScoreText.text = "Your score: " + coins + " (" + FormatRuntime((int) (runtime * 100)) + ")";
			UpdateLeaderboards();
		});
	}

	public void UpdateLeaderboards() {
		Action<Exception> onError = ex => {
			Debug.LogError("Failed to fetch score list: " + ex.ToString());
			RankNameText.text = "Network error\nPlease check your connectivity.";
		};

		// Fetch top 5
		Social.FetchScores (centerAroundPlayer: false)
		.Then ((PagedList<Score> topScores) => {
			// Generate a list of names & ranks + another for scores
			StringBuilder rankList = new StringBuilder();
			StringBuilder scoresList = new StringBuilder();
			bool ourScoreHasBeenIncluded = false;
			int rank = 1;
			foreach (Score s in topScores) {
				string name = s.GamerInfo["profile"]["displayName"] + " (#" + (rank++) + ")";
				Bundle info = Bundle.FromJson(s.Info);
				string scoreText = s.Value + " (" + FormatRuntime(info["runtime"]) + ")";
				// If it's us, use a highlight color
				if (s.GamerInfo.GamerId == Social.CurrentGamerId()) {
					HighlightTextForUs(name, rankList).AppendLine();
					HighlightTextForUs(scoreText, scoresList).AppendLine();
					ourScoreHasBeenIncluded = true;
				}
				else {
					rankList.AppendLine(name);
					scoresList.AppendLine(scoreText);
				}
			}
			RankNameText.text = rankList.ToString();
			RankScoreText.text = scoresList.ToString();

			// And append player's score at the bottom
			if (!ourScoreHasBeenIncluded) {
				Social.FetchScores (centerAroundPlayer: true)
				.Then (playerScore => {
					// Player has never scored here
					if (playerScore.Count == 0) return;

					string name = playerScore[0].GamerInfo["profile"]["displayName"] + " (#" + playerScore[0].Rank + ")";
					Bundle info = Bundle.FromJson(playerScore[0].Info);
					string scoreText = playerScore[0].Value + " (" + FormatRuntime(info["runtime"]) + ")";
					HighlightTextForUs(name, rankList);
					HighlightTextForUs(scoreText, scoresList);
					RankNameText.text = rankList.ToString();
					RankScoreText.text = scoresList.ToString();
				})
				.Catch (ex => { /* We have never scored, that's ok */ });
			}
		})
		.Catch (onError);
	}

	string FormatRuntime(int runtime100thSec) {
		return string.Format("{0}:{1:00}", runtime100thSec / 60 / 100, (runtime100thSec / 100) % 60);
	}

	StringBuilder HighlightTextForUs(string text, StringBuilder appendTo) {
		return appendTo.Append("<color=#a52a2a>").Append(text).Append("</color>");
	}
}
