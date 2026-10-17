using GameCatalog.App.Diagnostics;
using GameCatalog.App.Services;
using GameCatalog.Core.Fields;
using GameCatalog.Localization;
using GameCatalog.Scripting;

namespace GameCatalog.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        AppLog.Initialize(AppPaths.LogsDirectory);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ErrorReporter.Show(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            AppLog.Error((Exception)e.ExceptionObject, "Unhandled domain exception");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            AppLog.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        AppServices.Prefs = PrefsStore.Load();
        AppServices.Languages = new LanguageManager(AppPaths.LanguagesDirectory, LanguageManager.DefaultLanguageCode, AppLog.Write);
        // An empty preference follows the system language (when a translation exists).
        AppServices.Languages.SetLanguage(StartupLanguage.Resolve(AppServices.Prefs.Language,
            AppServices.Languages.Available, System.Globalization.CultureInfo.CurrentUICulture, LanguageManager.DefaultLanguageCode));
        WireTranslations(AppServices.Languages);
        GameCatalog.UI.Components.Localizer.Translator = AppServices.Languages;

        Application.Run(new MainForm());
    }

    /// <summary>Points the UI-free layers at the active language.</summary>
    private static void WireTranslations(ITranslator t)
    {
        CatalogLabels.Current = new TranslatorCatalogLabels(t);
        ScriptMessages.CompileErrors = details => t.Get("Script.CompileErrors", details);
        ScriptMessages.CompileErrorLine = (line, msg) => t.Get("Script.CompileErrorLine", line, msg);
        ScriptMessages.RuntimeError = msg => t.Get("Script.RuntimeError", msg);
        ScriptMessages.Timeout = seconds => t.Get("Script.Timeout", seconds);
        ScriptMessages.Error = msg => t.Get("Script.Error", msg);
        ScriptMessages.NoData = t.Get("Script.NoData");
        t.LanguageChanged += (_, _) => ScriptMessages.NoData = t.Get("Script.NoData");
    }
}
