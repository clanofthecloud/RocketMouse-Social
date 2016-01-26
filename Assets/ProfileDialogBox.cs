using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CotcSdk;
using CotcSdk.FacebookIntegration;
using System.Collections.Generic;
using System;

public class ProfileDialogBox : MonoBehaviour {

	public Social Social;
	public InputField NameField;
	public Button ConnectFacebookButton;
	// Update these as well once the profile is changed
	public Leaderboards Leaderboards;
	private string OriginalName;

	// Use this for initialization
	void Start () {
		Debug.Assert(Social != null);
		Debug.Assert(NameField != null);
		Debug.Assert(ConnectFacebookButton != null);
		Debug.Assert(Leaderboards != null);
		Social.GamerChanged += Social_GamerChanged;
		Refresh();
	}

	void Destroy() {
		Social.GamerChanged -= Social_GamerChanged;
	}

	void Social_GamerChanged(object sender, EventArgs e) {
		// Update the UI whenever the logged in gamer changes
		Refresh();
	}
	
	// Update is called once per frame
	void Update () {}

	public void ConnectFacebookPressed() {
		if (Social.CurrentGamer.Network != LoginNetwork.Anonymous) {
			throw new UnityException("No need to convert non-anonymous account to Facebook");
		}

		Social.ConvertAccountToFb().Done();
	}

	public void DonePressed() {
		// If the name has been modified, save it to the profile
		if (OriginalName != NameField.text) {
			Bundle profile = Bundle.CreateObject("displayName", NameField.text);
			Social.CurrentGamer.Profile.Set(profile).Done(done => {
				Leaderboards.UpdateLeaderboards();
			});
		}
		// Disappear
		StartCoroutine(Various.ScaleLayerDown(this.gameObject, 0.3f));
	}

	public void Refresh() {
		if (Social.CurrentGamer == null) return;
		Social.CurrentGamer.Profile.Get().Done(profile => {
			OriginalName = profile["displayName"];
			NameField.text = OriginalName;
		});
		ConnectFacebookButton.enabled = (Social.CurrentGamer.Network == LoginNetwork.Anonymous);
	}
}
