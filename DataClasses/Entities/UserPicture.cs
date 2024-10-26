using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot.DataClasses;

[Index(nameof(Path), nameof(OwnerId), IsUnique = true)]
public class UserPicture
{
    [Key]
    public int Id { get; init; }
    public ulong OwnerId { get; init; }
    public string Path { get; init; }
    public KushBotUser Owner { get; init; }

    [NotMapped] public bool IsGif { get => Path.StartsWith("gif"); }
    [NotMapped] public int? Representation { get => GetRepresentation(); }

    private int? GetRepresentation()
    {
        int.TryParse(System.IO.Path.GetFileNameWithoutExtension(this.Path), out var value);
        return value;
    }
    
    public UserPicture() { }
    public UserPicture(ulong ownerId, string path)
    {
        OwnerId = ownerId;
        Path = path;
    }
}
