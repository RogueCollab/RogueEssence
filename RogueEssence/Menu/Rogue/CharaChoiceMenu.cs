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
        private string chosenDest;
        public int FormSetting;
        public string SkinSetting;
        public Gender GenderSetting;
        public int IntrinsicSetting;
        private SummaryMenu titleMenu;
        private CharaSummary infoMenu;
        public SpeakerPortrait Portrait;
        private List<string> startChars;
        private ulong? seed;

        public CharaChoiceMenu(string teamName, string chosenDungeon, ulong? seed)
        {
            GenderSetting = Gender.Unknown;
            SkinSetting = DataManager.Instance.DefaultSkin;
            IntrinsicSetting = -1;
            FormSetting = -1;

            startChars = GetStartersList();


            List<MenuChoice> flatChoices = new List<MenuChoice>();
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_START_RANDOM"), () => { choose(MathUtils.Rand.Next(startChars.Count), true); }));
            for (int ii = 0; ii < startChars.Count; ii++)
            {
                int startSlot = ii;
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(startChars[ii]).GetColoredName(), () => { choose(startSlot, false); }));
            }

            int actualChoice = Math.Min(Math.Max(0, defaultChoice), flatChoices.Count - 1);
            IChoosable[][] box = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            int totalSlots = SLOTS_PER_PAGE;
            if (box.Length == 1)
                totalSlots = box[0].Length;

            team = teamName;
            chosenDest = chosenDungeon;
            this.seed = seed;

            Portrait = new SpeakerPortrait(MonsterID.Invalid, new EmoteStyle(0), new Loc(200, 64), true);

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

            if (!String.IsNullOrEmpty(Portrait.Speaker.Species))
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

        private void choose(int choice, bool randomized)
        {
            GameManager.Instance.RogueConfig.StarterRandomized = randomized;
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
                    CharaDetailMenu menu = new CharaDetailMenu(totalChoice > 0 ? startChars[totalChoice - 1] : "", this);
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
                string species = startChars[totalChoice - 1];
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
                Portrait.Speaker = MonsterID.Invalid;
                infoMenu.SetDetails("", DataManager.Instance.DefaultSkin, Gender.Unknown, "");
            }
            
        }


        private void start(int choice, string name)
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = Begin(choice, name);
        }
        
        public IEnumerator<YieldInstruction> Begin(int choice, string name)
        {
            bool seeded = seed.HasValue;
            ulong seedVal = seeded ? seed.Value : MathUtils.Rand.NextUInt64();
            RogueProgress save = new RogueProgress(seedVal, Guid.NewGuid().ToString().ToUpper(), seeded);
            string species = startChars[choice];
            GameManager.Instance.RogueConfig.Starter = species;
            GameManager.Instance.RogueConfig.IntrinsicSetting = IntrinsicSetting;
            GameManager.Instance.RogueConfig.FormSetting = FormSetting;
            GameManager.Instance.RogueConfig.GenderSetting= GenderSetting;
            GameManager.Instance.RogueConfig.SkinSetting = SkinSetting;
            GameManager.Instance.RogueConfig.TeamName = team;
            GameManager.Instance.RogueConfig.Nickname = name;
            GameManager.Instance.RogueConfig.Destination = chosenDest;
            GameManager.Instance.RogueConfig.Seeded = seeded;
            GameManager.Instance.RogueConfig.Seed = seedVal;
            return save.StartRogue(GameManager.Instance.RogueConfig);
        }

        public static List<string> GetStartersList()
        {
            List<string> starters = new List<string>();
            foreach(string key in DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetOrderedKeys(true))
            {
                if (DiagManager.Instance.DevMode)
                    starters.Add(key);

                else if (DataManager.Instance.Save.GetRogueUnlock(key) && DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(key).Released)
                    starters.Add(key);
                else if (DataManager.Instance.StartChars.FindIndex(mon => mon.mon.Species == key) > -1)
                    starters.Add(key);
            }

            return starters;
        }
    }
}
