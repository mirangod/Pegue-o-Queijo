using UnityEngine;
using System;
using System.Collections.Generic;

public enum CriterioDeDesbloqueioTipo { SempreLiberada, EstrelasTotais, MoedasTotais, FloresTotais, FaseConcluida, EstrelasDaFase }

[Serializable]
public class CriterioDeDesbloqueio
{
    public CriterioDeDesbloqueioTipo tipo;
    public string idDaFase;
    [Min(1)] public int quantidade = 1;
}

/// <summary>Persistência simples de conclusão e estrelas por ID de fase.</summary>
public static class ProgressaoDeFases
{
    private static string Key(string id) => "fase." + id;
    private const string TotalEstrelasKey = "recursos.estrelas";
    private const string TotalMoedasKey = "recursos.moedas";
    private const string TotalFloresKey = "recursos.flores";

    public static int Estrelas(string id) => string.IsNullOrWhiteSpace(id) ? 0 : PlayerPrefs.GetInt(Key(id), 0);
    public static bool Concluida(string id) => Estrelas(id) > 0;
    public static int TotalEstrelas => PlayerPrefs.GetInt(TotalEstrelasKey, 0);
    public static int TotalMoedas => PlayerPrefs.GetInt(TotalMoedasKey, 0);
    public static int TotalFlores => PlayerPrefs.GetInt(TotalFloresKey, 0);

    public static void RegistrarRecurso(RecursoColetavel tipo, int quantidade)
    {
        if (quantidade <= 0) return;
        string key = tipo == RecursoColetavel.Moeda ? TotalMoedasKey : TotalFloresKey;
        PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + quantidade);
        PlayerPrefs.Save();
    }

    public static void RegistrarConclusao(string id, int estrelas)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        int melhorResultado = Mathf.Clamp(Mathf.Max(Estrelas(id), estrelas), 0, 3);
        PlayerPrefs.SetInt(TotalEstrelasKey, Mathf.Max(0, TotalEstrelas + melhorResultado - Estrelas(id)));
        PlayerPrefs.SetInt(Key(id), melhorResultado);
        PlayerPrefs.Save();
    }

    public static bool Avaliar(CriterioDeDesbloqueio criterio)
    {
        switch (criterio.tipo)
        {
            case CriterioDeDesbloqueioTipo.SempreLiberada: return true;
            case CriterioDeDesbloqueioTipo.EstrelasTotais: return TotalEstrelas >= criterio.quantidade;
            case CriterioDeDesbloqueioTipo.MoedasTotais: return TotalMoedas >= criterio.quantidade;
            case CriterioDeDesbloqueioTipo.FloresTotais: return TotalFlores >= criterio.quantidade;
            case CriterioDeDesbloqueioTipo.FaseConcluida: return Concluida(criterio.idDaFase);
            case CriterioDeDesbloqueioTipo.EstrelasDaFase: return Estrelas(criterio.idDaFase) >= criterio.quantidade;
            default: return false;
        }
    }

    public static bool AvaliarTodos(IReadOnlyList<CriterioDeDesbloqueio> criterios, bool exigirTodos)
    {
        if (criterios == null || criterios.Count == 0 || criterios.Count > 3) return false;
        foreach (CriterioDeDesbloqueio criterio in criterios)
        {
            bool passou = Avaliar(criterio);
            if (exigirTodos && !passou) return false;
            if (!exigirTodos && passou) return true;
        }
        return exigirTodos;
    }

}
