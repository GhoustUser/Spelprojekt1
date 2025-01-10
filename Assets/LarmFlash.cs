using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LarmFlash : MonoBehaviour
{
    private Image image;
    [SerializeField] private bool tutorialScene;
    public static bool enableLarm;

    private void Start()
    {
        image = GetComponent<Image>();   
    }

    private void Update()
    {
        if (tutorialScene)
        {
            if (!enableLarm) return;
            StartCoroutine(FlashLight());
            enableLarm = false;
        }
    }

    private IEnumerator FlashLight()
    {
        Color flashColor = new Color(1, 0.1f, 0.1f, 0.25f);
        float flashCooldown = 3;
        float flashTime = .5f;
        while (true)
        {
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
