using UnityEngine;

/// <summary>
/// Faz um objeto de info (texto 3D) aparecer sempre no campo de visao do utilizador,
/// preso de forma suave a cabeca (CenterEyeAnchor). Ativa-se sozinho quando o
/// GameObject e ligado (SetActive(true)) pelo NarratorManager. Nao altera nada da
/// logica de interacao — so reposiciona o painel para a frente dos olhos.
/// </summary>
public class InfoSeguirCamera : MonoBehaviour
{
    [Header("Cabeca (auto: CenterEyeAnchor)")]
    public Transform cabeca;
    public string nomeCabeca = "CenterEyeAnchor";

    [Header("Posicionamento")]
    [Tooltip("Distancia a frente dos olhos (metros).")]
    public float distancia = 1.2f;
    [Tooltip("Ajuste vertical relativo a altura dos olhos (metros). Negativo = mais abaixo.")]
    public float alturaOffset = 0f;

    [Header("Seguimento")]
    [Tooltip("Suavidade do seguir. 0 = travado rigido a cabeca. Maior = mais suave/preguicoso.")]
    public float suavidade = 6f;
    [Tooltip("Posiciona imediatamente a frente ao ser ativado (sem deslizar do sitio antigo).")]
    public bool snapAoAtivar = true;

    [Header("Orientacao")]
    [Tooltip("Offset de rotacao em Y. Este texto foi autorado a 180; mantem 180 para nao inverter.")]
    public float offsetYaw = 180f;

    void OnEnable()
    {
        GarantirCabeca();
        if (snapAoAtivar && cabeca != null)
        {
            transform.position = PosicaoAlvo();
            transform.rotation = RotacaoAlvo();
        }
    }

    void LateUpdate()
    {
        if (cabeca == null)
        {
            GarantirCabeca();
            if (cabeca == null) return;
        }

        float t = (suavidade <= 0f) ? 1f : 1f - Mathf.Exp(-suavidade * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, PosicaoAlvo(), t);
        transform.rotation = Quaternion.Slerp(transform.rotation, RotacaoAlvo(), t);
    }

    void GarantirCabeca()
    {
        if (cabeca != null) return;
        GameObject c = GameObject.Find(nomeCabeca);
        if (c != null) cabeca = c.transform;
        else if (Camera.main != null) cabeca = Camera.main.transform;
    }

    Vector3 PosicaoAlvo()
    {
        Vector3 fwd = cabeca.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();
        return cabeca.position + fwd * distancia + Vector3.up * alturaOffset;
    }

    Quaternion RotacaoAlvo()
    {
        Vector3 dir = transform.position - cabeca.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = cabeca.forward;
        return Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(0f, offsetYaw, 0f);
    }
}
