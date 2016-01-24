using UnityEngine;
using System.Collections;
using CotcSdk;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour {
	private const float AnimationDuration = 0.3f;
	public GameObject ContentsGameObject;
	public Button OkButton, CancelButton;
	public Text TitleText, MessageText;
	RawImage Backdrop;
	Promise<int> CurrentPromise;

	// Use this for initialization
	void Start () {
		Debug.Assert(ContentsGameObject != null);
		Debug.Assert(OkButton != null && CancelButton != null);
		Debug.Assert(TitleText != null && MessageText != null);

		this.gameObject.SetActive(false);
		Backdrop = GetComponent<RawImage>();
		OkButton.onClick.AddListener(() => {
			HideWithResult(1);
		});
		CancelButton.onClick.AddListener(() => {
			HideWithResult(0);
		});
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void HideWithResult(int result) {
		var prom = CurrentPromise;
		StartCoroutine(Various.ScaleLayerDown(ContentsGameObject, AnimationDuration,
			() => Backdrop.gameObject.SetActive(false)));
		CurrentPromise = null;
		prom.PostResult(result);
	}

	// Returns a promise that is resolved with the index of the button pressed (0=cancel, 1=OK).
	public Promise<int> Show(string title, string text, bool withCancelButton = true) {
		if (CurrentPromise != null) {
			// Return as cancelled
			CurrentPromise.PostResult(0);
		}

		CurrentPromise = new Promise<int>();
		TitleText.text = title;
		MessageText.text = text;
		CancelButton.gameObject.SetActive(withCancelButton);
		Backdrop.gameObject.SetActive(true);
		gameObject.SetActive(true);
		StartCoroutine(Various.ScaleLayerUp(ContentsGameObject, AnimationDuration));
		return CurrentPromise;
	}
}
