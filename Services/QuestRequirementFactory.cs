using KushBot.DataClasses;
using System;

namespace KushBot.Services;

public class QuestRequirementFactory
{
    public QuestRequirement Create(QuestRequirementType type, string value)
    {
        return type switch
        {
            QuestRequirementType.Win => new WinQuestRequirement
            {
                Value = value,
                Type = type
            },

            QuestRequirementType.Lose => new LoseQuestRequirement
            {
                Value = value,
                Type = type
            },

            QuestRequirementType.BapsX => new BapsXQuestRequirement
            {
                Value = value,
                Type = type,
            },

            QuestRequirementType.ModifierX => new ModifierXQuestRequirement
            {
                Value = value,
                Type = type,
            },

            QuestRequirementType.Command => new CommandQuestRequirement
            {
                Value = value,
                Type = type,
            },

            QuestRequirementType.Chain => new ChainQuestRequirement
            {
                Value = value,
                Type = type,
            },

            QuestRequirementType.Count => new CountQuestRequirement
            {
                Value = value,
                Type = type,
            },

            _ => throw new ArgumentException("Invalid QuestRequirementType", nameof(type)),
        };
    }
}
