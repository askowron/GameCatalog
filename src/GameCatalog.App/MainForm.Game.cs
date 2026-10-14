using System.ComponentModel;
using System.Diagnostics;
using GameCatalog.App.Forms;
using GameCatalog.App.Services;
using GameCatalog.Core.Models;
using GameCatalog.UI.Components;
using GameCatalog.UI.Controls;

namespace GameCatalog.App;

/// <summary>Game menu: add/delete/number, clipboard, undo, pictures, extras, URL/path, web search.</summary>
public partial class MainForm
{
    private void actGameAdd_Execute(object? sender, EventArgs e)
    {
        CommitEditors();
        var game = new Game { Title = L.Get("Game.NewTitle"), DateAdded = DateTime.Today };
        if (!_prefs.AutoNumbering)
        {
            // AMC mode: ask for the number ("Movie number" dialog).
            int proposed = _session.Games.NextNumber();
            using var dialog = new NumberConflictDialog(proposed, proposed,
                n => _session.Games.Find(n) is not null, n => _session.Games.Find(n)?.DisplayTitle);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (dialog.DontAskAgain)
            {
                _prefs.AutoNumbering = true;
                _prefs.Save();
            }
            game.Number = _numbering.ResolveNewNumber(dialog.ChosenNumber, dialog.Resolution);
        }
        _session.Games.Add(game);
        _session.Undo.Push(() =>
        {
            _session.Games.Remove(game);
            _session.MarkDirty();
        });
        _session.MarkDirty();
        SelectGame(game.Number);
        tabsGame.SelectedTab = tabGameFields;
        gameFrame.FocusTitle();
    }

    private void actGameDelete_Execute(object? sender, EventArgs e)
    {
        // Like AMC, the delete shortcut is ignored while the user is typing in an editor.
        if (gameFrame.ContainsFocus || customFrame.ContainsFocus || findBar.ContainsFocus) return;
        if (_current is not { } game) return;
        if (MessageBox.Show(this, L.Get("Msg.ConfirmDelete", game.Title), L.Get("Msg.Confirm"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _session.Games.Remove(game);
        _session.Undo.Push(() =>
        {
            _session.Games.Add(game);
            _session.MarkDirty();
            SelectGame(game.Number);
        });
        _session.MarkDirty();
    }

    private void actGameFind_Execute(object? sender, EventArgs e) => ShowFindBar(!findBar.Visible);

    private void actGameRandom_Execute(object? sender, EventArgs e)
    {
        var view = _presenter.CurrentView();
        if (view.Count == 0) return;
        SelectGame(view[Random.Shared.Next(view.Count)].Number);
        SetHint(L.Get("Status.RandomPicked"));
    }

    private void actGameGoToNumber_Execute(object? sender, EventArgs e)
    {
        int max = Math.Max(1, _session.Games.Games.Count == 0 ? 1 : _session.Games.Games.Max(g => g.Number));
        using var dialog = new NumberDialog(_current?.Number ?? 1, max);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (_session.Games.Find(dialog.Value) is null)
        {
            MessageBox.Show(this, L.Get("Msg.NoGameWithNumber", dialog.Value), L.Get("Common.AppName"));
            return;
        }
        _presenter.State.FindOnly = null;
        RefreshList();
        SelectGame(dialog.Value);
    }

    private void actGameChangeNumber_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game) return;
        using var dialog = new NumberDialog(game.Number, Math.Max(_session.Games.NextNumber(), game.Number)) { Text = L.Get("Number.ChangeTitle") };
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Value == game.Number) return;
        int target = dialog.Value;
        var resolution = NumberConflictResolution.RenumberExisting;
        if (_session.Games.Find(target) is not null)
        {
            // Conflict like in AMC: show the "Game number" dialog.
            using var conflict = new NumberConflictDialog(target, Math.Max(_session.Games.NextNumber(), target),
                n => n != game.Number && _session.Games.Find(n) is not null,
                n => n != game.Number ? _session.Games.Find(n)?.DisplayTitle : null);
            if (conflict.ShowDialog(this) != DialogResult.OK) return;
            if (conflict.DontAskAgain)
            {
                _prefs.AutoNumbering = true;
                _prefs.Save();
            }
            target = conflict.ChosenNumber;
            resolution = conflict.Resolution;
        }
        if (_numbering.ChangeNumber(game, target, resolution) is int assigned) SelectGame(assigned);
    }

