namespace GurpsWizard.Data.Entities;

/// <summary>Configuração de campanha (pontos totais, limite de desvantagens).</summary>
public class CampaignEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int PointsTotal { get; set; } = 100;
    public int DisadvLimit { get; set; } = 50;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
