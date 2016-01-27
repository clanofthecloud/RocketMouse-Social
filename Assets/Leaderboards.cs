using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CotcSdk;
using System;
using System.Text;

public class Leaderboards : MonoBehaviour {

	public Text RankNameText, RankScoreText, YourScoreText;
	public Button ShareButton;
	// Used to perform social functions
	public Social Social;
	// Needs update if the player logs in/out
	private Gamer LastGamer;

	// Use this for initialization
	void Start () {
		RankNameText.text = "";
		RankScoreText.text = "";
		YourScoreText.text = "";
		Social.GamerChanged += Social_GamerChanged;
	}
	
	// Update is called once per frame
	void Update () {}

	void Destroy() {
		Social.GamerChanged -= Social_GamerChanged;
	}

	void Social_GamerChanged(object sender, EventArgs e) {
		// Update the UI whenever the logged in gamer changes
		UpdateLeaderboards();
	}

	public void PostScoreAndUpdateLeaderboards(uint coins, float runtime) {
		Social.PostScore(coins, runtime).Done (dummy => {
			YourScoreText.text = "Your score: " + coins + " (" + Various.FormatRuntime((int) (runtime * 100)) + ")";
			UpdateLeaderboards();
		});
	}

	public void UpdateLeaderboards() {
		Action<Exception> onError = ex => {
			Debug.LogError("Failed to fetch score list: " + ex.ToString());
			RankNameText.text = "Network error\nPlease check your connectivity.";
		};

		// Enable the sharing button only if we're logged in through facebook
		ShareButton.gameObject.SetActive(Social.CurrentGamer.Network == LoginNetwork.Facebook);

		// Fetch top 5
		Social.FetchScores (centerAroundPlayer: false)
		.Catch (onError)
		.Done ((PagedList<Score> topScores) => {
			// Generate a list of names & ranks + another for scores
			StringBuilder rankList = new StringBuilder();
			StringBuilder scoresList = new StringBuilder();
			bool ourScoreHasBeenIncluded = false;
			int rank = 1;
			foreach (Score s in topScores) {
				string name = s.GamerInfo["profile"]["displayName"] + " (#" + (rank++) + ")";
				Bundle info = Bundle.FromJson(s.Info);
				string scoreText = s.Value + " (" + Various.FormatRuntime(info["runtime"]) + ")";
				// If it's us, use a highlight color
				if (s.GamerInfo.GamerId == Social.CurrentGamer.GamerId) {
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
				.Catch (ex => { /* We have never scored, that's ok */ })
				.Done (playerScore => {
					// Player has never scored here
					if (playerScore.Count == 0) return;

					string name = playerScore[0].GamerInfo["profile"]["displayName"] + " (#" + playerScore[0].Rank + ")";
					Bundle info = Bundle.FromJson(playerScore[0].Info);
					string scoreText = playerScore[0].Value + " (" + Various.FormatRuntime(info["runtime"]) + ")";
					HighlightTextForUs(name, rankList);
					HighlightTextForUs(scoreText, scoresList);
					RankNameText.text = rankList.ToString();
					RankScoreText.text = scoresList.ToString();
				});
			}
		});
	}

	StringBuilder HighlightTextForUs(string text, StringBuilder appendTo) {
		return appendTo.Append("<color=#4adeff>").Append(text).Append("</color>");
	}
}