    private void actGameCopy_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game) return;
        CommitEditors();
        Clipboard.SetText(GameClipboard.Serialize(game));
        SetHint(L.Get("Status.GameCopied", game.Title));
    }

    private void actGamePaste_Execute(object? sender, EventArgs e)
    {
        var game = GameClipboard.TryDeserialize(Clipboard.ContainsText() ? Clipboard.GetText() : null);
        if (game is null)
        {
            MessageBox.Show(this, L.Get("Msg.ClipboardNoGame"), L.Get("Common.AppName"));
            return;
        }
        _session.Games.Add(game);
        _session.Undo.Push(() =>
        {
            _session.Games.Remove(game);
            _session.MarkDirty();
        });
        _session.MarkDirty();
        SelectGame(game.Number);
    }

    private void actGameUndo_Execute(object? sender, EventArgs e)
    {
        var game = _current;
        CommitEditors();
        SetHint(L.Get(_session.Undo.TryUndo() ? "Status.Undone" : "Status.NothingToUndo"));
        // The undo may have renumbered the selected game: follow the instance, not the old number.
        if (game is not null && IsInCatalog(game)) SelectGame(game.Number);
        RebindCurrent();
    }

    private bool IsInCatalog(Game game) => _session.Games.Games.Any(g => ReferenceEquals(g, game));

    private void actGameUndo_Update(object? sender, EventArgs e) => actGameUndo.Enabled = _session.Undo.Count > 0;

    private void actGameCheckAll_Execute(object? sender, EventArgs e) => SetAllChecked(true);
    private void actGameUncheckAll_Execute(object? sender, EventArgs e) => SetAllChecked(false);

    private void SetAllChecked(bool value)
    {
        foreach (var g in _presenter.CurrentView()) g.Checked = value;
        _session.MarkDirty();
        RebindCurrent();
    }

    private void SetColorTag(int tag)
    {
        if (_current is not { } game) return;
        game.ColorTag = tag;
        _session.MarkModified();
        UpdateRow(game);
    }

    // --- Pictures (AMC PanelPicture + ToolbarPicture) ---

    private void actPicShow_Execute(object? sender, EventArgs e)
    {
        _prefs.CoverVisible = !_prefs.CoverVisible;
        _prefs.Save();
        ApplyCoverVisibility();
    }

    private void actPicShow_Update(object? sender, EventArgs e) => actPicShow.Checked = _prefs.CoverVisible;

    private void ApplyCoverVisibility() => splitLeft.Panel2Collapsed = !_prefs.CoverVisible;

    private void actPicLoad_Execute(object? sender, EventArgs e)
    {
        if (!RequireCurrent()) return;
        using var dialog = new OpenFileDialog { Filter = L.Get("Filter.Images") };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        byte[] bytes;
        try
        {
            bytes = CoverImages.Downscale(File.ReadAllBytes(dialog.FileName), _prefs.MaxCoverWidth);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, L.Get("Common.Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        SetCover(bytes);
        SetHint(L.Get("Status.CoverLoaded"));
    }

    private void actPicSaveAs_Execute(object? sender, EventArgs e)
    {
        if (_current?.CoverImage is not { } cover) return;
        using var dialog = new SaveFileDialog { Filter = L.Get("Filter.ImageSave"), FileName = L.Get("File.CoverName", _current.Number) };
        if (dialog.ShowDialog(this) == DialogResult.OK) File.WriteAllBytes(dialog.FileName, cover);
    }

    private void actPicCopy_Execute(object? sender, EventArgs e)
    {
        if (pictureWindow.CurrentImage is { } image) Clipboard.SetImage(image);
    }

    private void actPicPaste_Execute(object? sender, EventArgs e)
    {
        if (_current is null || !Clipboard.ContainsImage()) return;
        using var image = Clipboard.GetImage();
        if (image is not null) SetCover(CoverImages.ToPng(image));
    }

    private void actPicDelete_Execute(object? sender, EventArgs e)
    {
        if (_current?.HasCover == true) SetCover(null);
    }

    private void SetCover(byte[]? bytes)
    {
        var game = _current!;
        var old = game.CoverImage;
        game.CoverImage = bytes;
        _session.Undo.Push(() =>
        {
            game.CoverImage = old;
            _session.MarkModified();
            RebindCurrent();
        });
        _session.MarkModified();
        pictureWindow.ShowCover(bytes);
        _ = RefreshHtmlAsync();
        if (_presenter.State.Thumbnails) RefreshList();
    }

    // --- Extras (AMC extras tab) ---

    private void actExtrasShow_Execute(object? sender, EventArgs e) => tabsGame.SelectedTab = tabExtras;

    private void actExtraAdd_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game) return;
        var extra = new GameExtra { GameNumber = game.Number, Category = ExtraCategories.All[0] };
        using var form = new ExtraEditForm(extra);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        game.Extras.Add(extra);
        _session.Undo.Push(() =>
        {
            game.Extras.Remove(extra);
            _session.MarkModified();
            RebindCurrent();
        });
        ExtrasChanged();
    }

    private void actExtraEdit_Execute(object? sender, EventArgs e)
    {
        if (extrasFrame.SelectedExtra is not { } extra) return;
        var backup = extra.Clone();
        using var form = new ExtraEditForm(extra);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _session.Undo.Push(() =>
        {
            CopyExtra(backup, extra);
            _session.MarkModified();
            RebindCurrent();
        });
        ExtrasChanged();
    }

    private void actExtraDelete_Execute(object? sender, EventArgs e)
    {
        if (_current is not { } game || extrasFrame.SelectedExtra is not { } extra) return;
        int index = game.Extras.IndexOf(extra);
        game.Extras.Remove(extra);
        _session.Undo.Push(() =>
        {
            game.Extras.Insert(Math.Clamp(index, 0, game.Extras.Count), extra);
            _session.MarkModified();
            RebindCurrent();
        });
        ExtrasChanged();
    }

    private void ExtrasChanged()
    {
        _session.MarkModified();
        extrasFrame.Bind(_current);
        allFieldsFrame.Bind(_current, _session.Games.CustomFieldDefs);
        if (_current is not null) UpdateRow(_current);
    }

    private static void CopyExtra(GameExtra from, GameExtra to)
    {
        to.Title = from.Title;
        to.Category = from.Category;
        to.Tag = from.Tag;
        to.CreatedBy = from.CreatedBy;
        to.Url = from.Url;
        to.Description = from.Description;
        to.Comments = from.Comments;
        to.Checked = from.Checked;
    }

    // --- URL / path / web search ---

    private void actUrlOpen_Execute(object? sender, EventArgs e) => OpenExternal(_current?.Url, "Msg.CannotOpenUrl");

    private void actUrlCopy_Execute(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_current?.Url)) Clipboard.SetText(_current.Url);
    }

    private void actPathOpen_Execute(object? sender, EventArgs e)
    {
        var path = _current?.InstallPath;
        if (string.IsNullOrWhiteSpace(path)) return;
        string target = Directory.Exists(path) ? path : File.Exists(path) ? Path.GetDirectoryName(path)! : path;
        try { Process.Start(new ProcessStartInfo("explorer", $"\"{target}\"") { UseShellExecute = true }); }
        catch (Win32Exception ex) { MessageBox.Show(this, L.Get("Msg.CannotOpenPath", ex.Message)); }
    }

    private void actPathCopy_Execute(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_current?.InstallPath)) Clipboard.SetText(_current.InstallPath);
    }

    private void actGetInfoFiles_Execute(object? sender, EventArgs e) => ImportFromFolder();

    private void WebSearch(SearchSite site)
    {
        var title = _current?.Title?.Trim();
        if (string.IsNullOrEmpty(title))
        {
            SetHint(L.Get("Status.NoGameSelected"));
            return;
        }
        string q = Uri.EscapeDataString(title);
        OpenExternal(site.Url.Contains("{q}") ? site.Url.Replace("{q}", q) : site.Url + q, "Msg.CannotOpenUrl");
    }

    private void OpenExternal(string? target, string errorKey)
    {
        if (string.IsNullOrWhiteSpace(target)) return;
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
        catch (Win32Exception ex) { MessageBox.Show(this, L.Get(errorKey, ex.Message)); }
    }
}
