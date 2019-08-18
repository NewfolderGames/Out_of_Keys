using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneScript : MonoBehaviour {

	[Header("Red Panda")]

	public GameObject deadPrefeb;
	public GameObject redpandaObject;
	public AudioSource audioSource;
	public AudioClip deadAudio;

	[Header("Transition")]

	public bool isTransitioning;
	public float transitionTime = 1f;
	public float transitionCurrent;
	public RectTransform transitionTransform;

	[Header("Scene")]

	public string startLevel = "level_1_1";

	private void Update() {
		
		if (!this.isTransitioning) return;

		this.transitionCurrent -= Time.deltaTime;
		var rate = this.transitionCurrent / this.transitionTime;
		this.transitionTransform.localScale = Vector2.Lerp(Vector2.one, Vector2.up, rate * rate);

		if ((this.transitionCurrent -= Time.deltaTime) <= 0) SceneManager.LoadSceneAsync(this.startLevel, LoadSceneMode.Single);

	}

	public void StartGame() {

		this.isTransitioning = true;
		this.transitionCurrent = this.transitionTime;

	}

	public void Oof() {

		var deadObject = Instantiate(this.deadPrefeb);

		var deadTransform = deadObject.GetComponent<Transform>();
		deadTransform.position = new Vector2(0.0625f, -1.6825f);

		var deadRigid2D = deadObject.GetComponent<Rigidbody2D>();
		deadRigid2D.AddForce(new Vector2(Random.Range(-0.5f, 0.5f), 1) * 20, ForceMode2D.Impulse);
		deadRigid2D.AddTorque(Random.Range(-2.5f, 2.5f), ForceMode2D.Impulse);

		this.audioSource.PlayOneShot(this.deadAudio);

		Destroy(this.redpandaObject);
		Destroy(deadObject, 10);

	}

	public void QuitGame() { Application.Quit(); }

}
