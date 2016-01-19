using UnityEngine;
using System;
using System.Collections;
using CotcSdk;

public class Social : MonoBehaviour {

	// The cloud allows to make generic operations (non user related)
	private Cloud Cloud;
	// The gamer is the base to perform most operations. A gamer object is obtained after successfully signing in.
	private Gamer Gamer;
	// When a gamer is logged in, the loop is launched for domain private. Only one is run at once.
	private DomainEventLoop Loop;
	// Leaderboards for score (max. number of collected coins) are stored under this name on the server.
	private static string ScoreBoardName = "scores";

	// Use this for initialization
	void Start () {
		var cotc = FindObjectOfType<CotcGameObject> ();
		if (cotc == null) {
			throw new SystemException ("No Clan of the Cloud SDK object found on the scene");
		}

		// Log unhandled exceptions (.Done block without .Catch -- not called if there is any .Then)
		Promise.UnhandledException += (object sender, ExceptionEventArgs e) => {
			Debug.LogError("Unhandled exception: " + e.Exception.ToString());
		};
//		Promise.Debug_OutputAllExceptions = true;

		// Initiate getting the main Cloud object
		cotc.GetCloud().Done(cloud => {
			Cloud = cloud;
			// Retry failed HTTP requests once after 1 sec, then 5, finally abort.
			Cloud.HttpRequestFailedHandler = (HttpRequestFailedEventArgs e) => {
				int count = (int?) e.UserData ?? 0;
				if (count == 0)      e.RetryIn(1000);
				else if (count == 1) e.RetryIn(5000);
				else                 e.Abort();
				e.UserData = ++count;
			};
			Debug.Log("Setup done");
//			PostSampleScoresForTesting();
			InitCloud();
		});
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void InitCloud() {
		AutoRelogin ();
	}

	private void Loop_ReceivedEvent(DomainEventLoop sender, EventLoopArgs e) {
		Debug.Log("Received event of type " + e.Message.Type + ": " + e.Message.ToJson());
	}

	#region Login stuff
	private void AutoRelogin() {
		// On the first time, we need to log in anonymously, else log back with the stored credentials
		if (!PlayerPrefs.HasKey ("GamerInfo")) {
			MakeAnonymousAccount().Done();
		}
		else {
			// Log back in
			GamerInfo info = new GamerInfo(PlayerPrefs.GetString("GamerInfo"));
			Debug.Log ("Attempting to log back with existing credentials");
			Cloud.Login (
				network: LoginNetwork.Anonymous,
				networkId: info.GamerId,
				networkSecret: info.GamerSecret)
			.Catch (ex => {
				Debug.LogError("Failed to log back in with stored credentials. Restarting process.");
				MakeAnonymousAccount().Done();
			})
			.Done(gamer => DidLogin(gamer));
		}
	}

	// Invoked when any sign in operation has completed
	private void DidLogin(Gamer newGamer) {
		if (Gamer != null) {
			Debug.LogWarning("Current gamer " + Gamer.GamerId + " has been dismissed");
			Loop.Stop();
		}
		Gamer = newGamer;
		Loop = Gamer.StartEventLoop();
		Loop.ReceivedEvent += Loop_ReceivedEvent;
		Debug.Log("Signed in successfully (ID = " + Gamer.GamerId + ")");
		// Keep login in persistent memory to restore next time
		PlayerPrefs.SetString("GamerInfo", new GamerInfo(Gamer).ToJson());
	}

	private Promise<Gamer> MakeAnonymousAccount() {
		return Cloud.LoginAnonymously ()
			.Then (gamer => DidLogin (gamer))
			.Catch (ex => {
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError ("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
	}
	#endregion

	#region Public methods for access by other components
	public string CurrentGamerId() {
		return Gamer.GamerId;
	}

	public Promise<PagedList<Score>> FetchScores(bool centerAroundPlayer) {
		// Offset -1 allows to center scores around player
		// As we display only top 5, five scores at once are enough
		return Gamer.Scores.List (
			board: ScoreBoardName,
			limit: centerAroundPlayer ? 1 : 5,
			offset: centerAroundPlayer ? -1 : 0
		);
	}

	// TODO remove
	public void PostSampleScoresForTesting() {
		for (int i = 0; i < 20; i++) {
			Cloud.LoginAnonymously().Then (gamer => {
				uint coins = (uint) (UnityEngine.Random.value * 3000);
				uint runtime = (uint) (UnityEngine.Random.value * 100 * 60 * 10);
				// We can add information with the field "scoreInfo", however the data size is limited so we only attach crucial information.
				// Therefore, ghost data will be uploaded in another way.
				return gamer.Scores.Post (
					score: coins,
					board: ScoreBoardName,
					order: ScoreOrder.HighToLow,
					scoreInfo: Bundle.CreateObject("runtime", runtime).ToJson());
			}).Done ();
		}
	}

	public Promise<PostedGameScore> PostScore(uint coinsCollected, float runtime) {
		// Encode runtime in seconds to a fixed point (100ths of second)
		uint runtimeFixed = (uint) (runtime * 100);
		// Attach additional information with the score
		Bundle scoreInfo = Bundle.CreateObject (
			"runtime", runtimeFixed
		);
		// We can add information with the field "scoreInfo", however the data size is limited so we only attach crucial information.
		// Therefore, ghost data will be uploaded in another way.
		return Gamer.Scores.Post (
			score: coinsCollected,
			board: ScoreBoardName,
			order: ScoreOrder.HighToLow,
			scoreInfo: scoreInfo.ToJson());
	}
	#endregion
}
