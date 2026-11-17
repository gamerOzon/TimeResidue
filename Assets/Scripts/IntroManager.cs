using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public Renderer cylinderRenderer;
    [Tooltip("Video em PT. Se idioma=PT e existir, substitui o video EN.")]
    public VideoClip videoPt;

    [Header("Timing")]
    public float fadeStartTime = 50f;
    public float fadeDuration = 2f;

    [Header("Menu")]
    public GameObject menuObject;
    public GameObject textObject;        // EN: Text
    public GameObject textObjectPt;      // PT: TextPT

    [Header("Som do Menu")]
    public AudioClip menuSound;
    private AudioSource audioSource;

    private bool fading = false;
    private float fadeTimer = 0f;
    private bool menuShown = false;
    private float textTimer = 0f;
    private bool textWaiting = false;
    private bool textShown = false;

    void Start()
    {
        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("SceneElements"));
        
        if (menuObject != null)
            menuObject.SetActive(false);

        if (textObject != null)
            textObject.SetActive(false);

        if (textObjectPt != null)
            textObjectPt.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Idioma: troca o video para a versao PT se for o caso
        if (LanguageManager.Portugues && videoPt != null && videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = videoPt;
            videoPlayer.Play();
        }
    }

    void Update()
    {
        if (!fading && videoPlayer.isPlaying && videoPlayer.time >= fadeStartTime)
        {
            fading = true;
            Camera.main.cullingMask |= (1 << LayerMask.NameToLayer("SceneElements"));
        }

        if (fading)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            SetAlpha(cylinderRenderer, alpha);

            if (fadeTimer >= fadeDuration)
                cylinderRenderer.enabled = false;
        }

        // Mostra menu quando o áudio termina
        if (!menuShown && !videoPlayer.isPlaying && videoPlayer.time > 0)
        {
            menuShown = true;

            if (menuObject != null)
                menuObject.SetActive(true);

            if (menuSound != null)
            {
                audioSource.PlayOneShot(menuSound);
                textWaiting = true;
            }
        }

        // Aguarda 2 segundos após o som e mostra o texto
        if (textWaiting && !textShown)
        {
            textTimer += Time.deltaTime;
            if (textTimer >= 2f)
            {
                textShown = true;
                GameObject txt = (LanguageManager.Portugues && textObjectPt != null) ? textObjectPt : textObject;
                if (txt != null)
                    txt.SetActive(true);
            }
        }
    }

    void SetAlpha(Renderer r, float alpha)
    {
        Material mat = r.material;
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
    }
}