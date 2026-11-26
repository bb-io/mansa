using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Mansa.Handlers.Static;

public class LanguageDataHandler : IStaticDataSourceItemHandler
{
    public static Dictionary<string, string> Data => new()
    {
        { "Swahili", "Swahili (East Africa)" },
        { "Arabic", "Arabic (North Africa)" },
        { "Afrikaans", "Afrikaans (South Africa)" },
        { "Amharic", "Amharic (Ethiopia)" },
        { "Hausa", "Hausa (West Africa)" },
        { "Igbo", "Igbo (Nigeria)" },
        { "Somali", "Somali (Horn of Africa)" },
        { "Xhosa", "Xhosa (South Africa)" },
        { "Zulu", "Zulu (South Africa)" },
        { "Yoruba", "Yoruba (Nigeria)" },
        { "Kinyarwanda", "Kinyarwanda (Rwanda)" },
        { "Shona", "Shona (Zimbabwe)" },
        { "Tswana", "Tswana (Botswana)" },
        { "Chichewa", "Chichewa (Malawi)" },
        { "Luganda", "Luganda (Uganda)" },
        { "Malagasy", "Malagasy (Madagascar)" },
        { "Oromo", "Oromo (Ethiopia)" },
        { "Sesotho", "Sesotho (Lesotho)" },
        { "Sepedi", "Sepedi (South Africa)" },
        { "Tsonga", "Tsonga (South Africa)" },
        { "English", "English" },
        { "French", "French" },
        { "Spanish", "Spanish" },
        { "Portuguese", "Portuguese" },
        { "Kirundi", "Kirundi (Beta)" },
        { "Ndebele (South)", "Ndebele (South) (Beta)" },
        { "Swati", "Swati (Beta)" },
        { "Tigrinya", "Tigrinya (Beta)" },
        { "Twi", "Twi (Beta)" },
        { "Ewe", "Ewe (Beta)" },
        { "Lingala", "Lingala (Beta)" },
        { "Kikongo", "Kikongo (Beta)" },
        { "Bambara", "Bambara (Beta)" },
        { "Bemba", "Bemba (Beta)" },
        { "Dagbani", "Dagbani (Beta)" }
    };

    public IEnumerable<DataSourceItem> GetData()
    {
        return Data.Select(x => new DataSourceItem(x.Key, x.Value));
    }
}