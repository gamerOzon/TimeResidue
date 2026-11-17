using UnityEngine;
using System.Collections;

public class NarratorManagerAlternativa : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip audio17;
    public AudioClip audio17Pt;

    [Header("Tempo após narração para fechar")]
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
        AudioClip c17 = (LanguageManager.Portugues && audio17Pt != null) ? audio17Pt : audio17;
        if (c17 != null)
        {
            audioSource.clip = c17;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        yield return new WaitForSeconds(tempoAposFecho);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
