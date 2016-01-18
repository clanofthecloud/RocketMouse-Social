using UnityEngine;
using System.Collections;

public class MouseController : MonoBehaviour {

	public float jetpackForce = 75.0f;

	public float forwardMovementSpeed = 3.0f;

	public Transform groundCheckTransform;
	
	private bool grounded;
	
	public LayerMask groundCheckLayerMask;
	
	Animator animator;

	public ParticleSystem jetpack;

	private bool dead = false;

	private uint coins = 0;
	private float runtime = 0;

	public Texture2D coinIconTexture;

	public AudioClip coinCollectSound;

	public AudioSource jetpackAudio;
	
	public AudioSource footstepsAudio;

	public ParallaxScroll parallax;

	public Leaderboards leaderboards;

	public GameObject gameOverLayer;
	bool gameOverHasBeenShown;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate () 
	{
		bool jetpackActive = Input.GetButton("Fire1");
		
		jetpackActive = jetpackActive && !dead;
		
		if (jetpackActive)
		{
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, jetpackForce));
		}
		
		if (!dead)
		{
			// Advance only if not grounded
			Vector2 newVelocity = GetComponent<Rigidbody2D>().velocity;
			newVelocity.x = forwardMovementSpeed;
			GetComponent<Rigidbody2D>().velocity = newVelocity;
		}
		
		UpdateGroundedStatus();
		
		AdjustJetpack(jetpackActive);

		AdjustFootstepsAndJetpackSound(jetpackActive);

		parallax.offset = transform.position.x;

		// Count the running time
		runtime += Time.fixedDeltaTime;
	} 

	void UpdateGroundedStatus()
	{
		//1
		grounded = Physics2D.OverlapCircle(groundCheckTransform.position, 0.1f, groundCheckLayerMask);
		
		//2
		animator.SetBool("grounded", grounded);
	}

	void AdjustJetpack (bool jetpackActive)
	{
		jetpack.enableEmission = !grounded;
		jetpack.emissionRate = jetpackActive ? 300.0f : 75.0f; 
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.gameObject.CompareTag("Coins"))
			CollectCoin(collider);
		else
			HitByLaser(collider);
	}
		
	void HitByLaser(Collider2D laserCollider)
	{
		if (!dead)
			laserCollider.gameObject.GetComponent<AudioSource>().Play();

		dead = true;

		animator.SetBool("dead", true);
	}

	void CollectCoin(Collider2D coinCollider)
	{
		coins++;
		
		Destroy(coinCollider.gameObject);

		AudioSource.PlayClipAtPoint(coinCollectSound, transform.position);
	}

	void OnGUI()
	{
		DisplayCoinsCount();

		DisplayRestartButton();
	}

	void DisplayCoinsCount()
	{
		Rect coinIconRect = new Rect(10, 10, 32, 32);
		GUI.DrawTexture(coinIconRect, coinIconTexture);                         
		
		GUIStyle style = new GUIStyle();
		style.fontSize = 30;
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.yellow;
		
		Rect labelRect = new Rect(coinIconRect.xMax, coinIconRect.y, 60, 32);
		GUI.Label(labelRect, coins.ToString(), style);
	}

	void DisplayRestartButton()
	{
		if (dead && grounded)
		{
			if (!gameOverHasBeenShown) {
				gameOverHasBeenShown = true;
				leaderboards.PostScoreAndUpdateLeaderboards(coins, runtime);
				// Progressively show the canvas
				gameOverLayer.SetActive(true);
				StartCoroutine(ScaleGameOverLayerUp());
			}
		}
	}

	IEnumerator ScaleGameOverLayerUp() {
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime * 2) {
			float time = Mathf.Pow(t, 3);
			gameOverLayer.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), time);
			yield return null;
		}
		gameOverLayer.transform.localScale = new Vector3(1, 1, 1);
	}

	void AdjustFootstepsAndJetpackSound(bool jetpackActive)    
	{
		footstepsAudio.enabled = !dead && grounded;
		
		jetpackAudio.enabled =  !dead && !grounded;
		jetpackAudio.volume = jetpackActive ? 1.0f : 0.5f;        
	}

	public void RestartGame() {
		Application.LoadLevel (Application.loadedLevelName);
	}
}
