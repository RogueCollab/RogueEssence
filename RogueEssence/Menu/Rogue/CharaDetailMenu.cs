using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class CharaDetailMenu : BaseSettingsMenu
    {
        private string species;
        private int origFormSetting;
        private string origSkinSetting;
        private Gender origGenderSetting;
        private int origIntrinsicSetting;

        private CharaChoiceMenu baseMenu;

        private List<int> legalForms;
        private List<string> legalSkins;
        private List<Gender> legalGenders;
        private List<int> legalIntrinsics;
        
        public CharaDetailMenu(string species, CharaChoiceMenu baseMenu)
        {
            this.species = species;
            this.baseMenu = baseMenu;
            origFormSetting = baseMenu.FormSetting;
            origSkinSetting = baseMenu.SkinSetting;
            origGenderSetting = baseMenu.GenderSetting;
            origIntrinsicSetting = baseMenu.IntrinsicSetting;

            MonsterData dex = DataManager.Instance.GetMonster(species);
            //individual tactics
            List<MenuSetting> totalChoices = new List<MenuSetting>();
            totalChoices.Add(CreateFormChoices(dex));
            totalChoices.Add(CreateSkinChoices(dex));
            totalChoices.Add(CreateGenderChoices(dex));
            totalChoices.Add(CreateIntrinsicChoices(dex));

            baseMenu.UpdateExtraInfo();

            Initialize(new Loc(136, 128), 176, Text.FormatKey("MENU_CHARA_DETAILS"), totalChoices.ToArray());
        }

        private void confirmAction()
        {
            SaveCurrentChoices();
            origFormSetting = baseMenu.FormSetting;
            origSkinSetting = baseMenu.SkinSetting;
            origGenderSetting = baseMenu.GenderSetting;
            origIntrinsicSetting = baseMenu.IntrinsicSetting;
        }

        protected override void Canceled()
        {
            baseMenu.FormSetting = origFormSetting;
            baseMenu.SkinSetting = origSkinSetting;
            baseMenu.GenderSetting = origGenderSetting;
            baseMenu.IntrinsicSetting = origIntrinsicSetting;
            baseMenu.UpdateExtraInfo();
            base.Canceled();
        }

        protected override void UpdateKeys(InputManager input)
        {
            //when entering the customization choice, limit the actual choice to the defaults
            //allow the player to choose any combination of traits within a given species
            //however, when switching between species, the settings are kept even if invalid for new species, just display legal substitutes in those cases
            if (input.JustPressed(FrameInput.InputType.SortItems))
                Canceled();
            else
                base.UpdateKeys(input);
        }

        protected override void SettingChanged(int index)
        {
            //if form is changed, we may need to update the intrinsic listings
            if (index == 0)
            {
                baseMenu.FormSetting = TotalChoices[0].CurrentChoice - 1;

                MonsterData dex = DataManager.Instance.GetMonster(species);

                List<MenuSetting> totalChoices = new List<MenuSetting>();
                //limit color setting
                TotalChoices[1] = CreateSkinChoices(dex);

                //limit gender setting
                TotalChoices[2] = CreateGenderChoices(dex);
                
                //limit intrinsic
                TotalChoices[3] = CreateIntrinsicChoices(dex);

                SetChoices(TotalChoices);
            }
            else if (index == 1)
                baseMenu.SkinSetting = legalSkins[TotalChoices[1].CurrentChoice];
            else if (index == 2)
                baseMenu.GenderSetting = TotalChoices[2].CurrentChoice == 0 ? Gender.Unknown : legalGenders[TotalChoices[2].CurrentChoice - 1];
            else if (index == 3)
                baseMenu.IntrinsicSetting = TotalChoices[3].CurrentChoice == 0 ? -1 : legalIntrinsics[TotalChoices[3].CurrentChoice - 1];

            baseMenu.UpdateExtraInfo();

            base.SettingChanged(index);
        }

        private MenuSetting CreateFormChoices(MonsterData dex)
        {
            legalForms = GetPossibleForms(dex);
            baseMenu.FormSetting = origFormSetting;
            if (baseMenu.FormSetting > legalForms.Count)
                baseMenu.FormSetting = legalForms.Count - 1;

            List<string> choices = new List<string>();
            choices.Add(Text.FormatKey("MENU_START_RANDOM"));
            for (int jj = 0; jj < legalForms.Count; jj++)
                choices.Add(dex.Forms[legalForms[jj]].FormName.ToLocal());

            return new MenuSetting(Text.FormatKey("MENU_CHARA_FORM"), 48, 72, choices, baseMenu.FormSetting + 1, origFormSetting + 1, confirmAction);
        }
        private MenuSetting CreateSkinChoices(MonsterData dex)
        {
            int formSettingIndex = baseMenu.FormSetting > -1 ? legalForms[baseMenu.FormSetting] : -1;
            if (formSettingIndex > -1)
                legalSkins = dex.Forms[formSettingIndex].GetPossibleSkins();
            else
            {
                legalSkins = new List<string>() { DataManager.Instance.DefaultSkin };
                foreach(string key in DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetOrderedKeys(true))
                {
                    if (DataManager.Instance.GetSkin(key).Challenge)
                        legalSkins.Add(key);
                }
            }
            List<string> choices = new List<string>();
            int chosenIndex = 0;
            int origChosenIndex = -1;
            for (int jj = 0; jj < legalSkins.Count; jj++)
            {
                choices.Add(DataManager.Instance.GetSkin(legalSkins[jj]).GetColoredName());
                if (legalSkins[jj] == baseMenu.SkinSetting)
                    chosenIndex = jj;
                if (legalSkins[jj] == origSkinSetting)
                    origChosenIndex = jj;
            }

            return new MenuSetting(Text.FormatKey("MENU_CHARA_SKIN"), 48, 72, choices, chosenIndex, origChosenIndex, confirmAction);
        }
        private MenuSetting CreateGenderChoices(MonsterData dex)
        {
            int genderFormIndex = GetGenderFormIndex(dex, baseMenu.FormSetting, legalForms);
            legalGenders = genderFormIndex > -1 ? dex.Forms[genderFormIndex].GetPossibleGenders() : new List<Gender> { Gender.Male, Gender.Female };
            baseMenu.GenderSetting = LimitGender(dex, genderFormIndex, baseMenu.GenderSetting);
            List<string> choices = new List<string>();
            choices.Add(Text.FormatKey("MENU_START_RANDOM"));
            int chosenIndex = -1;
            int origChosenIndex = -2;
            for (int jj = 0; jj < legalGenders.Count; jj++)
            {
                choices.Add(legalGenders[jj].ToLocal());
                if (legalGenders[jj] == baseMenu.GenderSetting)
                    chosenIndex = jj;
                if (legalGenders[jj] == origGenderSetting)
                    origChosenIndex = jj;
            }
            if (Gender.Unknown == origGenderSetting)
                origChosenIndex = -1;

            return new MenuSetting(Text.FormatKey("MENU_CHARA_GENDER"), 48, 72, choices, chosenIndex + 1, origChosenIndex + 1, confirmAction);
        }
        private MenuSetting CreateIntrinsicChoices(MonsterData dex)
        {
            int intrinsicFormIndex = GetIntrinsicFormIndex(dex, baseMenu.FormSetting, legalForms);
            legalIntrinsics = intrinsicFormIndex > -1 ? dex.Forms[intrinsicFormIndex].GetPossibleIntrinsicSlots() : new List<int> { 0, 1, 2 };
            baseMenu.IntrinsicSetting = LimitIntrinsic(dex, intrinsicFormIndex, baseMenu.IntrinsicSetting);
            List<string> choices = new List<string>();
            choices.Add(Text.FormatKey("MENU_START_RANDOM"));
            int chosenIndex = -1;
            int origChosenIndex = -2;

            for (int jj = 0; jj < legalIntrinsics.Count; jj++)
            {
                if (intrinsicFormIndex == -1)
                {
                    choices.Add(Text.FormatKey("MENU_CHARA_INTRINSIC", legalIntrinsics[jj] + 1));
                }
                else
                {
                    if (legalIntrinsics[jj] == 0)
                        choices.Add(DataManager.Instance.GetIntrinsic(dex.Forms[intrinsicFormIndex].Intrinsic1).GetColoredName());
                    else if (legalIntrinsics[jj] == 1)
                        choices.Add(DataManager.Instance.GetIntrinsic(dex.Forms[intrinsicFormIndex].Intrinsic2).GetColoredName());
                    else
                        choices.Add(DataManager.Instance.GetIntrinsic(dex.Forms[intrinsicFormIndex].Intrinsic3).GetColoredName());
                }


                if (legalIntrinsics[jj] == baseMenu.IntrinsicSetting)
                    chosenIndex = jj;
                if (legalIntrinsics[jj] == origIntrinsicSetting)
                    origChosenIndex = jj;
            }
            if (origIntrinsicSetting == -1)
                origChosenIndex = -1;

            return new MenuSetting(Text.FormatKey("MENU_CHARA_DETAIL_INTRINSIC"), 48, 72, choices, chosenIndex + 1, origChosenIndex + 1, confirmAction);
        }

        public static List<int> GetPossibleForms(MonsterData dex)
        {
            List<int> forms = new List<int>();
            if (dex == null)
                return forms;

            for (int ii = 0; ii < dex.Forms.Count; ii++)
            {
                //check to see if the form is allowed, or if it's only mid-dungeon
                if (!dex.Forms[ii].Temporary && (DiagManager.Instance.DevMode || dex.Forms[ii].Released))
                    forms.Add(ii);
                else if (ii == 0)//always add default form
                    forms.Add(ii);
            }

            return forms;
        }

        public static int GetGenderFormIndex(MonsterData dex, int formSlot, List<int> possibleForms)
        {
            if (dex == null)
                return -1;
            if (formSlot == -1)
            {
                //check to see if all possible forms have the same gender ratio
                bool sameGender = true;
                List<Gender> possible1 = dex.Forms[possibleForms[0]].GetPossibleGenders();
                for (int ii = 1; ii < possibleForms.Count; ii++)
                {
                    List<Gender> possible2 = dex.Forms[possibleForms[ii]].GetPossibleGenders();
                    if (possible1.Count != possible2.Count)
                    {
                        sameGender = false;
                        break;
                    }
                    for (int jj = 0; jj < possible1.Count; jj++)
                    {
                        if (possible1[jj] != possible2[jj])
                        {
                            sameGender = false;
                            break;
                        }
                    }
                    if (!sameGender)
                        break;
                }
                if (sameGender)
                    return possibleForms[0];
                return -1;
            }
            else
                return possibleForms[formSlot];
        }

        /// <summary>
        /// Gets the form that is used when choosing intrinsic
        /// </summary>
        /// <param name="dex"></param>
        /// <param name="formSlot"></param>
        /// <param name="possibleForms"></param>
        /// <returns>The actual form index in the monster form list, not the one out of the possible</returns>
        public static int GetIntrinsicFormIndex(MonsterData dex, int formSlot, List<int> possibleForms)
        {
            if (dex == null)
                return -1;
            if (formSlot == -1)
            {
                //check to see if all possible forms have the same intrinsic
                bool sameIntrinsic = true;
                List<int> possible1 = dex.Forms[possibleForms[0]].GetPossibleIntrinsicSlots();
                for (int ii = 1; ii < possibleForms.Count; ii++)
                {
                    List<int> possible2 = dex.Forms[possibleForms[ii]].GetPossibleIntrinsicSlots();

                    if (possible1.Count != possible2.Count)
                    {
                        sameIntrinsic = false;
                        break;
                    }
                    for (int jj = 0; jj < possible1.Count; jj++)
                    {
                        if (possible1[jj] != possible2[jj])
                        {
                            sameIntrinsic = false;
                            break;
                        }
                    }
                    if (!sameIntrinsic)
                        break;

                }
                if (sameIntrinsic)
                    return possibleForms[0];
                return -1;
            }
            else
                return possibleForms[formSlot];
        }

        public static Gender LimitGender(MonsterData dex, int formIndex, Gender gender)
        {

            if (formIndex > -1)
            {
                if (gender != Gender.Unknown)
                {
                    List<Gender> possibles = dex.Forms[formIndex].GetPossibleGenders();
                    if (possibles.Count == 1)
                        return possibles[0];
                    else
                    {
                        //m/f choice  = give male, female, unknown, as a choice
                        //if by now the gender has not been restricted, and the currently chosen gender is "genderless",
                        //we must change it to "unknown"
                        if (gender == Gender.Genderless)
                            gender = Gender.Unknown;
                    }
                }
            }
            else if (gender == Gender.Genderless)
                gender = Gender.Unknown;

            return gender;
        }

        public static int LimitIntrinsic(MonsterData dex, int formIndex, int intrinsicIndex)
        {
            if (formIndex > -1 && intrinsicIndex > -1)
            {
                List<int> possibles = dex.Forms[formIndex].GetPossibleIntrinsicSlots();
                if (possibles.Contains(intrinsicIndex))
                    return intrinsicIndex;
                return 0;
            }
            return intrinsicIndex;
        }
    }
}
