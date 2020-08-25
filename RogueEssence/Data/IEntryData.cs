namespace RogueEssence.Data
{
    public interface IDescribedData : IEntryData
    {
        LocalText Desc { get; set; }
    }

    /// <summary>
    /// All classes that represent indexed data such as monsters, items, etc. implement this class.
    /// This is used for editor lists that need to load the names of all monsters, items, etc. without actually loading all data files.
    /// Also for localization.
    /// </summary>
    public interface IEntryData
    {
        LocalText Name { get; set; }
        bool Released { get; }
        string Comment { get; set; }

        EntrySummary GenerateEntrySummary();
    }
}
