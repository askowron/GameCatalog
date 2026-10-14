using System.Globalization;
using GameCatalog.Core.Models;

namespace GameCatalog.Core.Fields;

/// <summary>
/// Writes a textual value into the field identified by tag — the inverse of <see cref="GameFieldValues"/>.
/// Used when applying script / info-provider results. Unknown tags and tags prefixed with
/// <c>custom:</c> are written to <see cref="Game.CustomValues"/> (an empty value removes the entry).
/// </summary>
public static class GameFieldWriter
{
    public const string CustomPrefix = "custom:";

    /// <returns>False when the value cannot be parsed or the field is virtual; the game is then unchanged.</returns>
    public static bool Set(Game g, string tag, string value)
    {
        value ??= "";
        if (tag.StartsWith(CustomPrefix, StringComparison.Ordinal))
            return SetCustom(g, tag[CustomPrefix.Length..], value);
        if (GameFields.VirtualTags.Contains(tag)) return false;

        switch (tag)
        {
            case "Number": return TryInt(value, v => g.Number = v);
            case "ColorTag": return TryInt(value, v => g.ColorTag = v);
            case "ReleaseYear": return TryInt(value, v => g.ReleaseYear = v);
            case "DiskCount": return TryInt(value, v => g.DiskCount = v);
            case "UserRating": return TryDouble(value, v => g.UserRating = (int)Math.Round(v * 10));
            case "CriticRating": return TryDouble(value, v => g.CriticRating = (int)Math.Round(v * 10));
            case "PlaytimeHours": return TryDouble(value, v => g.PlaytimeHours = v);
            case "DateAdded": return TryDate(value, v => g.DateAdded = v);
            case "DateFinished": return TryDate(value, v => g.DateFinished = v);
            case "LastPlayed": return TryDate(value, v => g.LastPlayed = v);
            case "Checked": g.Checked = YesNo.Parse(value) || value.Equals(CatalogLabels.Current.Yes, StringComparison.OrdinalIgnoreCase); return true;
            case "CompletionStatus":
                if (!TryParseStatus(value, out var status)) return false;
                g.CompletionStatus = status;
                return true;
            case "MediaLabel": g.MediaLabel = value; return true;
            case "MediaType": g.MediaType = value; return true;
            case "Store": g.Store = value; return true;
            case "Borrower": g.Borrower = value; return true;
            case "Title": g.Title = value; return true;
            case "LocalizedTitle": g.LocalizedTitle = value; return true;
            case "Developer": g.Developer = value; return true;
            case "Publisher": g.Publisher = value; return true;
            case "Designer": g.Designer = value; return true;
            case "Composer": g.Composer = value; return true;
            case "VoiceActors": g.VoiceActors = value; return true;
            case "Country": g.Country = value; return true;
            case "Genre": g.Genre = value; return true;
            case "Pegi": g.Pegi = value; return true;
            case "URL": g.Url = value; return true;
            case "Description": g.Description = value; return true;
            case "Comments": g.Comments = value; return true;
            case "InstallPath": g.InstallPath = value; return true;
            case "Engine": g.Engine = value; return true;
            case "Version": g.Version = value; return true;
            case "GameMode": g.GameMode = value; return true;
            case "Platform": g.Platform = value; return true;
            case "Resolution": g.Resolution = value; return true;
            case "Framerate": g.Framerate = value; return true;
            case "AudioLanguages": g.AudioLanguages = value; return true;
            case "Subtitles": g.Subtitles = value; return true;
            case "SizeGb": g.SizeGb = value; return true;
            default: return SetCustom(g, tag, value);
        }
    }

    /// <summary>Accepts the enum name or the caption in the current language.</summary>
    public static bool TryParseStatus(string value, out CompletionStatus status)
    {
        foreach (var s in Enum.GetValues<CompletionStatus>())
        {
            if (s.ToString().Equals(value, StringComparison.OrdinalIgnoreCase) ||
                CatalogLabels.Current.Status(s).Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                status = s;
                return true;
            }
        }
        status = CompletionStatus.Backlog;
        return false;
    }

    private static bool SetCustom(Game g, string tag, string value)
    {
        if (value.Length == 0) g.CustomValues.Remove(tag);
        else g.CustomValues[tag] = value;
        return true;
    }

    private static bool TryInt(string s, Action<int> apply)
    {
        if (!int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return false;
        apply(v);
        return true;
    }

    private static bool TryDouble(string s, Action<double> apply)
    {
        if (!double.TryParse(s.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return false;
        apply(v);
        return true;
    }

    private static bool TryDate(string s, Action<DateTime?> apply)
    {
        if (!DateTime.TryParse(s.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var v) &&
            !DateTime.TryParse(s.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out v)) return false;
        apply(v.Date);
        return true;
    }
}
