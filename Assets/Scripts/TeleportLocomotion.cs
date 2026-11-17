using UnityEngine;

public class TeleportLocomotion : MonoBehaviour
{
    [Header("Chão (pai da área válida de teleporte)")]
    public Transform chao;

    [Header("Visual")]
    public Color corValida = Color.green;
    public Color corInvalida = Color.red;
    public float raioIndicador = 0.15f;

    [Header("Arco")]
    public float forcaArco = 5f;
    public float gravidade = 9.8f;
    public int resolucaoArco = 30;
    public float distanciaMaxima = 10f;

    [Header("Input")]
    public float limiarThumbstick = 0.7f;

    [Header("Feedback")]
    public float vibracaoDuracao = 0.15f;
    public float vibracaoIntensidade = 0.3f;

    private Transform maoEsquerda;
    private Transform maoDireita;
    private OVRPlayerController ovrPC;
    private CharacterController charController;

    private LineRenderer lineRenderer;
    private GameObject indicador;
    private MeshRenderer indicadorRenderer;

    private bool apontando = false;
    private Vector3 pontoTeleporte;
    private bool pontoValido = false;
    private bool usandoMaoEsquerda = false;

    void Start()
    {
        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;

        // Encontrar o OVRPlayerController e desativar movimento por thumbstick
        ovrPC = FindObjectOfType<OVRPlayerController>();
        if (ovrPC != null)
        {
            ovrPC.EnableLinearMovement = false;
            ovrPC.EnableRotation = false;
            charController = ovrPC.GetComponent<CharacterController>();
            Debug.Log("[Teleport] OVRPlayerController encontrado, movimento desativado.");
        }
        else
        {
            Debug.LogWarning("[Teleport] OVRPlayerController NÃO encontrado!");
        }

        // Criar LineRenderer para o arco
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        // Criar indicador de destino (disco no chão)
        indicador = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicador.name = "TeleportIndicator";
        indicador.transform.localScale = new Vector3(raioIndicador * 2f, 0.01f, raioIndicador * 2f);
        Object.Destroy(indicador.GetComponent<Collider>());
        indicadorRenderer = indicador.GetComponent<MeshRenderer>();
        indicadorRenderer.material = new Material(Shader.Find("Sprites/Default"));
        indicador.SetActive(false);
    }

    void Update()
    {
        if (ovrPC == null) return;

        // Forçar desativação do movimento normal todos os frames
        ovrPC.EnableLinearMovement = false;
        ovrPC.EnableRotation = false;

        // Verificar thumbstick forward (empurrar para frente)
        Vector2 thumbEsq = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector2 thumbDir = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        bool apontarEsq = thumbEsq.y > limiarThumbstick && maoEsquerda != null;
        bool apontarDir = thumbDir.y > limiarThumbstick && maoDireita != null;

        if (apontarEsq || apontarDir)
        {
            apontando = true;
            usandoMaoEsquerda = apontarEsq;
            Transform mao = usandoMaoEsquerda ? maoEsquerda : maoDireita;
            CalcularArco(mao);
            MostrarArco();
        }
        else if (apontando)
        {
            // Thumbstick largado — teleportar se válido
            apontando = false;
            lineRenderer.enabled = false;
            indicador.SetActive(false);

            if (pontoValido)
            {
                Teleportar();
            }
        }
    }

    void CalcularArco(Transform mao)
    {
        pontoValido = false;

        Vector3 origem = mao.position;
        Vector3 velocidade = mao.forward * forcaArco;

        lineRenderer.positionCount = resolucaoArco;
        float passo = distanciaMaxima / (resolucaoArco * forcaArco);

        for (int i = 0; i < resolucaoArco; i++)
        {
            float t = i * passo;
            Vector3 ponto = origem + velocidade * t + 0.5f * Vector3.down * gravidade * t * t;
            lineRenderer.SetPosition(i, ponto);

            // Verificar colisão com o chão entre este ponto e o anterior
            if (i > 0)
            {
                Vector3 anterior = lineRenderer.GetPosition(i - 1);
                RaycastHit hit;
                if (Physics.Linecast(anterior, ponto, out hit))
                {
                    // Verificar se bateu no Chão ou num filho do Chão
                    if (chao != null && hit.collider.transform.IsChildOf(chao))
                    {
                        pontoTeleporte = hit.point;
                        pontoValido = true;
                    }

                    // Cortar o arco no ponto de impacto
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    break;
                }
            }
        }
    }

    void MostrarArco()
    {
        lineRenderer.enabled = true;
        Color cor = pontoValido ? corValida : corInvalida;
        lineRenderer.startColor = cor;
        lineRenderer.endColor = cor;

        if (pontoValido)
        {
            indicador.SetActive(true);
            indicador.transform.position = pontoTeleporte + Vector3.up * 0.01f;
            indicadorRenderer.material.color = corValida;
        }
        else
        {
            indicador.SetActive(false);
        }
    }

    void Teleportar()
    {
        // Manter a mesma rotação, só mover a posição
        // Offset entre cabeça e player controller (para manter a posição relativa)
        GameObject cab = GameObject.Find("CenterEyeAnchor");

        // Desativar CharacterController temporariamente para poder mover por transform
        if (charController != null) charController.enabled = false;

        if (cab != null)
        {
            Vector3 offset = ovrPC.transform.position - cab.transform.position;
            offset.y = 0f; // Só offset horizontal
            ovrPC.transform.position = pontoTeleporte + offset;
        }
        else
        {
            ovrPC.transform.position = pontoTeleporte;
        }

        if (charController != null) charController.enabled = true;

        // Vibração de feedback
        OVRInput.Controller controller = usandoMaoEsquerda
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;
        StartCoroutine(Vibrar(controller));
    }

    System.Collections.IEnumerator Vibrar(OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(1f, vibracaoIntensidade, controller);
        yield return new WaitForSeconds(vibracaoDuracao);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }

    void OnDestroy()
    {
        if (indicador != null)
            Object.Destroy(indicador);
    }
}
