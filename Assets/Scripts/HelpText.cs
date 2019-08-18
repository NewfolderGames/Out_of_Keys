using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpText : MonoBehaviour {

	private Text text;
	[SerializeField]
	private float delay = 2f;
	private bool isDelay;
	[SerializeField]
	private float time = 2f;
	private float currentTime;
	[SerializeField]
	private Color start = Color.white;
	[SerializeField]
	private Color end = new Color(1, 1, 1, 0);

	private void Awake() {

		this.text = this.GetComponent<Text>();
		this.currentTime = this.time;
		this.isDelay = this.delay > 0;

		if (this.isDelay) this.Invoke("StopDelay", this.delay);

	}

	private void Update() {

		if (this.isDelay) return;

		this.text.color = Color.Lerp(this.start, this.end, 1 - (this.currentTime / this.time));
		if ((this.currentTime -= Time.deltaTime) <= 0) Destroy(this.gameObject);

	}

	private void StopDelay() { this.isDelay = false; }

}
