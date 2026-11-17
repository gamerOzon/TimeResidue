using UnityEngine;

public class PeixeInteracao : MonoBehaviour
{
    [Header("Peixe")]
    public Transform peixe;
    public float alturaLevitar = 0.5f;
    public float velocidadeSubir = 2.0f;

    [Header("Wobble")]
    public float wobbleIntensidade = 0.05f;
    public float wobbleVelocidade = 3.0f;

    [Header("Audio")]
    public float tempoSemAudio = 3.0f;
    private AudioSource audioSource;

    [Header("Deteção de Mão")]
    public float distanciaAtivacao = 0.3f;

    // Eventos
    public System.Action onAnimacaoIniciou;
    public System.Action onInteracaoCompleta;

    private enum Estado { Inativo, Subindo, Levitando, Descendo }
    private Estado estadoAtual = Estado.Inativo;

    private Vector3 posicaoInicial;
    private Vector3 posicaoAlvo;
    private bool jaAtivou = false;
    private float timerLevitando = 0f;
    private bool audioDisparado = false;

    private Transform maoEsquerda;
    private Transform maoDireita;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        if (peixe != null)
            posicaoInicial = peixe.position;

        posicaoAlvo = posicaoInicial + Vector3.up * alturaLevitar;

        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;
    }

    void Update()
    {
        if (!jaAtivou)
        {
            bool maoPertoEsq = maoEsquerda != null &&
                Vector3.Distance(maoEsquerda.position, transform.position) < distanciaAtivacao;
            bool maoPertoDir = maoDireita != null &&
                Vector3.Distance(maoDireita.position, transform.position) < distanciaAtivacao;

            if (maoPertoEsq || maoPertoDir)
            {
                jaAtivou = true;
                audioDisparado = false;
                estadoAtual = Estado.Subindo;

                // NOVO — avisa que a animação começou
                onAnimacaoIniciou?.Invoke();
            }
        }

        switch (estadoAtual)
        {
            case Estado.Subindo:
                peixe.position = Vector3.MoveTowards(
                    peixe.position, posicaoAlvo, Time.deltaTime * velocidadeSubir);
                AplicarWobble(posicaoAlvo);
                if (Vector3.Distance(peixe.position, posicaoAlvo) < 0.01f)
                {
                    estadoAtual = Estado.Levitando;
                    timerLevitando = 0f;
                }
                break;

            case Estado.Levitando:
                AplicarWobble(posicaoAlvo);
                timerLevitando += Time.deltaTime;

                // Espera o áudio do NarratorManager terminar
                bool audioTerminou = !audioDisparado ||
                    timerLevitando >= tempoSemAudio;

                if (audioTerminou)
                    estadoAtual = Estado.Descendo;
                break;

            case Estado.Descendo:
                peixe.position = Vector3.MoveTowards(
                    peixe.position, posicaoInicial, Time.deltaTime * velocidadeSubir);
                AplicarWobble(posicaoInicial);
                if (Vector3.Distance(peixe.position, posicaoInicial) < 0.01f)
                {
                    peixe.position = posicaoInicial;
                    estadoAtual = Estado.Inativo;
                    jaAtivou = false;

                    // Avisa que terminou
                    onInteracaoCompleta?.Invoke();
                }
                break;
        }
    }

    void AplicarWobble(Vector3 posicaoBase)
    {
        float wobbleX = Mathf.Sin(Time.time * wobbleVelocidade) * wobbleIntensidade;
        float wobbleZ = Mathf.Cos(Time.time * wobbleVelocidade * 0.7f) * wobbleIntensidade;
        peixe.position = new Vector3(
            posicaoBase.x + wobbleX,
            peixe.position.y,
            posicaoBase.z + wobbleZ);
    }
}