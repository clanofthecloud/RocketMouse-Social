using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MouseController : MonoBehaviour {

	public float jetpackForce = 75.0f;

	public float initialForwardMovementSpeed = 2.5f;
	public float incrementForwardMovementSpeedPerSecond = 0.04f;

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

	public GameObject gameOverLayer, editProfileLayer;
	public Text coinText;
	private bool gameOverHasBeenShown;
	private float forwardMovementSpeed;

	// Use this for initialization
	void Start () {
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		animator = GetComponent<Animator>();	
		forwardMovementSpeed = initialForwardMovementSpeed;
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

		forwardMovementSpeed += Time.fixedDeltaTime * incrementForwardMovementSpeedPerSecond;

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
		var em = jetpack.emission;
		em.enabled = !grounded;
		em.rate = new ParticleSystem.MinMaxCurve(jetpackActive ? 300.0f : 75.0f);
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
		coinText.text = "x" + coins;
	}

	void DisplayRestartButton()
	{
		if (dead && grounded)
		{
			if (!gameOverHasBeenShown) {
				gameOverHasBeenShown = true;
				leaderboards.PostScoreAndUpdateLeaderboards(coins, runtime);
				// Progressively show the canvas
				StartCoroutine(Various.ScaleLayerUp(gameOverLayer, 0.5f));
			}
		}
	}

	void AdjustFootstepsAndJetpackSound(bool jetpackActive)    
	{
		footstepsAudio.enabled = !dead && grounded;
		
		jetpackAudio.enabled =  !dead && !grounded;
		jetpackAudio.volume = jetpackActive ? 1.0f : 0.5f;        
	}

	public void RestartGame() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void ShowProfileDialog() {
		StartCoroutine(Various.ScaleLayerUp(editProfileLayer, 0.3f));
	}
}
