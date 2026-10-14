using GameCatalog.App.Services;

namespace GameCatalog.App.Diagnostics;

/// <summary>Logs an unexpected exception and tells the user where the details are.</summary>
public static class ErrorReporter
{
    public static void Show(Exception ex, IWin32Window? owner = null)
    {
        AppLog.Error(ex, "Unhandled UI exception");
        var t = AppServices.Languages;
        MessageBox.Show(owner,
            t.Get("Msg.UnexpectedError", ex.Message, AppLog.FilePath ?? ""),
            t.Get("Common.AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
