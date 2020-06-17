namespace RogueEssence.Data
{
    public interface IDescribedData : IEntryData
    {
        LocalText Desc { get; set; }
    }
    public interface IEntryData
    {
        LocalText Name { get; set; }
        bool Released { get; }
        string Comment { get; set; }

        EntrySummary GenerateEntrySummary();
    }
}
