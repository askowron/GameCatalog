using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;
using Microsoft.Data.Sqlite;

namespace GameCatalog.Storage;

/// <summary>
/// SQLite catalog store — counterpart of AMC catalog load/save (XML/SQL), game edition:
/// one .gc database with Games / Extras / Loans / CustomFieldDefs / CustomValues tables.
/// SaveAll rewrites the whole catalog in a single transaction.
/// </summary>
public sealed class SqliteGameCatalogStore
{
    private readonly string _filePath;
    public string FilePath => _filePath;

    public SqliteGameCatalogStore(string filePath) => _filePath = filePath;

    private SqliteConnection Open()
    {
        var cs = new SqliteConnectionStringBuilder { DataSource = _filePath }.ToString();
        var c = new SqliteConnection(cs);
        c.Open();
        return c;
    }

    public void Initialize()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Games(
 Number INTEGER PRIMARY KEY, Checked INTEGER NOT NULL DEFAULT 0, ColorTag INTEGER NOT NULL DEFAULT 0,
 MediaLabel TEXT NOT NULL DEFAULT '', MediaType TEXT NOT NULL DEFAULT '', Store TEXT NOT NULL DEFAULT '',
 DateAdded TEXT NULL, Borrower TEXT NOT NULL DEFAULT '',
 DateFinished TEXT NULL, LastPlayed TEXT NULL, CompletionStatus INTEGER NOT NULL DEFAULT 0,
 UserRating INTEGER NOT NULL DEFAULT -1, CriticRating INTEGER NOT NULL DEFAULT -1,
 Title TEXT NOT NULL DEFAULT '', LocalizedTitle TEXT NOT NULL DEFAULT '',
 Developer TEXT NOT NULL DEFAULT '', Publisher TEXT NOT NULL DEFAULT '',
 Designer TEXT NOT NULL DEFAULT '', Composer TEXT NOT NULL DEFAULT '',
 VoiceActors TEXT NOT NULL DEFAULT '', Country TEXT NOT NULL DEFAULT '',
 ReleaseYear INTEGER NOT NULL DEFAULT 0, PlaytimeHours REAL NOT NULL DEFAULT 0,
 Genre TEXT NOT NULL DEFAULT '', Pegi TEXT NOT NULL DEFAULT '', Platform TEXT NOT NULL DEFAULT '',
 Url TEXT NOT NULL DEFAULT '', Description TEXT NOT NULL DEFAULT '', Comments TEXT NOT NULL DEFAULT '',
 InstallPath TEXT NOT NULL DEFAULT '', Engine TEXT NOT NULL DEFAULT '', Version TEXT NOT NULL DEFAULT '',
 GameMode TEXT NOT NULL DEFAULT '', Resolution TEXT NOT NULL DEFAULT '', Framerate TEXT NOT NULL DEFAULT '',
 AudioLanguages TEXT NOT NULL DEFAULT '', Subtitles TEXT NOT NULL DEFAULT '',
 SizeGb TEXT NOT NULL DEFAULT '', DiskCount INTEGER NOT NULL DEFAULT 0,
 CoverImage BLOB NULL);
CREATE TABLE IF NOT EXISTS Extras(
 Id INTEGER PRIMARY KEY AUTOINCREMENT, GameNumber INTEGER NOT NULL,
 Checked INTEGER NOT NULL DEFAULT 0, Tag TEXT NOT NULL DEFAULT '', Title TEXT NOT NULL DEFAULT '',
 Category TEXT NOT NULL DEFAULT '', Url TEXT NOT NULL DEFAULT '',
 Description TEXT NOT NULL DEFAULT '', Comments TEXT NOT NULL DEFAULT '',
 CreatedBy TEXT NOT NULL DEFAULT '', CoverImage BLOB NULL);
CREATE TABLE IF NOT EXISTS Loans(
 Id INTEGER PRIMARY KEY AUTOINCREMENT, GameNumber INTEGER NOT NULL DEFAULT 0,
 GameTitle TEXT NOT NULL DEFAULT '', Borrower TEXT NOT NULL DEFAULT '',
 LoanDate TEXT NOT NULL, ReturnDate TEXT NULL);
CREATE TABLE IF NOT EXISTS CustomFieldDefs(
 Tag TEXT PRIMARY KEY, Name TEXT NOT NULL DEFAULT '',
 FieldType TEXT NOT NULL DEFAULT 'Text', ListValues TEXT NOT NULL DEFAULT '',
 DefaultValue TEXT NOT NULL DEFAULT '');
CREATE TABLE IF NOT EXISTS CustomValues(
 GameNumber INTEGER NOT NULL, Tag TEXT NOT NULL, Value TEXT NOT NULL DEFAULT '',
 PRIMARY KEY(GameNumber, Tag));";
        cmd.ExecuteNonQuery();
    }

    private static string? Dt(DateTime? d) => d?.ToString("yyyy-MM-dd");
    private static DateTime? ParseDt(object? o)
    {
        if (o is null || o == DBNull.Value) return null;
        var s = o.ToString();
        return DateTime.TryParse(s, out var d) ? d : null;
    }

    // ---------- save the whole catalog (like AMC Save) ----------
    public void SaveAll(GameList list, IEnumerable<LoanRecord> loans)
    {
        var duplicates = list.Games.GroupBy(g => g.Number).Where(grp => grp.Count() > 1).Select(grp => grp.Key).OrderBy(n => n).ToList();
        if (duplicates.Count > 0)
            throw new InvalidOperationException($"Duplicate game numbers: {string.Join(", ", duplicates)}. Renumber the games before saving.");
        Initialize();
        using var c = Open();
        using var tx = c.BeginTransaction();

        foreach (var g in list.Games) UpsertGame(c, g);
        // remove games that no longer exist
        var keep = string.Join(",", list.Games.Select(g => g.Number).DefaultIfEmpty(-1));
        using (var del = c.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = $"DELETE FROM Games WHERE Number NOT IN ({keep})";
            del.ExecuteNonQuery();
            del.CommandText = $"DELETE FROM Extras WHERE GameNumber NOT IN ({keep})";
            del.ExecuteNonQuery();
            del.CommandText = $"DELETE FROM CustomValues WHERE GameNumber NOT IN ({keep})";
            del.ExecuteNonQuery();
        }
        // custom field definitions
        using (var del = c.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM CustomFieldDefs";
            del.ExecuteNonQuery();
        }
        foreach (var d in list.CustomFieldDefs)
        {
            using var cmd = c.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "INSERT INTO CustomFieldDefs(Tag,Name,FieldType,ListValues,DefaultValue) VALUES($t,$n,$f,$l,$d)";
            cmd.Parameters.AddWithValue("$t", d.Tag);
            cmd.Parameters.AddWithValue("$n", d.Name);
            cmd.Parameters.AddWithValue("$f", d.FieldType);
            cmd.Parameters.AddWithValue("$l", d.ListValues);
            cmd.Parameters.AddWithValue("$d", d.DefaultValue);
            cmd.ExecuteNonQuery();
        }
        // loans: full refresh
        using (var del = c.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM Loans";
            del.ExecuteNonQuery();
        }
        foreach (var l in loans)
        {
            using var cmd = c.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "INSERT INTO Loans(GameNumber,GameTitle,Borrower,LoanDate,ReturnDate) VALUES($g,$t,$b,$ld,$rd)";
            cmd.Parameters.AddWithValue("$g", l.GameNumber);
            cmd.Parameters.AddWithValue("$t", l.GameTitle);
            cmd.Parameters.AddWithValue("$b", l.Borrower);
            cmd.Parameters.AddWithValue("$ld", l.LoanDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$rd", (object?)l.ReturnDate?.ToString("yyyy-MM-dd") ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    private static void UpsertGame(SqliteConnection c, Game g)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Games(Number,Checked,ColorTag,MediaLabel,MediaType,Store,DateAdded,Borrower,
 DateFinished,LastPlayed,CompletionStatus,UserRating,CriticRating,Title,LocalizedTitle,
 Developer,Publisher,Designer,Composer,VoiceActors,Country,ReleaseYear,PlaytimeHours,
 Genre,Pegi,Platform,Url,Description,Comments,InstallPath,Engine,Version,GameMode,
 Resolution,Framerate,AudioLanguages,Subtitles,SizeGb,DiskCount,CoverImage)
VALUES($n,$ch,$ct,$ml,$mt,$st,$da,$bo,$df,$lp,$cs,$ur,$cr,$ti,$lt,$de,$pu,$ds,$co,$va,$cn,$ry,$ph,$ge,$pe,$pl,$ur2,$de2,$cm,$ip,$en,$ve,$gm,$re,$fr,$al,$su,$sz,$dc,$cv)
ON CONFLICT(Number) DO UPDATE SET Checked=excluded.Checked,ColorTag=excluded.ColorTag,
 MediaLabel=excluded.MediaLabel,MediaType=excluded.MediaType,Store=excluded.Store,DateAdded=excluded.DateAdded,
 Borrower=excluded.Borrower,DateFinished=excluded.DateFinished,LastPlayed=excluded.LastPlayed,
 CompletionStatus=excluded.CompletionStatus,UserRating=excluded.UserRating,CriticRating=excluded.CriticRating,
 Title=excluded.Title,LocalizedTitle=excluded.LocalizedTitle,Developer=excluded.Developer,Publisher=excluded.Publisher,
 Designer=excluded.Designer,Composer=excluded.Composer,VoiceActors=excluded.VoiceActors,Country=excluded.Country,
 ReleaseYear=excluded.ReleaseYear,PlaytimeHours=excluded.PlaytimeHours,Genre=excluded.Genre,Pegi=excluded.Pegi,
 Platform=excluded.Platform,Url=excluded.Url,Description=excluded.Description,Comments=excluded.Comments,
 InstallPath=excluded.InstallPath,Engine=excluded.Engine,Version=excluded.Version,GameMode=excluded.GameMode,
 Resolution=excluded.Resolution,Framerate=excluded.Framerate,AudioLanguages=excluded.AudioLanguages,
 Subtitles=excluded.Subtitles,SizeGb=excluded.SizeGb,DiskCount=excluded.DiskCount,CoverImage=excluded.CoverImage";
        cmd.Parameters.AddWithValue("$n", g.Number);
        cmd.Parameters.AddWithValue("$ch", g.Checked ? 1 : 0);
        cmd.Parameters.AddWithValue("$ct", g.ColorTag);
        cmd.Parameters.AddWithValue("$ml", g.MediaLabel);
        cmd.Parameters.AddWithValue("$mt", g.MediaType);
        cmd.Parameters.AddWithValue("$st", g.Store);
        cmd.Parameters.AddWithValue("$da", (object?)Dt(g.DateAdded) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$bo", g.Borrower);
        cmd.Parameters.AddWithValue("$df", (object?)Dt(g.DateFinished) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$lp", (object?)Dt(g.LastPlayed) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$cs", (int)g.CompletionStatus);
        cmd.Parameters.AddWithValue("$ur", g.UserRating);
        cmd.Parameters.AddWithValue("$cr", g.CriticRating);
        cmd.Parameters.AddWithValue("$ti", g.Title);
        cmd.Parameters.AddWithValue("$lt", g.LocalizedTitle);
        cmd.Parameters.AddWithValue("$de", g.Developer);
        cmd.Parameters.AddWithValue("$pu", g.Publisher);
        cmd.Parameters.AddWithValue("$ds", g.Designer);
        cmd.Parameters.AddWithValue("$co", g.Composer);
        cmd.Parameters.AddWithValue("$va", g.VoiceActors);
        cmd.Parameters.AddWithValue("$cn", g.Country);
        cmd.Parameters.AddWithValue("$ry", g.ReleaseYear);
        cmd.Parameters.AddWithValue("$ph", g.PlaytimeHours);
        cmd.Parameters.AddWithValue("$ge", g.Genre);
        cmd.Parameters.AddWithValue("$pe", g.Pegi);
        cmd.Parameters.AddWithValue("$pl", g.Platform);
        cmd.Parameters.AddWithValue("$ur2", g.Url);
        cmd.Parameters.AddWithValue("$de2", g.Description);
        cmd.Parameters.AddWithValue("$cm", g.Comments);
        cmd.Parameters.AddWithValue("$ip", g.InstallPath);
        cmd.Parameters.AddWithValue("$en", g.Engine);
        cmd.Parameters.AddWithValue("$ve", g.Version);
        cmd.Parameters.AddWithValue("$gm", g.GameMode);
        cmd.Parameters.AddWithValue("$re", g.Resolution);
        cmd.Parameters.AddWithValue("$fr", g.Framerate);
        cmd.Parameters.AddWithValue("$al", g.AudioLanguages);
        cmd.Parameters.AddWithValue("$su", g.Subtitles);
        cmd.Parameters.AddWithValue("$sz", g.SizeGb);
        cmd.Parameters.AddWithValue("$dc", g.DiskCount);
        cmd.Parameters.AddWithValue("$cv", (object?)g.CoverImage ?? DBNull.Value);
        cmd.ExecuteNonQuery();

        using var del = c.CreateCommand();
        del.CommandText = "DELETE FROM Extras WHERE GameNumber=$g";
        del.Parameters.AddWithValue("$g", g.Number);
        del.ExecuteNonQuery();

        foreach (var e in g.Extras)
        {
            using var ec = c.CreateCommand();
            ec.CommandText = "INSERT INTO Extras(GameNumber,Checked,Tag,Title,Category,Url,Description,Comments,CreatedBy,CoverImage) VALUES($g,$c,$t,$ti,$ca,$u,$d,$cm,$cb,$cv)";
            ec.Parameters.AddWithValue("$g", g.Number);
            ec.Parameters.AddWithValue("$c", e.Checked ? 1 : 0);
            ec.Parameters.AddWithValue("$t", e.Tag);
            ec.Parameters.AddWithValue("$ti", e.Title);
            ec.Parameters.AddWithValue("$ca", e.Category);
            ec.Parameters.AddWithValue("$u", e.Url);
            ec.Parameters.AddWithValue("$d", e.Description);
            ec.Parameters.AddWithValue("$cm", e.Comments);
            ec.Parameters.AddWithValue("$cb", e.CreatedBy);
            ec.Parameters.AddWithValue("$cv", (object?)e.CoverImage ?? DBNull.Value);
            ec.ExecuteNonQuery();
        }

        using var delv = c.CreateCommand();
        delv.CommandText = "DELETE FROM CustomValues WHERE GameNumber=$g";
        delv.Parameters.AddWithValue("$g", g.Number);
        delv.ExecuteNonQuery();
        foreach (var kv in g.CustomValues)
        {
            using var vc = c.CreateCommand();
            vc.CommandText = "INSERT INTO CustomValues(GameNumber,Tag,Value) VALUES($g,$t,$v)";
            vc.Parameters.AddWithValue("$g", g.Number);
            vc.Parameters.AddWithValue("$t", kv.Key);
            vc.Parameters.AddWithValue("$v", kv.Value);
            vc.ExecuteNonQuery();
        }
    }

    // ---------- load (like AMC Load) ----------
    public CatalogData LoadAll()
    {
        Initialize();
        var games = new List<Game>();
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Games ORDER BY Number";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var g = new Game
            {
                Number = Convert.ToInt32(r["Number"]),
                Checked = Convert.ToInt32(r["Checked"]) != 0,
                ColorTag = Convert.ToInt32(r["ColorTag"]),
                MediaLabel = r["MediaLabel"].ToString() ?? "",
                MediaType = r["MediaType"].ToString() ?? "",
                Store = r["Store"].ToString() ?? "",
                DateAdded = ParseDt(r["DateAdded"]),
                Borrower = r["Borrower"].ToString() ?? "",
                DateFinished = ParseDt(r["DateFinished"]),
                LastPlayed = ParseDt(r["LastPlayed"]),
                CompletionStatus = (CompletionStatus)Convert.ToInt32(r["CompletionStatus"]),
                UserRating = Convert.ToInt32(r["UserRating"]),
                CriticRating = Convert.ToInt32(r["CriticRating"]),
                Title = r["Title"].ToString() ?? "",
                LocalizedTitle = r["LocalizedTitle"].ToString() ?? "",
                Developer = r["Developer"].ToString() ?? "",
                Publisher = r["Publisher"].ToString() ?? "",
                Designer = r["Designer"].ToString() ?? "",
                Composer = r["Composer"].ToString() ?? "",
                VoiceActors = r["VoiceActors"].ToString() ?? "",
                Country = r["Country"].ToString() ?? "",
                ReleaseYear = Convert.ToInt32(r["ReleaseYear"]),
                PlaytimeHours = Convert.ToDouble(r["PlaytimeHours"]),
                Genre = r["Genre"].ToString() ?? "",
                Pegi = r["Pegi"].ToString() ?? "",
                Platform = r["Platform"].ToString() ?? "",
                Url = r["Url"].ToString() ?? "",
                Description = r["Description"].ToString() ?? "",
                Comments = r["Comments"].ToString() ?? "",
                InstallPath = r["InstallPath"].ToString() ?? "",
                Engine = r["Engine"].ToString() ?? "",
                Version = r["Version"].ToString() ?? "",
                GameMode = r["GameMode"].ToString() ?? "",
                Resolution = r["Resolution"].ToString() ?? "",
                Framerate = r["Framerate"].ToString() ?? "",
                AudioLanguages = r["AudioLanguages"].ToString() ?? "",
                Subtitles = r["Subtitles"].ToString() ?? "",
                SizeGb = r["SizeGb"].ToString() ?? "",
                DiskCount = Convert.ToInt32(r["DiskCount"]),
                CoverImage = r["CoverImage"] == DBNull.Value ? null : (byte[])r["CoverImage"],
            };
            games.Add(g);
        }
        r.Close();

        foreach (var g in games)
        {
            using var ec = c.CreateCommand();
            ec.CommandText = "SELECT * FROM Extras WHERE GameNumber=$g ORDER BY Id";
            ec.Parameters.AddWithValue("$g", g.Number);
            using var er = ec.ExecuteReader();
            while (er.Read())
            {
                g.Extras.Add(new GameExtra
                {
                    Id = Convert.ToInt32(er["Id"]),
                    GameNumber = g.Number,
                    Checked = Convert.ToInt32(er["Checked"]) != 0,
                    Tag = er["Tag"].ToString() ?? "",
                    Title = er["Title"].ToString() ?? "",
                    Category = er["Category"].ToString() ?? "",
                    Url = er["Url"].ToString() ?? "",
                    Description = er["Description"].ToString() ?? "",
                    Comments = er["Comments"].ToString() ?? "",
                    CreatedBy = er["CreatedBy"].ToString() ?? "",
                    CoverImage = er["CoverImage"] == DBNull.Value ? null : (byte[])er["CoverImage"],
                });
            }
            er.Close();

            using var vc = c.CreateCommand();
            vc.CommandText = "SELECT Tag,Value FROM CustomValues WHERE GameNumber=$g";
            vc.Parameters.AddWithValue("$g", g.Number);
            using var vr = vc.ExecuteReader();
            while (vr.Read()) g.CustomValues[vr["Tag"].ToString() ?? ""] = vr["Value"].ToString() ?? "";
            vr.Close();
        }

        var defs = new List<CustomFieldDefinition>();
        using var dc = c.CreateCommand();
        dc.CommandText = "SELECT * FROM CustomFieldDefs ORDER BY Tag";
        using var dr = dc.ExecuteReader();
        while (dr.Read())
        {
            defs.Add(new CustomFieldDefinition
            {
                Tag = dr["Tag"].ToString() ?? "",
                Name = dr["Name"].ToString() ?? "",
                FieldType = dr["FieldType"].ToString() ?? "Text",
                ListValues = dr["ListValues"].ToString() ?? "",
                DefaultValue = dr["DefaultValue"].ToString() ?? "",
            });
        }
        dr.Close();

        var loans = new List<LoanRecord>();
        using var lc = c.CreateCommand();
        lc.CommandText = "SELECT * FROM Loans ORDER BY LoanDate DESC, Id DESC";
        using var lr = lc.ExecuteReader();
        while (lr.Read())
        {
            loans.Add(new LoanRecord
            {
                Id = Convert.ToInt32(lr["Id"]),
                GameNumber = Convert.ToInt32(lr["GameNumber"]),
                GameTitle = lr["GameTitle"].ToString() ?? "",
                Borrower = lr["Borrower"].ToString() ?? "",
                LoanDate = DateTime.TryParse(lr["LoanDate"].ToString(), out var d) ? d : DateTime.Today,
                ReturnDate = string.IsNullOrWhiteSpace(lr["ReturnDate"].ToString()) ? null :
                             DateTime.TryParse(lr["ReturnDate"].ToString(), out var rd) ? rd : null,
            });
        }
        return new CatalogData(games, defs, loans);
    }
}
