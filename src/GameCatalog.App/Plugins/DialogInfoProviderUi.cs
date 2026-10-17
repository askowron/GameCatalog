using GameCatalog.Plugins.Abstractions;

namespace GameCatalog.App.Plugins;

/// <summary>Info-provider interaction implemented with the application's dialogs.</summary>
internal sealed class DialogInfoProviderUi(Func<string, IList<string>, int> pick, Action<string> message) : IInfoProviderUi
{
    public int PickList(string title, IList<string> options) => pick(title, options);
    public void ShowMessage(string text) => message(text);
}
