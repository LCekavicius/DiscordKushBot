using System.Collections.Generic;

namespace KushBot.Global;

public sealed record Pet(string Name, string NormalizedName, PetType Type);

public static class Pets
{
    public static Pet SuperNed { get; } = new Pet("Superned", "superned", PetType.SuperNed);
    public static Pet Pinata { get; } = new Pet("Pinata", "pinata", PetType.Pinata);
    public static Pet Goran { get; } = new Pet("Goran Jelic", "goran-jelic", PetType.Goran);
    public static Pet Maybich { get; } = new Pet("Maybich", "maybich", PetType.Maybich);
    public static Pet Jew { get; } = new Pet("Jew", "jew", PetType.Jew);
    public static Pet TylerJuan { get; } = new Pet("TylerJuan", "tylerjuan", PetType.TylerJuan);

    public static List<Pet> All = new List<Pet>() { SuperNed, Pinata, Goran, Maybich, Jew, TylerJuan };
}


public enum PetType
{
    SuperNed,
    Pinata,
    Goran,
    Maybich,
    Jew,
    TylerJuan,
}