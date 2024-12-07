namespace ProjetoOdontoPrev.Models;

public class Clinica
{
    public int ID_Cliente { get; set; }
    public string? Cliente { get; set; }
    public string? CEP_Cliente { get; set; }
    public int ID_Clinica { get; set; }
    public string? NomeClinica { get; set; }
    public string? CEP_Clinica { get; set; }
    public double Distancia { get; set; }
    public double DistanciaReal { get; set; }
    public double NotaPesquisa { get; set; }
    public double ValorConsulta { get; set; }
    public double DistanciaNormalizada { get; set; }
    public double NotaNormalizada { get; set; }
    public double ValorNormalizado { get; set; }
    public double Pontuacao { get; set; }
    public int RankNovo { get; set; }
    public string? StatusClinica { get; set; }
    public DateTime DataAcao { get; set; }
    public string? HoraAcao { get; set; }
}
