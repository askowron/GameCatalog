using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;
using GameCatalog.Core.Services;
using GameCatalog.Storage;

namespace GameCatalog.App.Services;

/// <summary>
/// The open catalog: games, loans, file, dirty flag and undo stack (AMC TMovieList + file
/// handling from main.pas). UI code listens to its events instead of tracking state itself.
/// </summary>
public sealed class CatalogSession(PrefsStore prefs)
{
    private SqliteGameCatalogStore? _store;

    public GameList Games { get; } = new();
    public List<LoanRecord> Loans { get; } = new();
    public UndoService Undo { get; } = new();
    public string FilePath { get; private set; } = "";
    public bool IsDirty { get; private set; }

    /// <summary>A catalog was opened or created.</summary>
    public event EventHandler? Opened;
    /// <summary>Raised before writing — editors commit pending changes here.</summary>
    public event EventHandler? Saving;
    public event EventHandler? Saved;
    public event EventHandler? DirtyChanged;
    /// <summary>Catalog content changed; views refresh.</summary>
    public event EventHandler? Changed;

    /// <summary>Documents\GameCatalog\catalog.gc (copied once from the legacy catalog.db).</summary>
    public static string DefaultPath()
    {
        var gc = Path.Combine(AppPaths.DataDirectory, "catalog.gc");
        var legacy = Path.Combine(AppPaths.DataDirectory, "catalog.db");
        if (!File.Exists(gc) && File.Exists(legacy))
        {
            try { File.Copy(legacy, gc); }
            catch (IOException) { /* the legacy file stays usable */ }
        }
        return gc;
    }

    /// <param name="addSamplesIfNew">Fill a catalog file that did not exist yet with sample games (first run).</param>
    public void Open(string path, bool addSamplesIfNew = false)
    {
        bool existed = File.Exists(path);
        // Load first: a corrupt or locked file must leave the current session untouched.
        var store = new SqliteGameCatalogStore(path);
        var data = store.LoadAll();
        _store = store;
        Games.ReplaceAll(data.Games);
        Games.CustomFieldDefs.Clear();
        Games.CustomFieldDefs.AddRange(data.CustomFieldDefs);
        Loans.Clear();
        Loans.AddRange(data.Loans);
        Undo.Clear();
        FilePath = path;
        SetDirty(false);
        if (!existed && addSamplesIfNew)
        {
            foreach (var g in SampleDataFactory.CreateSamples()) Games.Add(g);
            _store.SaveAll(Games, Loans);
        }
        RememberRecent(path);
        Opened?.Invoke(this, EventArgs.Empty);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void New(string path)
    {
        // Create the file first: when that fails the current catalog stays as it was.
        var store = new SqliteGameCatalogStore(path);
        store.SaveAll(new GameList(), Array.Empty<LoanRecord>());
        _store = store;
        Games.Clear();
        Games.CustomFieldDefs.Clear();
        Loans.Clear();
        Undo.Clear();
        FilePath = path;
        SetDirty(false);
        RememberRecent(path);
        Opened?.Invoke(this, EventArgs.Empty);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Save()
    {
        if (_store is null) throw new InvalidOperationException("No catalog is open.");
        Saving?.Invoke(this, EventArgs.Empty);
        if (prefs.BackupOnSave && File.Exists(FilePath))
        {
            try { File.Copy(FilePath, FilePath + ".bak", overwrite: true); }
            catch (IOException) { /* a failed backup must not block saving */ }
        }
        _store.SaveAll(Games, Loans);
        SetDirty(false);
        Saved?.Invoke(this, EventArgs.Empty);
    }

    public void SaveAs(string path)
    {
        var previousStore = _store;
        var previousPath = FilePath;
        _store = new SqliteGameCatalogStore(path);
        FilePath = path;
        try { Save(); }
        catch
        {
            _store = previousStore;
            FilePath = previousPath;
            throw;
        }
        RememberRecent(path);
    }

    /// <summary>Marks unsaved changes and notifies views.</summary>
    public void MarkDirty()
    {
        SetDirty(true);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Marks unsaved changes without asking views to rebuild (single-field edits).</summary>
    public void MarkModified() => SetDirty(true);

    private void SetDirty(bool value)
    {
        if (IsDirty == value) return;
        IsDirty = value;
        DirtyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RememberRecent(string path)
    {
        prefs.PushRecent(path);
        prefs.Save();
    }
}
