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
		Promise.Debug_OutputAllExceptions = true;

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
		Debug.LogWarning(PlayerPrefs.HasKey ("GamerInfo"));
		if (!PlayerPrefs.HasKey ("GamerInfo")) {
			MakeAnonymousAccount ();
		}
		else {
			// Log back in
			GamerInfo info = new GamerInfo(PlayerPrefs.GetString("GamerInfo"));
			Debug.Log ("Attempting to log back with existing credentials");
			Cloud.Login (LoginNetwork.Anonymous, info.GamerId, info.GamerSecret, true)
			.Then (gamer => DidLogin(gamer))
			.Catch (ex => {
				Debug.LogError("Failed to log back in with stored credentials. Restarting process.");
				MakeAnonymousAccount ();
			});
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
}
