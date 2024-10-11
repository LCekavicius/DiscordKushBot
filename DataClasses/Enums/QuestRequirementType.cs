namespace KushBot.DataClasses;

public enum QuestRequirementType
{
    BapsX, // e.g. Flip 700 baps in one flip
    ModifierX, // e.g. get 3.0 as bet modifier
    Command, // e.g. use kush moteris
    Chain, // e.g. do X 3 times in a row
    //Quest finishers, should always be at the end of the list (maybe rework this later?)
    Win,
    Lose,
    Count // e.g. do X 3 times
}
