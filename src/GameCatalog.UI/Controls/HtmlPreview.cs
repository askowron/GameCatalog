using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

    public HtmlPreview()
    {
        InitializeComponent();
        lnkInstall.LinkClicked += (_, _) => OpenExternal(RuntimeDownloadUrl);
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
        if (!IsAvailable || webView.CoreWebView2 is null) return;
        if (html.Length < NavigateToStringLimit)
        {
            webView.NavigateToString(html);
        }
        else
        {
            var file = Path.Combine(Path.GetTempPath(), "gamecatalog_preview.html");
            await File.WriteAllTextAsync(file, html);
            webView.CoreWebView2.Navigate(new Uri(file).AbsoluteUri);
        }
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
        catch (Exception ex) when (ex is WebView2RuntimeNotFoundException or COMException or InvalidOperationException)
        {
            IsAvailable = false;
            webView.Visible = false;
            pnlMissing.Visible = true;
        }
    }

    private static void OpenExternal(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Win32Exception) { /* no browser registered */ }
    }
}
