using UnityEngine;

/// <summary>
/// Guarda o idioma escolhido (Instructions) e mantem-no entre cenas
/// durante a sessao. static => sobrevive a troca de cena.
/// false = Ingles (default), true = Portugues.
/// </summary>
public static class LanguageManager
{
    public static bool Portugues = false;

    public static void SetPortugues() { Portugues = true; }
    public static void SetIngles()   { Portugues = false; }
}
