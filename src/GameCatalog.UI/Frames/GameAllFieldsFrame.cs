using GameCatalog.Core.Fields;
using GameCatalog.Core.Models;

namespace GameCatalog.UI.Frames;

/// <summary>Read-only list of every field of the current game ("All fields" tab).</summary>
public partial class GameAllFieldsFrame : UserControl
{
    public GameAllFieldsFrame() => InitializeComponent();

    public void Bind(Game? game, IReadOnlyList<CustomFieldDefinition> customFields)
    {
        gridFields.Rows.Clear();
        if (game is null) return;
        foreach (var f in GameFields.All)
            gridFields.Rows.Add(CatalogLabels.Current.Field(f.Tag), GameFieldValues.Get(game, f.Tag));
        foreach (var d in customFields)
            gridFields.Rows.Add(d.Name, game.CustomValues.TryGetValue(d.Tag, out var v) ? v : "");
    }
}
