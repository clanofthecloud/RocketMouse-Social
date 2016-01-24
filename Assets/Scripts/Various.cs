using System;
using System.Collections;
using UnityEngine;

public static class Various
{

	public static IEnumerator ScaleLayerDown(GameObject layer, float totalTime, Action onFinished = null) {
		var speed = 1 / totalTime;
		for (float t = 1.0f; t > 0.0f; t -= Time.deltaTime * speed) {
			float time = Mathf.Pow(t, 3);
			layer.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), time);
			yield return null;
		}
		layer.transform.localScale = Vector3.zero;
		layer.SetActive(false);
		if (onFinished != null) onFinished();
	}

	public static IEnumerator ScaleLayerUp(GameObject layer, float totalTime) {
		var speed = 1 / totalTime;
		layer.SetActive(true);
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime * speed) {
			float time = Mathf.Pow(t, 3);
			layer.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), time);
			yield return null;
		}
		layer.transform.localScale = new Vector3(1, 1, 1);
	}
}
