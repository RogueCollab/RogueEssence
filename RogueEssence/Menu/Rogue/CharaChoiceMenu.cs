using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;
using RogueEssence.Script;

namespace RogueEssence.Menu
{
    public class CharaChoiceMenu : MultiPageMenu
    {
        private static int defaultChoice;

        private const int SLOTS_PER_PAGE = 12;

        private string team;
        private int chosenDest;
        public int FormSetting;
        public int SkinSetting;
        public Gender GenderSetting;
        public int IntrinsicSetting;
        private SummaryMenu titleMenu;
        private CharaSummary infoMenu;
        public SpeakerPortrait Portrait;
        private List<int> startChars;
        private ulong? seed;

        public CharaChoiceMenu(string teamName, int chosenDungeon, ulong? seed)
        {
            GenderSetting = Gender.Unknown;
            SkinSetting = 0;
            IntrinsicSetting = -1;
            FormSetting = -1;
            
            startChars = new List<int>();
            for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Monster].Count; ii++)
            {
                if (DiagManager.Instance.DevMode)
                    startChars.Add(ii);
                //TODO: String Assets
                else if (DataManager.Instance.Save.GetRogueUnlock(ii) && DataManager.Instance.DataIndices[DataManager.DataType.Monster].Entries[ii.ToString()].Released)
                    startChars.Add(ii);
                else if (DataManager.Instance.StartChars.FindIndex(mon => mon.mon.Species == ii) > -1)
                    startChars.Add(ii);
            }


