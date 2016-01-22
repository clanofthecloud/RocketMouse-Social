using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProfileDialogBox : MonoBehaviour {

	public Social Social;
	public InputField NameField;

	// Use this for initialization
	void Start () {
		NameField.text = Social.CurrentGamer["profile"]["displayName"];
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DonePressed() {
	}
}
