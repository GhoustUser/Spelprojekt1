using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LarmFlash : MonoBehaviour
{
    private Image image;
    [SerializeField] private bool tutorialScene;
    public static bool enableLarm;
    private bool larmEnabled;
    private bool larmStarted;

    private void Start()
    {
        image = GetComponent<Image>();
        larmEnabled = false;
    }

    private void Update()
    {
        if (TimerManager.timer < 60) larmEnabled = true;
        if (tutorialScene)
        {
            if (!enableLarm) return;
            //StartCoroutine(FlashLight());
            enableLarm = false;
        }
        else if (larmEnabled && !larmStarted)
        {
            StartCoroutine(FlashLight());
            larmStarted = true;
        }
    }

    private IEnumerator FlashLight()
    {
        Color flashColor = new Color(1, 0.1f, 0.1f, 0.25f);
        float flashCooldown = 3;
        float flashTime = .5f;
        while (true)
        {
            if (!tutorialScene) flashCooldown = 2.5f + (4.5f * TimerManager.timer / 600);
            float flashCounter = 0;
            while (flashCounter < flashTime)
            {
                image.color = Color.clear + (flashColor * (flashCounter / flashTime));
                yield return null;
                flashCounter += Time.deltaTime;
            }

            yield return new WaitForSeconds(0.2f);

            flashCounter = 0;
            while (flashCounter < flashTime)
            {
                image.color = flashColor - (flashColor * (flashCounter / flashTime));
                yield return null;
                flashCounter += Time.deltaTime;
            }

            yield return new WaitForSeconds(flashCooldown);
        }
    }
}
