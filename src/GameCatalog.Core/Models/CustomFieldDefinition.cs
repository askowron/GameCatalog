namespace GameCatalog.Core.Models;

/// <summary>Definicja pola własnego (odpowiednik AMC TCustomFieldProperties).</summary>
public sealed class CustomFieldDefinition
{
    public string Tag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FieldType { get; set; } = "Text"; // Text, Number, Date, List, Url, YesNo
    public string ListValues { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
}
