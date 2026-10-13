using GameCatalog.Core.Models;

namespace GameCatalog.Core.Services;

/// <summary>Przykładowe dane startowe (3 gry + DLC), żeby aplikacja nie startowała pusta.</summary>
public static class SampleDataFactory
{
    public static IEnumerable<Game> CreateSamples() => new List<Game>
    {
        new()
        {
            Number = 1, Title = "Wiedźmin 3: Dziki Gon", LocalizedTitle = "Wiedźmin 3: Dziki Gon",
            Developer = "CD PROJEKT RED", Publisher = "CD PROJEKT RED",
            Designer = "Konrad Tomaszkiewicz", Composer = "Marcin Przybyłowicz",
            VoiceActors = "Jacek Rozenek (Geralt)", Country = "Polska",
            ReleaseYear = 2015, PlaytimeHours = 120, Genre = "RPG", Pegi = "PEGI 18",
            Platform = "PC", Engine = "REDengine 3", GameMode = "Single-player",
            Store = "GOG", MediaType = "Cyfrowa",
            AudioLanguages = "polski, angielski", Subtitles = "polskie",
            UserRating = 100, CriticRating = 93,
            CompletionStatus = CompletionStatus.Completed100,
            DateAdded = new DateTime(2023, 5, 1), DateFinished = new DateTime(2023, 8, 20),
            LastPlayed = new DateTime(2023, 8, 20),
            Url = "https://www.thewitcher.com/pl/witcher3",
            Description = "Geralt z Rivii szuka Ciri. Otwarty świat, wybory moralne, dwa duże dodatki.",
            Comments = "Ulubiona gra. Dodatki obowiązkowe.",
            Extras = new List<GameExtra>
            {
                new() { Title = "Serca z kamienia", Category = "DLC fabularne", CreatedBy = "CD PROJEKT RED", Description = "Historia Olgierda von Evereca." },
                new() { Title = "Krew i Wino", Category = "Rozszerzenie", CreatedBy = "CD PROJEKT RED", Description = "Toussaint, Regis, winnice." },
            }
        },
        new()
        {
            Number = 2, Title = "Hades", Developer = "Supergiant Games", Publisher = "Supergiant Games",
            Country = "USA", ReleaseYear = 2020, PlaytimeHours = 65, Genre = "Roguelike",
            Pegi = "PEGI 12", Platform = "PC", Engine = "własny",
            GameMode = "Single-player", Store = "Steam", MediaType = "Cyfrowa",
            AudioLanguages = "angielski", Subtitles = "polskie",
            UserRating = 95, CriticRating = 93,
            CompletionStatus = CompletionStatus.Playing,
            DateAdded = new DateTime(2024, 1, 12), LastPlayed = DateTime.Today,
            Url = "https://www.supergiantgames.com/games/hades/",
            Description = "Roguelike z mitologią grecką. Ucieczka z Hadesu, relacje z bogami.",
        },
        new()
        {
            Number = 3, Title = "Baldur's Gate 3", Developer = "Larian Studios", Publisher = "Larian Studios",
            Country = "Belgia", ReleaseYear = 2023, PlaytimeHours = 0, Genre = "RPG",
            Pegi = "PEGI 18", Platform = "PS5", Engine = "Divinity 4.0",
            GameMode = "Single + Multi", Store = "PS Store", MediaType = "Cyfrowa",
            AudioLanguages = "angielski", Subtitles = "polskie",
            UserRating = -1, CriticRating = 96,
            CompletionStatus = CompletionStatus.Backlog,
            DateAdded = DateTime.Today,
            Description = "Turowy RPG w świecie D&D. Kooperacja do 4 graczy.",
        },
    };
}
