using System.Collections;
using UnityEngine;

public class TutorialStartScript : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject drMarcus;
    [SerializeField] private GameObject tubePlayer;
    [SerializeField] private AudioClip glassSfx;
    [SerializeField] private AudioClip glassBreakSfx;
    [SerializeField] private int glassHitsLimit = 0;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer spriteRendererTube;
    private AudioSource audioSource;

    private int hitsOnGlass;
    private bool hasPressed = false; 

    void Start()
    {
        spriteRenderer = player.GetComponent<SpriteRenderer>();
        spriteRendererTube = tubePlayer.GetComponent<SpriteRenderer>();
        animator = drMarcus.GetComponent<Animator>();
        audioSource = player.GetComponent<AudioSource>();

        player.GetComponent<CircleCollider2D>().enabled = false;

        PlayerMovement.controlEnabled = false;
        PlayerAttack.controlEnabled = false;
        TimerManager.pauseTimer = true;
        spriteRenderer.enabled = false; 
    }

    void Update()
    {
        if (hitsOnGlass == 2)
        {
            animator.Play("Idle");
        }
        else if (hitsOnGlass == 3)
        {
            animator.Play("Scared");
        }
        else if (hitsOnGlass == 4 && hasPressed == false)
        {
            animator.Play("ButtonClick");
            TimerManager.pauseTimer = false; 
            hasPressed = true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            GlassHit();
        }
    }

    private void GlassHit()
    {
        hitsOnGlass++;

        if (hitsOnGlass == glassHitsLimit)
        {
            StartCoroutine(SpawnPlayer());
        }
        else if (hitsOnGlass < glassHitsLimit)
        {
            audioSource.PlayOneShot(glassSfx);
        }
    }

    private IEnumerator SpawnPlayer()
    {
        audioSource.PlayOneShot(glassBreakSfx);

        yield return new WaitForSeconds(1);
        player.SetActive(true);
        spriteRenderer.enabled = true;
        spriteRenderer.color = Color.clear;
        Vector2 originalPosition = player.transform.position;

        float spawnCounter = 1;
        while (spawnCounter > 0)
        {
            spriteRenderer.color = new Color(1, 1, 1, 1 - spawnCounter);
            spriteRendererTube.color = new Color(1, 1, 1, spawnCounter);
            player.transform.position = Vector2.MoveTowards(player.transform.position, new Vector2(originalPosition.x + 1.3f, originalPosition.y), Time.deltaTime * .75f);

            yield return null;
            spawnCounter = Mathf.Max(0, spawnCounter - Time.deltaTime);
        }

        player.GetComponent<CircleCollider2D>().enabled = true;
        PlayerMovement.controlEnabled = true;
        PlayerAttack.controlEnabled = true;
        enabled = false;
    }
}