            List<MenuChoice> flatChoices = new List<MenuChoice>();
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_START_RANDOM"), () => { choose(startChars[MathUtils.Rand.Next(startChars.Count)]); }));
            for (int ii = 0; ii < startChars.Count; ii++)
            {
                int startChar = startChars[ii];
                //TODO: String Assets
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.DataIndices[DataManager.DataType.Monster].Entries[startChar.ToString()].GetColoredName(), () => { choose(startChar); }));
            }

            int actualChoice = Math.Min(Math.Max(0, defaultChoice), flatChoices.Count - 1);
            IChoosable[][] box = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            int totalSlots = SLOTS_PER_PAGE;
            if (box.Length == 1)
                totalSlots = box[0].Length;

            team = teamName;
            chosenDest = chosenDungeon;
            this.seed = seed;

            Portrait = new SpeakerPortrait(new MonsterID(), new EmoteStyle(0), new Loc(200, 64), true);

            infoMenu = new CharaSummary(new Rect(new Loc(152, 128), new Loc(136, LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));

            int startPage = actualChoice / SLOTS_PER_PAGE;
            int startIndex = actualChoice % SLOTS_PER_PAGE;

            Initialize(new Loc(16, 16), 112, Text.FormatKey("MENU_CHARA_CHOICE_TITLE"), box, startIndex, startPage, totalSlots, false, -1);

            titleMenu = new SummaryMenu(Rect.FromPoints(new Loc(Bounds.End.X + 8, 16), new Loc(GraphicsManager.ScreenWidth - 8, 16 + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));
            MenuText title = new MenuText(Text.FormatKey("MENU_START_TEAM", team), new Loc(titleMenu.Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            title.Color = TextTan;
            titleMenu.Elements.Add(title);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            titleMenu.Draw(spriteBatch);

            if (Portrait.Speaker.Species > 0)
                Portrait.Draw(spriteBatch, new Loc());

            if (!Inactive)
                infoMenu.Draw(spriteBatch);
        }

        protected override void ChoiceChanged()
        {
            defaultChoice = CurrentChoiceTotal;
            UpdateExtraInfo();
            base.ChoiceChanged();
        }

        private void choose(int choice)
        {
            MenuManager.Instance.AddMenu(new NicknameMenu((string name) =>
            {
                MenuManager.Instance.ClearMenus();
                start(choice, name);
            }, () => { }), false);
        }


        protected override void UpdateKeys(InputManager input)
        {
            //when entering the customization choice, limit the actual choice to the defaults
            //allow the player to choose any combination of traits within a given species
            //however, when switching between species, the settings are kept even if invalid for new species, just display legal substitutes in those cases
            if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                int totalChoice = CurrentChoiceTotal;
                if (totalChoice > 0)
                {
                    GameManager.Instance.SE("Menu/Confirm");
                    CharaDetailMenu menu = new CharaDetailMenu(totalChoice > 0 ? startChars[totalChoice - 1] : -1, this);
                    MenuManager.Instance.AddMenu(menu, true);
                }
                else//TODO: allow editing on the random spot
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else
                base.UpdateKeys(input);
        }

        public void UpdateExtraInfo()
        {
            int totalChoice = CurrentChoiceTotal;
            
            if (totalChoice > 0)
            {
                int species = startChars[totalChoice - 1];
                MonsterData monsterData = DataManager.Instance.GetMonster(species);
                int formSlot = FormSetting;
                List<int> forms = CharaDetailMenu.GetPossibleForms(monsterData);
                if (formSlot >= forms.Count)
                    formSlot = forms.Count - 1;
                int formIndex = formSlot > -1 ? forms[formSlot] : -1;
                int genderFormIndex = CharaDetailMenu.GetGenderFormIndex(monsterData, formSlot, forms);
                Gender gender = CharaDetailMenu.LimitGender(monsterData, genderFormIndex, GenderSetting);
                int intrinsicFormIndex = CharaDetailMenu.GetIntrinsicFormIndex(monsterData, formSlot, forms);
                int intrinsic = CharaDetailMenu.LimitIntrinsic(monsterData, intrinsicFormIndex, IntrinsicSetting);

                Portrait.Speaker = new MonsterID(startChars[totalChoice - 1], formIndex > -1 ? formIndex : 0, SkinSetting, gender);
                
                string formString = "";
                if (formIndex > -1)
                    formString = monsterData.Forms[formIndex].FormName.ToLocal();

                string intrinsicString = "";
                if (intrinsic > -1)
                {
                    if (intrinsicFormIndex == -1)
                    {
                        intrinsicString = Text.FormatKey("MENU_CHARA_INTRINSIC", intrinsic);
                    }
                    else
                    {
                        if (intrinsic == 0)
                            intrinsicString = DataManager.Instance.GetIntrinsic(monsterData.Forms[intrinsicFormIndex].Intrinsic1).GetColoredName();
                        else if (intrinsic == 1)
                            intrinsicString = DataManager.Instance.GetIntrinsic(monsterData.Forms[intrinsicFormIndex].Intrinsic2).GetColoredName();
                        else
                            intrinsicString = DataManager.Instance.GetIntrinsic(monsterData.Forms[intrinsicFormIndex].Intrinsic3).GetColoredName();
                    }
                }
                infoMenu.SetDetails(formString, SkinSetting, gender, intrinsicString);
            }
            else
            {
                Portrait.Speaker = new MonsterID();
                infoMenu.SetDetails("", -1, Gender.Unknown, "");
            }
            
        }


        private void start(int choice, string name)
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = Begin(choice, name);
        }
        
        public IEnumerator<YieldInstruction> Begin(int choice, string name)
        {
            GameManager.Instance.BGM("", true);
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            GameProgress save = new RogueProgress(seed.HasValue ? seed.Value : MathUtils.Rand.NextUInt64(), Guid.NewGuid().ToString().ToUpper(), seed.HasValue);
            save.UnlockDungeon(chosenDest);
            DataManager.Instance.SetProgress(save);
            DataManager.Instance.Save.UpdateVersion();
            DataManager.Instance.Save.UpdateOptions();
            DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();
            DataManager.Instance.Save.ActiveTeam.Name = team;

            MonsterData monsterData = DataManager.Instance.GetMonster(choice);

            int formSlot = FormSetting;
            List<int> forms = CharaDetailMenu.GetPossibleForms(monsterData);
            if (formSlot >= forms.Count)
                formSlot = forms.Count - 1;
            if (formSlot == -1)
                formSlot = MathUtils.Rand.Next(forms.Count);

            int formIndex = forms[formSlot];

            Gender gender = CharaDetailMenu.LimitGender(monsterData, formIndex, GenderSetting);
            if (gender == Gender.Unknown)
                gender = monsterData.Forms[formIndex].RollGender(MathUtils.Rand);
            
            int intrinsicSlot = CharaDetailMenu.LimitIntrinsic(monsterData, formIndex, IntrinsicSetting);
            int intrinsic;
            if (intrinsicSlot == -1)
                intrinsic = monsterData.Forms[formIndex].RollIntrinsic(MathUtils.Rand, 3);
            else if (intrinsicSlot == 0)
                intrinsic = monsterData.Forms[formIndex].Intrinsic1;
            else if (intrinsicSlot == 1)
                intrinsic = monsterData.Forms[formIndex].Intrinsic2;
            else
                intrinsic = monsterData.Forms[formIndex].Intrinsic3;

            Character newChar = DataManager.Instance.Save.ActiveTeam.CreatePlayer(MathUtils.Rand, new MonsterID(choice, formIndex, SkinSetting, gender), DataManager.Instance.StartLevel, intrinsic, DataManager.Instance.StartPersonality);
            newChar.Nickname = name;
            DataManager.Instance.Save.ActiveTeam.Players.Add(newChar);

            try
            {
                LuaEngine.Instance.OnNewGame();
                if (DataManager.Instance.Save.ActiveTeam.Players.Count == 0)
                    throw new Exception("Script generated an invalid team!");
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.BeginGameInSegment(new ZoneLoc(chosenDest, new SegLoc()), GameProgress.DungeonStakes.Risk, true, false));
        }
    }
}
