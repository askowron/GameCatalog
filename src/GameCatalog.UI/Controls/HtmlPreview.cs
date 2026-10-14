using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Web.WebView2.Core;

namespace GameCatalog.UI.Controls;

/// <summary>
/// HTML preview of the current game (AMC HTMLViewer) on Microsoft Edge WebView2.
/// When the WebView2 runtime is missing a message with a download link is shown instead.
/// </summary>
public partial class HtmlPreview : UserControl
{
    private const string RuntimeDownloadUrl = "https://developer.microsoft.com/microsoft-edge/webview2/";
    private const int NavigateToStringLimit = 1_500_000;
    private Task? _initialization;
    private string? _previewFile;

    public HtmlPreview()
    {
        InitializeComponent();
        lnkInstall.LinkClicked += (_, _) => OpenExternal(RuntimeDownloadUrl);
        Disposed += (_, _) => DeletePreviewFile();
    }

    /// <summary>WebView2 profile folder; must be writable (not next to the exe in Program Files).</summary>
    public static string UserDataFolder { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameCatalog", "WebView2");

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsAvailable { get; private set; } = true;

    public async Task ShowHtmlAsync(string html)
    {
        if (DesignMode) return;   // never start WebView2 inside the Visual Studio designer
        await (_initialization ??= InitializeAsync());
        if (IsDisposed || !IsAvailable || webView.CoreWebView2 is null) return;
        if (html.Length >= NavigateToStringLimit)
        {
            // A unique file per call: a pending navigation never reads a file that is being rewritten.
            var file = Path.Combine(Path.GetTempPath(), "gamecatalog_preview_" + Path.GetRandomFileName() + ".html");
            await File.WriteAllTextAsync(file, html);
            if (IsDisposed || webView.CoreWebView2 is null)
            {
                TryDelete(file);
                return;
            }
            DeletePreviewFile();
            _previewFile = file;
            await NavigateAndWaitAsync(() => webView.CoreWebView2.Navigate(new Uri(file).AbsoluteUri));
        }
        else
        {
            await NavigateAndWaitAsync(() => webView.NavigateToString(html));
        }
    }

    /// <summary>
    /// Starts a navigation and completes when a navigation finished (successfully or with an error), so
    /// printing right after showing prints the new page. Completions of navigations cancelled by a newer
    /// one are ignored: a superseded call completes together with the navigation that replaced it.
    /// </summary>
    private Task NavigateAndWaitAsync(Action navigate)
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Finish()
        {
            webView.NavigationCompleted -= OnCompleted;
            Disposed -= OnDisposed;
            completed.TrySetResult();
        }
        void OnCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess && e.WebErrorStatus == CoreWebView2WebErrorStatus.OperationCanceled) return;
            Finish();
        }
        void OnDisposed(object? sender, EventArgs e) => Finish();   // never leave the caller waiting
        webView.NavigationCompleted += OnCompleted;
        Disposed += OnDisposed;
        try
        {
            navigate();
        }
        catch
        {
            Finish();
            throw;
        }
        return completed.Task;
    }

    private void DeletePreviewFile()
    {
        if (_previewFile is null) return;
        TryDelete(_previewFile);
        _previewFile = null;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { /* temp file: best effort */ }
    }

    public void Print()
    {
        if (IsAvailable && webView.CoreWebView2 is not null)
            webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);
    }

    private async Task InitializeAsync()
    {
        try
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, UserDataFolder);
            await webView.EnsureCoreWebView2Async(environment);
            webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView.CoreWebView2.NewWindowRequested += (_, e) =>
            {
                e.Handled = true;
                OpenExternal(e.Uri);
            };
            webView.CoreWebView2.NavigationStarting += (_, e) =>
            {
                if (e.IsUserInitiated && e.Uri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    e.Cancel = true;
                    OpenExternal(e.Uri);
                }
            };
        }
        catch (Exception ex)
        {
            // Any failure of the WebView2 runtime or profile (missing runtime, locked or read-only profile
            // folder, COM errors) must not break the host window: fall back to the message panel. The
            // exception is swallowed here, so the cached initialization task never stays faulted.
            IsAvailable = false;
            if (!IsDisposed)
            {
                webView.Visible = false;
                pnlMissing.Visible = true;
                if (ex is not WebView2RuntimeNotFoundException)
                    lblMissing.Text += Environment.NewLine + ex.Message;
            }
        }
    }

    private static void OpenExternal(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Win32Exception) { /* no browser registered */ }
    }
}
