using System;
using RogueEssence.Data;

namespace RogueEssence.Dev.ViewModels;

using Dev.Services;

public class DevEditPageViewModel : EditorPageViewModel
{
    public void btnEditEmote_Click()
    {
        OpenList(DataManager.DataType.Emote, DataManager.Instance.GetEmote, () => { return new EmoteData(); });
    }

    public void btnEditElement_Click()
    {
        OpenList(DataManager.DataType.Element, DataManager.Instance.GetElement, () => { return new ElementData(); });
    }

    public void btnEditGrowthGroup_Click()
    {
        OpenList(DataManager.DataType.GrowthGroup, DataManager.Instance.GetGrowth, () => { return new GrowthData(); });
    }

    public void btnEditSkillGroup_Click()
    {
        OpenList(DataManager.DataType.SkillGroup, DataManager.Instance.GetSkillGroup,
            () => { return new SkillGroupData(); });
    }

    public void btnEditRank_Click()
    {
        OpenList(DataManager.DataType.Rank, DataManager.Instance.GetRank, () => { return new RankData(); });
    }

    public void btnEditSkin_Click()
    {
        OpenList(DataManager.DataType.Skin, DataManager.Instance.GetSkin, () => { return new SkinData(); });
    }

    public void btnEditAI_Click()
    {
        OpenList(DataManager.DataType.AI, DataManager.Instance.GetAITactic, () => { return new AITactic(); });
    }


    private delegate IEntryData GetEntry(string entryNum);

    private delegate IEntryData CreateEntry();

    private void OpenList(DataManager.DataType dataType, GetEntry entryOp, CreateEntry createOp)
    {
        lock (GameBase.lockObj)
        {
            // DataListForm dataListForm = new DataListForm();
            // DataListFormViewModel choices = createChoices(dataListForm, dataType, entryOp, createOp);
            // DataOpContainer reindexOp = createReindexOp(dataType, choices);
            // DataOpContainer resaveFileOp = createResaveFileDiffOp(dataType, choices, entryOp, false);
            // DataOpContainer resaveDiffOp = createResaveFileDiffOp(dataType, choices, entryOp, true);
            // choices.SetOps(reindexOp, resaveFileOp, resaveDiffOp);
            //
            // dataListForm.DataContext = choices;
            // dataListForm.SetListContextMenu(CreateContextMenu(choices));
            // dataListForm.Show();
        }
    }
    
    // public override string? Title => "Dev Edit";

    public DevEditPageViewModel(NodeFactory nodeFactory, PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService,
        NodeBase node) : base(nodeFactory, pageFactory, tabEvents, dialogService)
    {
        var n = node as DataItemNode;
        var nn = node.Parent as DataRootNode;
        var dt = nn.DataType;
    }

    // public DevEditPageViewModel() : base(new PageFactory(new DesignServiceProvider()),
    //     new TabEvents(new PageFactory(new DesignServiceProvider())), new DialogService())
    // {
    //     // Title = "Dev Edit";
    // }
}