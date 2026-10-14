using GameCatalog.Core.Catalog;
using GameCatalog.Core.Models;
using Microsoft.Data.Sqlite;

namespace GameCatalog.Storage.Tests;

public sealed class SqliteStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"gc-test-{Guid.NewGuid():N}.gc");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_path)) File.Delete(_path);
    }

    [Fact]
    public void SaveAll_ThenLoadAll_RoundTripsEveryTable()
    {
        var list = new GameList();
        var g = new Game
        {
            Title = "Half-Life", LocalizedTitle = "HL", Developer = "Valve", ReleaseYear = 1998,
            PlaytimeHours = 12.5, UserRating = 95, CriticRating = -1, CompletionStatus = CompletionStatus.Completed100,
            LastPlayed = new DateTime(2024, 1, 2), CoverImage = new byte[] { 1, 2, 3 }, Checked = true, ColorTag = 3,
        };
        g.Extras.Add(new GameExtra { Title = "Opposing Force", Category = "Expansion" });
        g.CustomValues["Disc"] = "Blue";
        list.Add(g);
        list.CustomFieldDefs.Add(new CustomFieldDefinition { Tag = "Disc", Name = "Disc", FieldType = "Text" });
        var loans = new List<LoanRecord> { new() { GameNumber = 1, GameTitle = "Half-Life", Borrower = "Ola", LoanDate = new DateTime(2024, 3, 4) } };

        var store = new SqliteGameCatalogStore(_path);
        store.SaveAll(list, loans);
        var data = store.LoadAll();

        var r = Assert.Single(data.Games);
        Assert.Equal("Half-Life", r.Title);
        Assert.Equal("HL", r.LocalizedTitle);
        Assert.Equal(12.5, r.PlaytimeHours);
        Assert.Equal(95, r.UserRating);
        Assert.Equal(-1, r.CriticRating);
        Assert.Equal(CompletionStatus.Completed100, r.CompletionStatus);
        Assert.Equal(new DateTime(2024, 1, 2), r.LastPlayed);
        Assert.Equal(new byte[] { 1, 2, 3 }, r.CoverImage);
        Assert.True(r.Checked);
        Assert.Equal(3, r.ColorTag);
        Assert.Equal("Opposing Force", Assert.Single(r.Extras).Title);
        Assert.Equal("Blue", r.CustomValues["Disc"]);
        Assert.Equal("Disc", Assert.Single(data.CustomFieldDefs).Tag);
        Assert.Equal("Ola", Assert.Single(data.Loans).Borrower);
    }

    [Fact]
    public void SaveAll_RemovesDeletedGamesAndTheirChildren()
    {
        var list = new GameList();
        list.Add(new Game { Title = "A" });
        list.Add(new Game { Title = "B", Extras = { new GameExtra { Title = "DLC" } } });
        var store = new SqliteGameCatalogStore(_path);
        store.SaveAll(list, Array.Empty<LoanRecord>());

        list.Remove(2);
        store.SaveAll(list, Array.Empty<LoanRecord>());

        var data = store.LoadAll();
        Assert.Equal("A", Assert.Single(data.Games).Title);
        Assert.Empty(data.Games[0].Extras);
    }

    [Fact]
    public void LoadAll_LegacyPolishYesValue_IsPreserved()
    {
        var list = new GameList();
        var g = new Game { Title = "A" };
        g.CustomValues["Owned"] = "Tak";
        list.Add(g);
        var store = new SqliteGameCatalogStore(_path);
        store.SaveAll(list, Array.Empty<LoanRecord>());

        var loaded = store.LoadAll().Games[0];
        Assert.True(Core.Fields.YesNo.Parse(loaded.CustomValues["Owned"]));
    }
}
