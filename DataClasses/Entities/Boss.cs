using KushBot.DataClasses.enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot.DataClasses;

public class Boss
{
    [Key]
    public int Id { get; init; }
    public ulong MessageId { get; set; }
    public required int Level { get; init; }
    public required int Health { get; init; }
    public required int ParticipantSlots { get; init; }
    public required DateTime StartDate { get; init; }
    public int BossBaseIndex { get; set; }
    public long AbilitiesValue { get; private set; }

    public List<BossParticipant> Participants { get; init; }
    
    [NotMapped]
    public BossAbilities Abilities
    {
        get => (BossAbilities)AbilitiesValue;
        set => AbilitiesValue = (long)value;
    }

    [NotMapped] public RarityType Rarity { get => (RarityType)(1 + Level / 10); }
}
