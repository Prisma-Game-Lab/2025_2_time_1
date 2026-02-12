using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FeedbackVisual : MonoBehaviour
{
    public Image imagemSangue;
    public float velocidadeSumiço = 2f;

    public void AtivarSangue()
    {
        StopAllCoroutines();
        StartCoroutine(SumirSuave());
    }

    IEnumerator SumirSuave()
    {
        // Aparece instantaneamente
        Color c = imagemSangue.color;
        c.a = 1f;
        imagemSangue.color = c;

        // Some gradualmente
        while (c.a > 0)
        {
            c.a -= Time.deltaTime * velocidadeSumiço;
            imagemSangue.color = c;
            yield return null;
        }
    }
}