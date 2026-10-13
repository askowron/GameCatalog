namespace GameCatalog.App;

/// <summary>Wspólne układanie kontrolek — WinForms dokuje w odwrotnej kolejności
/// dodawania, więc odwracamy kolejność w każdym Panelu, by wyświetlanie
/// zgadzało się z kolejnością budowania (TableLayoutPanel pomijamy).</summary>
internal static class ControlLayout
{
    internal static void FixDockOrder(Control parent)
    {
        var kids = parent.Controls.Cast<Control>().ToList();
        if (parent is Panel)
            foreach (var kid in kids)
                parent.Controls.SetChildIndex(kid, 0);
        foreach (var kid in kids)
            FixDockOrder(kid);
    }
}
