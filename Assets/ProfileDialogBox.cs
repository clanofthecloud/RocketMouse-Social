using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CotcSdk;
using CotcSdk.FacebookIntegration;
using System.Collections.Generic;

public class ProfileDialogBox : MonoBehaviour {

	public Social Social;
	public InputField NameField;
	public Button ConnectFacebookButton;
	private Gamer LastGamer;
	public ConfirmationDialog ConfirmationDialog;

	// Use this for initialization
	void Start () {
		Debug.Assert(Social != null);
		Debug.Assert(NameField != null);
		Debug.Assert(ConnectFacebookButton != null);
		Debug.Assert(ConfirmationDialog != null);
	}
	
	// Update is called once per frame
	void Update () {
		// Update the UI whenever the logged in gamer changes
		if (LastGamer != Social.CurrentGamer) {
			LastGamer = Social.CurrentGamer;
			Refresh();
		}
	}

	public void ConnectFacebookPressed() {
		if (Social.CurrentGamer.Network != LoginNetwork.Anonymous) {
			throw new UnityException("No need to convert non-anonymous account to Facebook");
		}

		Social.ConvertAccountToFb().Done();
	}

	public void DonePressed() {
		// If the name has been modified, save it to the profile
		if (Social.CurrentGamer["profile"]["displayName"] != NameField.text) {
			Bundle profile = Bundle.CreateObject("displayName", NameField.text);
			Social.CurrentGamer.Profile.Set(profile);
		}
		// Disappear
		StartCoroutine(Various.ScaleLayerDown(this.gameObject, 0.3f));
	}

	public void Refresh() {
		if (Social.CurrentGamer == null) return;
		NameField.text = Social.CurrentGamer["profile"]["displayName"];
		ConnectFacebookButton.enabled = (Social.CurrentGamer.Network == LoginNetwork.Anonymous);
	}
}
