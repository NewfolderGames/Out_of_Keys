using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatus : MonoBehaviour {

	private static bool isRestart = false;

	public delegate void StatusEvent();

	public StatusEvent OnReady;
	public StatusEvent OnHurt;
	public StatusEvent OnDie;
	public StatusEvent OnWin;

	[Header("Components")]

	[SerializeField]
	private Transform spriteTransform;
	[SerializeField]
	private AudioSource audioSource;
	[SerializeField]
	private Animator animator;
	[SerializeField]
	private Cinemachine.CinemachineVirtualCamera virtualCamera;
	private Cinemachine.CinemachineBasicMultiChannelPerlin cameraNoise;
	[SerializeField]
	private RectTransform transitionTransform;

	[Header("Level")]
	
	[SerializeField]
	private AudioClip clearAudio;
	[Space()]
	[SerializeField]
	private bool isStartTransition = true;
	[SerializeField]
	private bool isTransitioning = false;
	[SerializeField]
	private float transitionDelay = 1f;
	private bool isTransitionDelay = true;
	[SerializeField]
	private float transitionTime = 1f;
	private float transitionCurrent;
	[Space()]
	[SerializeField]
	private GameObject levelTextObject;
	[SerializeField]
	private GameObject restartTextObject;
	[Space()]
	[SerializeField]
	private string nextLevel;

	[Header("Health")]

	[SerializeField]
	private bool isDead = false;
	[SerializeField]
	private GameObject deadPrefeb;
	[SerializeField]
	private int health = 1;
	[SerializeField]
	private AudioClip hurtAudio;

	private void Awake() {

		this.cameraNoise = this.virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
		this.transitionCurrent = this.transitionTime;

		if (!PlayerStatus.isRestart) {

			this.transitionTransform.localScale = Vector2.one;
			this.isTransitioning = true;
			this.Invoke("StopTransitionDelay", this.transitionDelay);

		} else {

			this.isTransitioning = false;
			this.isStartTransition = false;
			this.transitionTransform.localScale = Vector2.up;
			
			Destroy(levelTextObject);

		}

	}

	private void Start() {

		if (PlayerStatus.isRestart) this.OnReady?.Invoke();

	}

	private void OnTriggerEnter2D(Collider2D other) {

		if (other.CompareTag("Bamboo")) {

			this.isTransitioning = true;
			this.isStartTransition = false;
			this.Invoke("StopTransitionDelay", this.transitionDelay);

			this.OnWin?.Invoke();
			this.animator.SetTrigger("win");
			this.audioSource.PlayOneShot(clearAudio);
			
			PlayerStatus.isRestart = false;

		}

	}

	private void Update() {

		if (!this.isTransitioning && Input.GetKeyDown(KeyCode.Return)) {

			PlayerStatus.isRestart = true;
			SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);

		}

		if (this.isTransitionDelay || !this.isTransitioning) return;

		this.transitionCurrent -= Time.deltaTime;
		var rate = this.transitionCurrent / this.transitionTime;
		this.transitionTransform.localScale = Vector2.Lerp(Vector2.one, Vector2.up, this.isStartTransition ? 1 - (rate * rate) : (rate * rate));

		if ((this.transitionCurrent -= Time.deltaTime) <= 0) {

			this.isTransitioning = false;
			this.isTransitionDelay = true;

			if (this.isStartTransition) {

				this.isStartTransition = false;
				this.transitionCurrent = this.transitionTime;
				this.transitionTransform.localScale = Vector2.up;

				this.OnReady?.Invoke();

			} else {

				this.transitionTransform.localScale = Vector2.one;
				SceneManager.LoadSceneAsync(this.nextLevel, LoadSceneMode.Single);

			}

		}

	}

	public void Hurt() {

		if (--this.health <= 0 && !isDead) {

			var deadObject = Instantiate(this.deadPrefeb);

			var deadTransform = deadObject.GetComponent<Transform>();
			deadTransform.position = this.spriteTransform.position;
			deadTransform.localScale = this.spriteTransform.localScale;

			var deadRigid2D = deadObject.GetComponent<Rigidbody2D>();
			deadRigid2D.AddForce(new Vector2(Random.Range(-0.5f, 0.5f), 1) * 20, ForceMode2D.Impulse);
			deadRigid2D.AddTorque(Random.Range(-2.5f, 2.5f), ForceMode2D.Impulse);

			this.isDead = true;
			this.audioSource.PlayOneShot(this.hurtAudio);
			this.spriteTransform.gameObject.SetActive(false);
			this.restartTextObject.gameObject.SetActive(true);

			this.OnDie?.Invoke();

		}

	}

	private void StopTransitionDelay() { this.isTransitionDelay = false; }

}
