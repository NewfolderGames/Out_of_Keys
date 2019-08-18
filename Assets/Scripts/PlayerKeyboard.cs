using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerKeyboard : MonoBehaviour {

	public static readonly KeyCode[][] KeyCodes = new KeyCode[][] {
		new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P },
		new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L },
		new KeyCode[] { KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M }
	};

	[Header("Components")]

	[SerializeField]
	private RectTransform keyboardTransform;
	[SerializeField]
	private AudioSource audioSource;
	[SerializeField]
	private PlayerController controller;
	[SerializeField]
	private PlayerStatus status;
	[SerializeField]
	private Cinemachine.CinemachineVirtualCamera virtualCamera;
	private Cinemachine.CinemachineBasicMultiChannelPerlin cameraNoise;

	[Header("Settings")]

	[SerializeField]
	private KeyCode[] unlocked;
	[SerializeField]
	private bool isEnabled = false;

	[Header("Resources")]

	[SerializeField]
	private GameObject keyPrefeb;
	[SerializeField]
	private Sprite[] keySprites;
	[SerializeField]
	private AudioClip keyAudioPress;

	public readonly Dictionary<KeyCode, PlayerKeyboardKey> keys = new Dictionary<KeyCode, PlayerKeyboardKey>();

	private void Awake() {

		this.cameraNoise = this.virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();

		for (int row = 0; row < PlayerKeyboard.KeyCodes.Length; row++) {

			var keyRow = PlayerKeyboard.KeyCodes[row];

			for (int index = 0; index < keyRow.Length; index++) {

				var keyCode = keyRow[index];

				var key = new PlayerKeyboardKey(keyCode, Instantiate(this.keyPrefeb));
				key.transform.SetParent(this.keyboardTransform);
				key.transform.localPosition = new Vector3(5.5f + (row * 4) + (index * 12), 7.5f + ((2 - row) * 12), 0);
				key.transform.localScale = Vector3.one;
				key.button.interactable = false;
				key.button.onClick.AddListener(() => this.Use(keyCode));

				this.keys.Add(keyCode, key);

			}

		}

		foreach (var keyCode in this.unlocked) this.Unlock(keyCode);

		this.keyboardTransform.sizeDelta = new Vector2(PlayerKeyboard.KeyCodes[0].Length * 12 - 1, 0);

		// Event.

		PlayerStatus.StatusEvent disable = () => {

			this.isEnabled = false;
			foreach (var entry in this.keys) entry.Value.button.interactable = false;

		};

		this.status.OnReady += () => this.isEnabled = true;
		this.status.OnDie += disable;
		this.status.OnWin += disable;

	}

	private void Update() {

		if (!this.isEnabled) return;

		foreach (var keyCode in this.keys.Keys) {

			if (Input.GetKeyDown(keyCode)) {

				this.Use(keyCode);

			} 

		}

	}

	public void Use(KeyCode keyCode) {

		if (!this.isEnabled || !this.keys.ContainsKey(keyCode)) return;

		var key = this.keys[keyCode];

		if (key.type == PlayerKeyboardKey.Type.None || key.isUsed) return;

		key.type = PlayerKeyboardKey.Type.Used;
		key.isUsed = true;
		key.image.sprite = this.keySprites[(int)PlayerKeyboardKey.Type.Used];
		key.button.interactable = false;
		key.Run?.Invoke();

		StartCoroutine("UseShake");
		audioSource.PlayOneShot(this.keyAudioPress);

	}

	public void Add(KeyCode keyCode, PlayerKeyboardKey.Type type, Action run) {

		if (!this.keys.ContainsKey(keyCode)) return;

		var key = this.keys[keyCode];

		key.type = type;
		key.isUsed = false;
		key.image.sprite = this.keySprites[(int)type];
		key.button.interactable = true;
		key.Run = run;

	}

	private void Unlock(KeyCode keyCode, PlayerKeyboardKey.Type type = PlayerKeyboardKey.Type.Wooden) {

		if (!this.keys.ContainsKey(keyCode)) return;

		Action action = null;

		switch (keyCode) {

			case KeyCode.W: action = () => this.controller.Run(PlayerController.Command.Jump); break;
			case KeyCode.A: action = () => this.controller.Run(PlayerController.Command.Left); break;
			case KeyCode.S: action = () => this.controller.Run(PlayerController.Command.Stop); break;
			case KeyCode.D: action = () => this.controller.Run(PlayerController.Command.Right); break;
			case KeyCode.Q: action = () => this.controller.Run(PlayerController.Command.WallJumpLeft); break;
			case KeyCode.E: action = () => this.controller.Run(PlayerController.Command.WallJumpRight); break;
			case KeyCode.M: action = () => this.status.Hurt(); break;

		}
		
		this.Add(keyCode, type, action);

	}

	private IEnumerator UseShake() {

		this.cameraNoise.m_AmplitudeGain = 0.25f;
		yield return new WaitForSecondsRealtime(0.1f);
		this.cameraNoise.m_AmplitudeGain = 0;

	}

}

[System.Serializable]
public class PlayerKeyboardKey {

	public enum Type { None, Used, Wooden, Iron, Gold, Diamond, Uranium, Unoptainium }

	public readonly KeyCode keyCode;
	public Type type;
	public bool isUsed = false;
	public Action Run;

	[Header("UI")]
	public readonly GameObject gameObject;
	public readonly RectTransform transform;
	public readonly Image image;
	public readonly Button button;

	public PlayerKeyboardKey (KeyCode keyCode, GameObject keyObject) {

		this.keyCode = keyCode;
		this.gameObject = keyObject;
		this.transform = keyObject.GetComponent<RectTransform>();
		this.image = keyObject.GetComponent<Image>();
		this.button = keyObject.GetComponent<Button>();

	}

}