using UnityEngine;

public class PoolBallsInteracao : MonoBehaviour
{
    [Header("Bolas")]
    public Transform bola1; // SM_Prop_PoolBall_04
    public Transform bola2; // SM_Prop_PoolBall_03
    public Transform bola3; // SM_Prop_PoolBall_01

    [Header("Duração do Áudio")]
    public float duracaoAudio = 10f;

    [Header("Deteção de Mão")]
    public float distanciaAtivacao = 0.3f;

    public System.Action onAnimacaoIniciou;
    public System.Action onInteracaoCompleta;

    private Transform maoEsquerda;
    private Transform maoDireita;
    private bool jaAtivou = false;
    private float tempoAnimacaoIniciou = 0f;

    // Posições Bola1 (PoolBall_04)
    private readonly Vector3 inicialBola1 = new Vector3(-1.4708004f, 0.723699987f, 1.16000366f);
    private readonly Vector3 mediaBola1   = new Vector3(-1.1135f,    0.740700006f, 1.08329999f);
    private readonly Vector3 finalBola1   = new Vector3(-1.1006f,    0.627900004f, 1.07309997f);

    // Posições Bola2 (PoolBall_03)
    private readonly Vector3 inicialBola2 = new Vector3(-1.36899948f, 0.723699987f, 1.04000092f);
    private readonly Vector3 mediaBola2   = new Vector3(-1.11619997f, 0.728399992f, 1.06270003f);
    private readonly Vector3 finalBola2   = new Vector3(-1.11619997f, 0.602500021f, 1.06270003f);

    // Posições Bola3 (PoolBall_01)
    private readonly Vector3 inicialBola3 = new Vector3(-1.37700081f, 0.723699987f, 1.26000226f);
    private readonly Vector3 mediaBola3   = new Vector3(-1.11409998f, 0.735800028f, 1.09389997f);
    private readonly Vector3 finalBola3   = new Vector3(-1.12100005f, 0.600700021f, 1.10080004f);

    void Start()
    {
        if (bola1 != null) bola1.position = inicialBola1;
        if (bola2 != null) bola2.position = inicialBola2;
        if (bola3 != null) bola3.position = inicialBola3;

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
                tempoAnimacaoIniciou = Time.time;
                onAnimacaoIniciou?.Invoke();
                StartCoroutine(Sequencia());
            }
        }
    }

    System.Collections.IEnumerator Sequencia()
        {
            float velocidade = 1.5f;

            // Avançar em cascata
            StartCoroutine(MoverPara(bola1, mediaBola1, velocidade));
            yield return new WaitUntil(() => Vector3.Distance(bola1.position, mediaBola1) < 0.05f);

            StartCoroutine(MoverPara(bola1, finalBola1, velocidade));
            StartCoroutine(MoverPara(bola2, mediaBola2, velocidade));
            yield return new WaitUntil(() => Vector3.Distance(bola2.position, mediaBola2) < 0.05f);

            StartCoroutine(MoverPara(bola2, finalBola2, velocidade));
            StartCoroutine(MoverPara(bola3, mediaBola3, velocidade));
            yield return new WaitUntil(() => Vector3.Distance(bola3.position, mediaBola3) < 0.05f);

            StartCoroutine(MoverPara(bola3, finalBola3, velocidade));
            yield return new WaitUntil(() => Vector3.Distance(bola3.position, finalBola3) < 0.05f);

            // Espera até o áudio terminar
            float tempoDecorrido = Time.time - tempoAnimacaoIniciou;
            float tempoRestante = duracaoAudio - tempoDecorrido;
            if (tempoRestante > 0f)
                yield return new WaitForSeconds(tempoRestante);

            // Recuar em cascata
            StartCoroutine(MoverPara(bola3, mediaBola3, velocidade));
            yield return new WaitUntil(() => Vector3.Distance(bola3.position, mediaBola3) < 0.05f);

            StartCoroutine(MoverPara(bola3, inicialBola3, velocidade));
            StartCoroutine(MoverPara(bola2, mediaBola2, velocidade));
            yield return new WaitUntil(() => Vector3.Distance(bola2.position, mediaBola2) < 0.05f);

            StartCoroutine(MoverPara(bola2, inicialBola2, velocidade));
            StartCoroutine(MoverPara(bola1, mediaBola1, velocidade));
            yield return new WaitUntil(() => Vector3.Distance(bola1.position, mediaBola1) < 0.05f);

            StartCoroutine(MoverPara(bola1, inicialBola1, velocidade));
            yield return new WaitUntil(() => Vector3.Distance(bola1.position, inicialBola1) < 0.05f);

            jaAtivou = false;
            onInteracaoCompleta?.Invoke();
        }

    System.Collections.IEnumerator MoverPara(Transform obj, Vector3 destino, float velocidade)
    {
        if (obj == null) yield break;
        while (Vector3.Distance(obj.position, destino) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, destino, velocidade * Time.deltaTime);
            yield return null;
        }
        obj.position = destino;
    }
}