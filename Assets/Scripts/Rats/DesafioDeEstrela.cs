using System;
using UnityEngine;

public enum TipoDesafioDeEstrela { MoedasColetadas, FloresColetadas, TempoMaximo, ObjetivoSecreto }

[Serializable]
public class DesafioDeEstrela
{
    public TipoDesafioDeEstrela tipo;
    [Min(1)] public int quantidade = 1;
    [Min(1)] public float tempoMaximoSegundos = 60f;

    public bool Concluido(float tempo, int moedas, int flores, bool secreto)
    {
        switch (tipo)
        {
            case TipoDesafioDeEstrela.MoedasColetadas: return moedas >= quantidade;
            case TipoDesafioDeEstrela.FloresColetadas: return flores >= quantidade;
            case TipoDesafioDeEstrela.TempoMaximo: return tempo <= tempoMaximoSegundos;
            case TipoDesafioDeEstrela.ObjetivoSecreto: return secreto;
            default: return false;
        }
    }
}
