using GameCatalog.App.Services;
using GameCatalog.UI.Components;
using GameCatalog.UI.Icons;

namespace GameCatalog.App.Forms;

/// <summary>Game number dialog with duplicate resolution (AMC "Movie number" dialog).</summary>
public partial class NumberConflictDialog : Form
{
    private readonly Func<int, bool>? _isDuplicate;
    private readonly Func<int, string?>? _describe;

    public int ChosenNumber { get; private set; }
    public NumberConflictResolution Resolution { get; private set; } = NumberConflictResolution.ShiftNew;
    public bool DontAskAgain => chkDontAskAgain.Checked;

    public NumberConflictDialog(int proposedNumber, int maxNumber, Func<int, bool>? isDuplicate = null, Func<int, string?>? describe = null)
    {
        InitializeComponent();
        _isDuplicate = isDuplicate;
        _describe = describe;
        ChosenNumber = proposedNumber;
        LoadIcons();

        numNumber.Maximum = Math.Max(Math.Max(1, maxNumber), proposedNumber + 1000);
        numNumber.Value = Math.Clamp(proposedNumber, 1, int.MaxValue);
        RefreshConflict();

        numNumber.ValueChanged += (_, _) => RefreshConflict();
        btnFind.Click += (_, _) => FindFreeNumber();
        btnOk.Click += (_, _) =>
        {
            ChosenNumber = (int)numNumber.Value;
            Resolution = rbAcceptDuplicate.Checked ? NumberConflictResolution.AcceptDuplicate
                : rbRenumberExisting.Checked ? NumberConflictResolution.RenumberExisting
                : NumberConflictResolution.ShiftNew;
        };
    }

    /// <summary>Window icon and the find button glyph from the default icon theme, as in AMC.</summary>
    private void LoadIcons()
    {
        try
        {
            using var strip = IconThemeImageList.LoadStrip(IconThemeImageList.DefaultThemeId, Path.Combine(AppContext.BaseDirectory, "Resources"));
            var bmp = (Bitmap)strip.Images[(int)AmcIcon.MovieNumber];
            Icon = (Icon)Icon.FromHandle(bmp.GetHicon()).Clone();
            ShowIcon = true;
            btnFind.Image = (Image)strip.Images[(int)AmcIcon.MovieFind].Clone();
        }
        catch
        {
            ShowIcon = false;
            btnFind.Text = "?";
        }
    }

    /// <summary>Resolution options only make sense when the number is taken (as in AMC).</summary>
    private void RefreshConflict()
    {
        int n = (int)numNumber.Value;
        bool dup = _isDuplicate is not null && _isDuplicate(n);
        foreach (var rb in new[] { rbAcceptDuplicate, rbRenumberExisting, rbShiftNew }) rb.Enabled = dup;
        lblTakenBy.Text = dup && _describe?.Invoke(n) is { } owner
            ? AppServices.Languages.Get("Number.TakenBy", n, owner)
            : "";
    }

    private void FindFreeNumber()
    {
        if (_isDuplicate is null) return;
        int n = Math.Max(1, (int)numNumber.Value);
        int guard = 0;
        while (_isDuplicate(n) && guard++ < 100000) n++;
        if (n > numNumber.Maximum) numNumber.Maximum = n + 100;
        numNumber.Value = n;
    }
}
