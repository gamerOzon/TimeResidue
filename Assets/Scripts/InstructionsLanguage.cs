using UnityEngine;
using System.Collections;

/// <summary>
/// Cena Instructions: dois botoes (EN / PT) por proximidade da mao.
/// Ao premir um: define o idioma, troca a textura do cilindro,
/// esconde os botoes e mostra o cilindro de instrucoes.
/// </summary>
public class InstructionsLanguage : MonoBehaviour
{
    [Header("Botoes (raiz p/ detecao de proximidade)")]
    public Transform botaoIngles;
    public Transform botaoPortugues;
    [Tooltip("Parte visual que desce ao premir (opcional).")]
    public Transform botaoInglesParte;
    public Transform botaoPortuguesParte;
    public float pressedY = -0.0096f;
    public float distanciaAtivacao = 0.3f;

    [Header("Esconder ao escolher (botoes + labels)")]
    public GameObject[] outrosEsconder;

    [Header("Ecra de instrucoes (cilindro) - comeca escondido")]
    public GameObject instructionsScreen;
    public Renderer screenRenderer;
    public Texture instrucoesEn;
    public Texture instrucoesPt;

    [Header("Som / Feedback")]
    public AudioClip clickSound;
    public float vibracaoDuracao = 0.3f;
    public float vibracaoIntensidade = 0.5f;

    private AudioSource audioSource;
    private Transform maoEsquerda;
    private Transform maoDireita;
    private bool escolhido = false;

    /// <summary>True so depois de escolher idioma + mostrar instrucoes. Trava o avanco prematuro (InstructionsAdvance).</summary>
    public static bool IdiomaEscolhido = false;

    void Start()
    {
        IdiomaEscolhido = false;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (instructionsScreen != null) instructionsScreen.SetActive(false);

        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;
    }

    void Update()
    {
        if (escolhido) return;

        OVRInput.Controller c;
        if (PertoDe(botaoIngles, out c))
            Selecionar(false, botaoInglesParte, c);
        else if (PertoDe(botaoPortugues, out c))
            Selecionar(true, botaoPortuguesParte, c);
    }

    bool PertoDe(Transform botao, out OVRInput.Controller controller)
    {
        controller = OVRInput.Controller.RTouch;
        if (botao == null) return false;

        bool esq = maoEsquerda != null &&
            Vector3.Distance(maoEsquerda.position, botao.position) < distanciaAtivacao;
        bool dir = maoDireita != null &&
            Vector3.Distance(maoDireita.position, botao.position) < distanciaAtivacao;

        if (esq) { controller = OVRInput.Controller.LTouch; return true; }
        if (dir) { controller = OVRInput.Controller.RTouch; return true; }
        return false;
    }

    void Selecionar(bool portugues, Transform parte, OVRInput.Controller controller)
    {
        escolhido = true;
        LanguageManager.Portugues = portugues;

        // anima o botao a descer
        if (parte != null)
        {
            Vector3 p = parte.localPosition;
            p.y = pressedY;
            parte.localPosition = p;
        }

        // som + vibracao
        if (clickSound != null) audioSource.PlayOneShot(clickSound);
        StartCoroutine(Vibrar(controller));

        // configura a textura do cilindro conforme idioma
        if (screenRenderer != null)
        {
            Texture t = portugues ? instrucoesPt : instrucoesEn;
            if (t != null) screenRenderer.material.SetTexture("_BaseMap", t);
        }

        float delay = clickSound != null ? clickSound.length : 0.4f;
        StartCoroutine(Finalizar(delay));
    }

    IEnumerator Finalizar(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (outrosEsconder != null)
        {
            foreach (var o in outrosEsconder)
                if (o != null) o.SetActive(false);
        }
        if (instructionsScreen != null) instructionsScreen.SetActive(true);

        // Liberta o avanco para MenuAndIntro so agora (apos instrucoes visiveis)
        IdiomaEscolhido = true;
    }

    IEnumerator Vibrar(OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(1f, vibracaoIntensidade, controller);
        yield return new WaitForSeconds(vibracaoDuracao);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }
}
