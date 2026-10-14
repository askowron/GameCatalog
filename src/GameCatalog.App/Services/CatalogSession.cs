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
        _store = new SqliteGameCatalogStore(path);
        var data = _store.LoadAll();
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
        Games.Clear();
        Games.CustomFieldDefs.Clear();
        Loans.Clear();
        Undo.Clear();
        _store = new SqliteGameCatalogStore(path);
        _store.SaveAll(Games, Loans);
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
        _store = new SqliteGameCatalogStore(path);
        FilePath = path;
        Save();
        RememberRecent(path);
    }

    /// <summary>Marks unsaved changes and notifies views.</summary>
    public void MarkDirty()
    {
        SetDirty(true);
        Changed?.Invoke(this, EventArgs.Empty);
    }

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
