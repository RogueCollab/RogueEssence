using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;
using Avalonia.Controls;
using RogueEssence.Dev.Views;
using System.Collections;
using Avalonia;
using System.Reactive.Subjects;

namespace RogueEssence.Dev
{
    public class MonsterIDEditor : Editor<MonsterID>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, MonsterID member, Type[] subGroupStack)
        {
            MonsterIDAttribute dataAtt = ReflectionExt.FindAttribute<MonsterIDAttribute>(attributes);

            {
                Avalonia.Controls.Grid innerPanel1 = getSharedRowPanel(2);

                TextBlock lblSpecies = new TextBlock();
                lblSpecies.Text = "Species:";
                lblSpecies.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblSpecies.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                innerPanel1.Children.Add(lblSpecies);
                innerPanel1.ColumnDefinitions[0].Width = new GridLength(46);
                lblSpecies.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                ComboBox cbSpecies = new ComboBox();
                ComboBox cbForms = new ComboBox();

                cbSpecies.VirtualizationMode = ItemVirtualizationMode.Simple;

                EntryDataIndex nameIndex = DataManager.Instance.DataIndices[DataManager.DataType.Monster];
                List<string> monsterKeys = nameIndex.GetOrderedKeys(false);
                int chosenSpecies = monsterKeys.IndexOf(member.Species);

                List<string> species = new List<string>();
                List<string> forms = new List<string>();

                if (dataAtt.InvalidSpecies)
                {
                    monsterKeys.Insert(0, "");
                    species.Add("**EMPTY**");
                    chosenSpecies++;
                }

                for (int ii = 0; ii < monsterKeys.Count; ii++)
                    species.Add(monsterKeys[ii] + ": " + nameIndex.Entries[monsterKeys[ii]].GetLocalString(false));

                var speciesSubject = new Subject<List<string>>();
                var formSubject = new Subject<List<string>>();

                cbSpecies.Bind(ComboBox.ItemsProperty, speciesSubject);
                speciesSubject.OnNext(species);
                cbSpecies.SelectedIndex = Math.Min(Math.Max(0, chosenSpecies), species.Count - 1);
                cbSpecies.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                {
                    loadForms(dataAtt, monsterKeys[cbSpecies.SelectedIndex], forms);
                    cbForms.SelectedIndex = -1;
                    cbForms.SelectedIndex = Math.Min(Math.Max(0, cbForms.SelectedIndex), forms.Count - 1);
                    formSubject.OnNext(forms);
                };

                innerPanel1.Children.Add(cbSpecies);
                cbSpecies.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                control.Children.Add(innerPanel1);


                Avalonia.Controls.Grid innerPanel2 = getSharedRowPanel(2);

                TextBlock lblForm = new TextBlock();
                lblForm.Margin = new Thickness(8, 0, 0, 0);
                lblForm.Text = "Form:";
                lblForm.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblForm.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                innerPanel2.Children.Add(lblForm);
                innerPanel2.ColumnDefinitions[0].Width = new GridLength(46);
                lblForm.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);


                cbForms.VirtualizationMode = ItemVirtualizationMode.Simple;
                int chosenForm = member.Form;

                loadForms(dataAtt, member.Species, forms);

                if (dataAtt.InvalidForm)
                    chosenForm++;

                cbForms.Bind(ComboBox.ItemsProperty, formSubject);
                formSubject.OnNext(forms);
                cbForms.SelectedIndex = Math.Min(Math.Max(0, chosenForm), forms.Count - 1);
                innerPanel2.Children.Add(cbForms);
                cbForms.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                control.Children.Add(innerPanel2);
            }

            Avalonia.Controls.Grid innerPanel3 = getSharedRowPanel(4);

            {
                TextBlock lblSkin = new TextBlock();
                lblSkin.Margin = new Thickness(8, 0, 0, 0);
                lblSkin.Text = "Skin:";
                lblSkin.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblSkin.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                innerPanel3.Children.Add(lblSkin);
                innerPanel3.ColumnDefinitions[0].Width = new GridLength(46);
                lblSkin.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

                ComboBox cbSkin = new ComboBox();
                cbSkin.VirtualizationMode = ItemVirtualizationMode.Simple;

                List<string> items = new List<string>();
                if (dataAtt.InvalidSkin)
                    items.Add("**EMPTY**");

                int chosenIndex = 0;
                foreach (string key in DataManager.Instance.DataIndices[DataManager.DataType.Skin].Entries.Keys)
                {
                    if (key == member.Skin)
                        chosenIndex = items.Count;

                    items.Add(key + ": " + DataManager.Instance.DataIndices[DataManager.DataType.Skin].Entries[key].GetLocalString(false));
                }

                var subject = new Subject<List<string>>();
                cbSkin.Bind(ComboBox.ItemsProperty, subject);
                subject.OnNext(items);
                cbSkin.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
                innerPanel3.Children.Add(cbSkin);
                cbSkin.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
            }

            {
                TextBlock lblGender = new TextBlock();
                lblGender.Margin = new Thickness(8, 0, 0, 0);
                lblGender.Text = "Gender:";
                lblGender.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblGender.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                innerPanel3.Children.Add(lblGender);
                innerPanel3.ColumnDefinitions[2].Width = new GridLength(46);
                lblGender.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);

                ComboBox cbGender = new ComboBox();
                cbGender.VirtualizationMode = ItemVirtualizationMode.Simple;
                int chosenIndex = (int)member.Gender;

                List<string> items = new List<string>();
                if (dataAtt.InvalidGender)
                {
                    items.Add("**EMPTY**");
                    chosenIndex++;
                }

                for (int ii = 0; ii <= (int)Gender.Female; ii++)
                    items.Add(((Gender)ii).ToLocal());

                var subject = new Subject<List<string>>();
                cbGender.Bind(ComboBox.ItemsProperty, subject);
                subject.OnNext(items);
                cbGender.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
                innerPanel3.Children.Add(cbGender);
                cbGender.SetValue(Avalonia.Controls.Grid.ColumnProperty, 3);
            }

            control.Children.Add(innerPanel3);
        }

        public override MonsterID SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            MonsterID result = new MonsterID();
            MonsterIDAttribute dataAtt = ReflectionExt.FindAttribute<MonsterIDAttribute>(attributes);

            int controlIndex = 0;

            Avalonia.Controls.Grid innerControl1 = (Avalonia.Controls.Grid)control.Children[controlIndex];

            int innerControlIndex = 0;
            innerControlIndex++;
            ComboBox cbSpecies = (ComboBox)innerControl1.Children[innerControlIndex];

            List<string> monsterKeys = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetOrderedKeys(false);
            if (dataAtt.InvalidSpecies)
                monsterKeys.Insert(0, "");
            result.Species = monsterKeys[cbSpecies.SelectedIndex];

            controlIndex++;
            Avalonia.Controls.Grid innerControl2 = (Avalonia.Controls.Grid)control.Children[controlIndex];
            innerControlIndex = 0;
            innerControlIndex++;
            ComboBox cbForm = (ComboBox)innerControl2.Children[innerControlIndex];
            result.Form = cbForm.SelectedIndex;
            if (dataAtt.InvalidForm)
                result.Form--;

            controlIndex++;
            Avalonia.Controls.Grid innerControl3 = (Avalonia.Controls.Grid)control.Children[controlIndex];

            innerControlIndex = 0;
            innerControlIndex++;
            ComboBox cbSkin = (ComboBox)innerControl3.Children[innerControlIndex];

            List<string> skinKeys = DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetOrderedKeys(false);
            result.Skin = skinKeys[cbSkin.SelectedIndex];

            innerControlIndex++;
            innerControlIndex++;
            ComboBox cbGender = (ComboBox)innerControl3.Children[innerControlIndex];
            result.Gender = (Gender)cbGender.SelectedIndex;
            if (dataAtt.InvalidGender)
                result.Gender--;

            return result;
        }

        public override string GetString(MonsterID obj, Type type, object[] attributes)
        {
            string name = "???";

            if (!String.IsNullOrEmpty(obj.Species))
            {
                MonsterData data = DataManager.Instance.GetMonster(obj.Species);
                if (obj.Form > -1)
                {
                    BaseMonsterForm form = data.Forms[obj.Form];
                    name = form.FormName.ToLocal();
                }
                else
                    name = data.Name.ToLocal();
            }
            if (!String.IsNullOrEmpty(obj.Skin))
            {
                SkinData data = DataManager.Instance.GetSkin(obj.Skin);
                name = String.Format("[{0}] ", data.Name.ToLocal()) + name;
            }

            if (obj.Gender != Gender.Unknown)
            {
                char genderChar = '\0';
                switch (obj.Gender)
                {
                    case Gender.Male:
                        genderChar = '\u2642';
                        break;
                    case Gender.Female:
                        genderChar = '\u2640';
                        break;
                    case Gender.Genderless:
                        genderChar = '-';
                        break;
                }

                if (genderChar != '\0' && name[name.Length - 1] != genderChar)
                    name += genderChar;
            }
            return name;
        }

        private void loadForms(MonsterIDAttribute dataAtt, string species, List<string> forms)
        {
            forms.Clear();
            MonsterData monsterData = DataManager.Instance.GetMonster(species);

            if (dataAtt.InvalidForm)
                forms.Add("**EMPTY**");

            for (int ii = 0; ii < monsterData.Forms.Count; ii++)
                forms.Add(monsterData.Forms[ii].FormName.ToLocal());

        }
    }
}
