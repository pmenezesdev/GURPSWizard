namespace GurpsWizard.App;

/// <summary>
/// Dicionário de sinônimos para a busca GURPS-PTBR.
/// Quando o usuário digita um termo conhecido, a query é expandida para
/// incluir também o nome canônico da biblioteca GCS-PTBR.
/// </summary>
public static class SearchSynonyms
{
    private static readonly Dictionary<string, string> Map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Vantagens / combat
            ["combate"]              = "Reflexos em Combate",
            ["sentido de combate"]   = "Reflexos em Combate",
            ["reflexo"]              = "Reflexos em Combate",
            ["reflexos"]             = "Reflexos em Combate",

            // Atributos (usuário pode digitar o nome por extenso)
            ["força"]                = "ST",
            ["destreza"]             = "DX",
            ["inteligência"]         = "IQ",
            ["inteligencia"]         = "IQ",
            ["saúde"]                = "HT",
            ["saude"]                = "HT",

            // Sentidos
            ["visão"]                = "Visão",
            ["visao"]                = "Visão",
            ["audição"]              = "Audição",
            ["audicao"]              = "Audição",
            ["tato"]                 = "Tato",
            ["olfato"]               = "Olfato/Paladar",
            ["paladar"]              = "Olfato/Paladar",
            ["olfato e paladar"]     = "Olfato/Paladar",

            // Outros termos comuns
            ["telepatia"]            = "Telepatia",
            ["riqueza"]              = "Riqueza",
            ["fobia"]                = "Fobia",
            ["dependencia"]          = "Dependência",
            ["dependência"]          = "Dependência",
            ["vício"]                = "Vício",
            ["vicio"]                = "Vício",
        };

    /// <summary>
    /// Retorna o termo canônico se a query bater exatamente com um sinônimo;
    /// caso contrário, retorna a query original.
    /// </summary>
    public static string Expand(string query)
    {
        var trimmed = query.Trim();
        return Map.TryGetValue(trimmed, out var canonical) ? canonical : trimmed;
    }
}
