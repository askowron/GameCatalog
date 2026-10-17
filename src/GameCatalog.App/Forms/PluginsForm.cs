using GameCatalog.App.Services;
using GameCatalog.Plugins.Hosting;

namespace GameCatalog.App.Forms;

/// <summary>Tools → Plugins: installed plugins with their state; check boxes enable/disable them.</summary>
public partial class PluginsForm : Form
{
    public PluginsForm(IReadOnlyList<PluginDescriptor> plugins, IReadOnlyCollection<string> disabledIds)
    {
        InitializeComponent();
        var t = AppServices.Languages;
        foreach (var p in plugins)
        {
            var item = new ListViewItem(p.Name) { Tag = p.Id, Checked = !disabledIds.Contains(p.Id) };
            item.SubItems.Add(p.Version);
            item.SubItems.Add(p.Author);
            item.SubItems.Add(t.Get("Plugin.State." + p.State));
            item.SubItems.Add(p.Error ?? "");
            lvPlugins.Items.Add(item);
        }
    }

    public IEnumerable<string> DisabledIds =>
        lvPlugins.Items.Cast<ListViewItem>().Where(i => !i.Checked).Select(i => (string)i.Tag!);
}
