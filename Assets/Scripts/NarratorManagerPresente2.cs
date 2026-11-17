using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NarratorManagerPresente2 : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip audio16;
    public AudioClip audio16Pt;
    public AudioClip audio18;

    [Header("Portal")]
    public GameObject portal;

    [Header("Setas e Textos")]
    public GameObject arrowBook;
    public GameObject arrowBall;
    public GameObject textBook;      // EN: textplastic
    public GameObject textBall;      // EN: textwood
    public GameObject textBookPt;    // PT: textplastic_pt
    public GameObject textBallPt;    // PT: textwood_pt

    [Header("Objetos")]
    public Transform livrodequimica;
    public Transform bolademadeira;

    [Header("Cenas de destino")]
    public string cenaAlternativa = "Alternativa";   // escolha: bola
    public string cenaFuturo = "FuturoPlastico";     // escolha: livro

    [Header("Deteção de Hover")]
    public float distanciaAgarro = 0.3f;

    [Header("Ajuste do Livro na mao")]
    [Tooltip("Posicao do livro relativa a mao ao agarrar.")]
    public Vector3 offsetLivro = new Vector3(0f, -0.03f, 0.04f);
    [Tooltip("Rotacao do livro relativa a mao ao agarrar.")]
    public Vector3 rotacaoLivro = new Vector3(0f, 180f, -45f);

    [Header("Ajuste da Bola na mao")]
    [Tooltip("Posicao da bola relativa a mao ao agarrar.")]
    public Vector3 offsetBola = new Vector3(0f, -0.03f, 0f);
    [Tooltip("Rotacao da bola relativa a mao ao agarrar.")]
    public Vector3 rotacaoBola = Vector3.zero;

    [Header("Feedback")]
    public float vibracaoDuracao = 0.3f;
    public float vibracaoIntensidade = 0.5f;

    [Header("Sons UI")]
    public AudioClip somArrows;
    [Tooltip("Volume do som da seta. >1 amplifica.")]
    public float volumeArrows = 2f;
    [Tooltip("Segundos ate repetir o som da seta se a pessoa nao escolher.")]
    public float intervaloLembrete = 5f;

    private AudioSource audioSource;
    private Transform maoEsquerda;
    private Transform maoDireita;
    private Transform cabeca;
    private bool escolheu = false;
    private bool esperandoEscolha = false;
    private bool esperandoPortal = false;

    private enum Escolha { Nenhuma, Livro, Bola }
    private Escolha escolhaAtual = Escolha.Nenhuma;

    [Header("Troca de objeto")]
    [Tooltip("Tempo minimo entre trocas, evita oscilacao se os objetos estiverem perto.")]
    public float cooldownTroca = 0.4f;
    private float tempoUltimaTroca = -999f;

    // estado inicial dos objetos (para devolver ao sitio na troca)
    private class EstadoInicial
    {
        public Transform parent;
        public Vector3 pos;
        public Quaternion rot;
        public bool kinematic;
        public bool gravity;
    }
    private EstadoInicial estLivro;
    private EstadoInicial estBola;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        if (portal != null) portal.SetActive(false);
        if (arrowBook != null) arrowBook.SetActive(false);
        if (arrowBall != null) arrowBall.SetActive(false);
        if (textBook != null) textBook.SetActive(false);
        if (textBall != null) textBall.SetActive(false);
        if (textBookPt != null) textBookPt.SetActive(false);
        if (textBallPt != null) textBallPt.SetActive(false);

        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        GameObject cab = GameObject.Find("CenterEyeAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;
        if (cab != null) cabeca = cab.transform;

        estLivro = Capturar(livrodequimica);
        estBola = Capturar(bolademadeira);

        StartCoroutine(Sequencia());
    }

    IEnumerator Sequencia()
    {
        if (portal != null) portal.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (portal != null) portal.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        AudioClip c16 = (LanguageManager.Portugues && audio16Pt != null) ? audio16Pt : audio16;
        if (c16 != null)
        {
            audioSource.clip = c16;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        yield return new WaitForSeconds(0.5f);

        if (arrowBook != null) arrowBook.SetActive(true);
        if (arrowBall != null) arrowBall.SetActive(true);
        GameObject tBook = (LanguageManager.Portugues && textBookPt != null) ? textBookPt : textBook;
        GameObject tBall = (LanguageManager.Portugues && textBallPt != null) ? textBallPt : textBall;
        if (tBook != null) tBook.SetActive(true);
        if (tBall != null) tBall.SetActive(true);

        if (somArrows != null)
            audioSource.PlayOneShot(somArrows, volumeArrows);

        esperandoEscolha = true;
        Coroutine lembrete = StartCoroutine(LembreteSom());
        yield return new WaitUntil(() => escolheu);
        StopCoroutine(lembrete);
    }

    IEnumerator LembreteSom()
    {
        while (!escolheu)
        {
            yield return new WaitForSeconds(intervaloLembrete);
            if (escolheu) yield break;
            if (somArrows != null)
                audioSource.PlayOneShot(somArrows, volumeArrows);
        }
    }

    void Update()
    {
        if (esperandoEscolha && !escolheu)
        {
            Transform maoLivro = MaoProxima(livrodequimica);
            Transform maoBola = MaoProxima(bolademadeira);

            if (maoLivro != null)
            {
                Agarrar(Escolha.Livro, maoLivro);
                AbrirPortal();
            }
            else if (maoBola != null)
            {
                Agarrar(Escolha.Bola, maoBola);
                AbrirPortal();
            }
        }
        // Janela de troca: ja escolheu mas ainda nao atravessou o portal.
        // Se a mao chegar no OUTRO objeto, devolve o atual ao sitio e troca.
        else if (escolheu && esperandoPortal
                 && Time.time - tempoUltimaTroca >= cooldownTroca)
        {
            if (escolhaAtual == Escolha.Livro)
            {
                Transform m = MaoProxima(bolademadeira);
                if (m != null) Trocar(Escolha.Bola, m);
            }
            else if (escolhaAtual == Escolha.Bola)
            {
                Transform m = MaoProxima(livrodequimica);
                if (m != null) Trocar(Escolha.Livro, m);
            }
        }

        if (esperandoPortal && cabeca != null && portal != null)
        {
            Collider col = portal.GetComponent<Collider>();
            if (col != null && col.bounds.Contains(cabeca.position))
            {
                esperandoPortal = false;
                if (escolhaAtual == Escolha.Livro)
                    StartCoroutine(ChegouPortalLivro());
                else if (escolhaAtual == Escolha.Bola)
                    StartCoroutine(ChegouPortalBola());
            }
        }
    }

    void AbrirPortal()
    {
        if (arrowBook != null) arrowBook.SetActive(false);
        if (arrowBall != null) arrowBall.SetActive(false);
        if (textBook != null) textBook.SetActive(false);
        if (textBall != null) textBall.SetActive(false);
        if (textBookPt != null) textBookPt.SetActive(false);
        if (textBallPt != null) textBallPt.SetActive(false);

        if (portal != null) portal.SetActive(true);
        esperandoPortal = true;
    }

    Transform MaoProxima(Transform objeto)
    {
        if (objeto == null) return null;
        if (maoEsquerda != null && Vector3.Distance(maoEsquerda.position, objeto.position) < distanciaAgarro)
            return maoEsquerda;
        if (maoDireita != null && Vector3.Distance(maoDireita.position, objeto.position) < distanciaAgarro)
            return maoDireita;
        return null;
    }

    void ColaNaMao(Transform objeto, Transform mao, Vector3 offset, Vector3 rotacao)
    {
        Rigidbody rb = objeto.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        objeto.SetParent(mao);
        objeto.localRotation = Quaternion.Euler(rotacao);
        objeto.localPosition = Vector3.zero;

        // Coloca o CENTRO visual do objeto no ponto desejado da mao,
        // independente de onde esta o pivot do modelo (assim nao "foge" ao rodar).
        Renderer[] rends = objeto.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            Vector3 alvoWorld = mao.TransformPoint(offset);
            objeto.position += alvoWorld - b.center;
        }
        else
        {
            objeto.localPosition = offset;
        }

        OVRInput.Controller controller = (mao == maoEsquerda)
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;
        StartCoroutine(Vibrar(controller));
    }

    void Agarrar(Escolha esc, Transform mao)
    {
        escolheu = true;
        escolhaAtual = esc;
        AplicarPega(esc, mao);
        tempoUltimaTroca = Time.time;
    }

    void Trocar(Escolha nova, Transform mao)
    {
        // devolve o objeto atual ao sitio original antes de pegar no novo
        if (escolhaAtual == Escolha.Livro) DevolverAoSitio(livrodequimica, estLivro);
        else if (escolhaAtual == Escolha.Bola) DevolverAoSitio(bolademadeira, estBola);

        escolhaAtual = nova;
        AplicarPega(nova, mao);
        tempoUltimaTroca = Time.time;
    }

    void AplicarPega(Escolha esc, Transform mao)
    {
        if (esc == Escolha.Livro)
            ColaNaMao(livrodequimica, mao, offsetLivro, rotacaoLivro);
        else if (esc == Escolha.Bola)
            ColaNaMao(bolademadeira, mao, offsetBola, rotacaoBola);
    }

    EstadoInicial Capturar(Transform o)
    {
        EstadoInicial e = new EstadoInicial();
        if (o == null) return e;
        e.parent = o.parent;
        e.pos = o.position;
        e.rot = o.rotation;
        Rigidbody rb = o.GetComponent<Rigidbody>();
        if (rb != null) { e.kinematic = rb.isKinematic; e.gravity = rb.useGravity; }
        return e;
    }

    void DevolverAoSitio(Transform o, EstadoInicial e)
    {
        if (o == null || e == null) return;
        o.SetParent(e.parent);
        o.position = e.pos;
        o.rotation = e.rot;
        Rigidbody rb = o.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = e.kinematic;
            rb.useGravity = e.gravity;
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    IEnumerator Vibrar(OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(1f, vibracaoIntensidade, controller);
        yield return new WaitForSeconds(vibracaoDuracao);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }

    IEnumerator ChegouPortalLivro()
    {
        if (portal != null) portal.SetActive(false);

        yield return null;

        SceneManager.LoadScene(cenaFuturo);
    }

    IEnumerator ChegouPortalBola()
    {
        yield return null;
        SceneManager.LoadScene(cenaAlternativa);
    }
}