using System.Text.Json.Serialization;

namespace Api.Reporting;

// Travels as text in JSON ("AssezBien"); the front end maps it to the French label.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Mention
{
    Ajourne,
    Passable,
    AssezBien,
    Bien,
    TresBien,
    Excellent,
}

public static class MentionRules
{
    // Lower bounds are inclusive: 10 is "Passable", 12 is "Assez bien", and so on.
    public static Mention For(decimal average) => average switch
    {
        < 10m => Mention.Ajourne,
        < 12m => Mention.Passable,
        < 14m => Mention.AssezBien,
        < 16m => Mention.Bien,
        < 18m => Mention.TresBien,
        _ => Mention.Excellent,
    };
}
