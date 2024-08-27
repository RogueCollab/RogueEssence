using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;
using ReactiveUI;
using System.Collections.ObjectModel;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class ModConfigViewModel : ViewModelBase
    {
        public ModConfigViewModel(ModHeader header)
        {
            Name = header.Name;
            Namespace = header.Namespace;
            Author = header.Author;
            Description = header.Description;
            UUID = header.UUID.ToString().ToUpper();
            Version = header.Version.ToString();
            GameVersion = header.GameVersion.ToString();

            ModTypes = new ObservableCollection<string>();
            for (int ii = 0; ii < (int)PathMod.ModType.Count; ii++)
                ModTypes.Add(((PathMod.ModType)ii).ToLocal());
            ChosenModType = (int)header.ModType;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            Relationships = new CollectionBoxViewModel(form, new StringConv(typeof(RelatedMod), new object[0]));
            Relationships.OnEditItem += Relationships_EditItem;
            Relationships.LoadFromList(header.Relationships);
        }

        private string name;
        public string Name
        {
            get => name;
            set => this.SetIfChanged(ref name, value);
        }

        private string author;
        public string Author
        {
            get => author;
            set => this.SetIfChanged(ref author, value);
        }

        private string description;
        public string Description
        {
            get => description;
            set => this.SetIfChanged(ref description, value);
        }

        private string editNamespace;
        public string Namespace
        {
            get => editNamespace;
            set => this.SetIfChanged(ref editNamespace, value);
        }

        private string uuid;
        public string UUID
        {
            get => uuid;
            set => this.SetIfChanged(ref uuid, value);
        }


        private string version;
        public string Version
        {
            get => version;
            set => this.SetIfChanged(ref version, value);
        }


        private string gameVersion;
        public string GameVersion
        {
            get => gameVersion;
            set => this.SetIfChanged(ref gameVersion, value);
        }

        public ObservableCollection<string> ModTypes { get; }

        private int chosenModType;
        public int ChosenModType
        {
            get => chosenModType;
            set => this.SetIfChanged(ref chosenModType, value);
        }

        public CollectionBoxViewModel Relationships { get; set; }


        public void Relationships_EditItem(int index, object element, bool advancedEdit, CollectionBoxViewModel.EditElementOp op)
        {
            string elementName = "Relationship[" + index + "]";
            DataEditForm frmData = new DataEditRootForm();
            frmData.Title = DataEditor.GetWindowTitle("Relationship", elementName, element, typeof(RelatedMod), new object[0]);

            //TODO: make this a member and reference it that way
            DataEditor.LoadClassControls(frmData.ControlPanel, "Relationship", null, elementName, typeof(RelatedMod), new object[0], element, true, new Type[0], advancedEdit);
            DataEditor.TrackTypeSize(frmData, typeof(RelatedMod));

            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, typeof(RelatedMod), new object[0], true, new Type[0], advancedEdit);

                bool itemExists = false;

                List<RelatedMod> relations = Relationships.GetList<List<RelatedMod>>();
                for (int ii = 0; ii < relations.Count; ii++)
                {
                    if (ii != index)
                    {
                        if (relations[ii].UUID == ((RelatedMod)element).UUID)
                            itemExists = true;
                    }
                }

                if (String.IsNullOrEmpty(((RelatedMod)element).Namespace))
                {
                    await MessageBox.Show(frmData, "Related mod needs a namespace!", "Namespace needed.", MessageBox.MessageBoxButtons.Ok);
                    return false;
                }
                if (itemExists)
                {
                    await MessageBox.Show(frmData, "Cannot add duplicate entries.", "Entry with UUID already exists.", MessageBox.MessageBoxButtons.Ok);
                    return false;
                }
                else
                {
                    op(index, element);
                    return true;
                }
            };

            //this.RegisterChild(frmData);
            frmData.Show();
        }

        public RelatedMod[] GetRelationshipArray()
        {
            List<RelatedMod> relationships = Relationships.GetList<List<RelatedMod>>();
            return relationships.ToArray();
        }

        public void btnRegenUUID_Click()
        {
            UUID = Guid.NewGuid().ToString().ToUpper();
        }
    }
}
