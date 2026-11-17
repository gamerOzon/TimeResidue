using UnityEngine;

public class MoneyPrizeInteracao : MonoBehaviour
{
    [Header("Objetos")]
    public Transform poster;
    public Transform moneyNote1;
    public Transform moneyNote2;

    [Header("Duração do Áudio")]
    public float duracaoAudio = 10f;

    [Header("Velocidades")]
    public float velocidadeBalanco = 2f;
    public float velocidadeRotacao = 180f;

    [Header("Deteção de Mão")]
    public float distanciaAtivacao = 0.3f;

    public System.Action onAnimacaoIniciou;
    public System.Action onInteracaoCompleta;

    private Transform maoEsquerda;
    private Transform maoDireita;
    private bool jaAtivou = false;
    private bool emAnimacao = false;
    private float tempoAnimacaoIniciou = 0f;

    private Quaternion rotacaoInicialPoster;
    private Quaternion rotacaoInicialNote1;
    private Quaternion rotacaoInicialNote2;

    void Start()
    {
        if (poster != null) rotacaoInicialPoster = poster.rotation;
        if (moneyNote1 != null) rotacaoInicialNote1 = moneyNote1.rotation;
        if (moneyNote2 != null) rotacaoInicialNote2 = moneyNote2.rotation;

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
                emAnimacao = true;
                tempoAnimacaoIniciou = Time.time;
                onAnimacaoIniciou?.Invoke();
                StartCoroutine(Sequencia());
            }
        }

        if (emAnimacao)
        {
            // Poster balança no eixo Z entre -2.4 e 2.4
            if (poster != null)
            {
                float angulo = Mathf.Sin(Time.time * velocidadeBalanco) * 2.4f;
                poster.rotation = rotacaoInicialPoster * Quaternion.Euler(0f, 0f, angulo);
            }

            // Money notes rodam 360 no eixo X
        if (moneyNote1 != null)
            moneyNote1.Rotate(velocidadeRotacao * Time.deltaTime, 0f, 0f, Space.World);
        if (moneyNote2 != null)
            moneyNote2.Rotate(velocidadeRotacao * Time.deltaTime, 0f, 0f, Space.World);
                }
    }

    System.Collections.IEnumerator Sequencia()
    {
        // Espera até o áudio terminar
        float tempoDecorrido = Time.time - tempoAnimacaoIniciou;
        float tempoRestante = duracaoAudio - tempoDecorrido;
        if (tempoRestante > 0f)
            yield return new WaitForSeconds(tempoRestante);

        // Para animações e volta ao estado inicial
        emAnimacao = false;

        if (poster != null) poster.rotation = rotacaoInicialPoster;
        if (moneyNote1 != null) moneyNote1.rotation = rotacaoInicialNote1;
        if (moneyNote2 != null) moneyNote2.rotation = rotacaoInicialNote2;

        jaAtivou = false;
        onInteracaoCompleta?.Invoke();
    }
} 