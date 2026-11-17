using UnityEngine;
using System.Collections;

public class NarratorManagerFuturoPlastico : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip narracao;
    public AudioClip narracaoPt;

    [Header("Fecho (opcional)")]
    [Tooltip("Se ligado, fecha a aplicacao depois da narracao (como na cena Alternativa).")]
    public bool fecharAoTerminar = false;
    public float tempoAposFecho = 5f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        StartCoroutine(Sequencia());
    }

    IEnumerator Sequencia()
    {
        AudioClip cNar = (LanguageManager.Portugues && narracaoPt != null) ? narracaoPt : narracao;
        if (cNar != null)
        {
            audioSource.clip = cNar;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
        }
        else
        {
            Debug.LogWarning("NarratorManagerFuturoPlastico: nenhum AudioClip atribuido em 'narracao'.");
        }

        if (fecharAoTerminar)
        {
            yield return new WaitForSeconds(tempoAposFecho);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
