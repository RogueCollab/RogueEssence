using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public abstract class TeamResultsMenu : SideScrollMenu
    {
        public MenuText Title;
        public MenuDivider Div;
        public SpeakerPortrait[] Portraits;
        public MenuText[] Stats;

        public GameProgress Ending;

        public void Initialize(GameProgress ending)
        {
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 140, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 140, 224));
            Ending = ending;

            Title = new MenuText(GetTitle(),
                new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);

            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            EventedList<Character> charList = GetChars();
            Stats = new MenuText[charList.Count * 5];
            Portraits = new SpeakerPortrait[charList.Count];
            for (int ii = 0; ii < charList.Count; ii++)
            {
                CharData character = charList[ii];
                MonsterData dexEntry = DataManager.Instance.GetMonster(character.BaseForm.Species);
                BaseMonsterForm formEntry = dexEntry.Forms[character.BaseForm.Form];
                Portraits[ii] = new SpeakerPortrait(character.BaseForm, new EmoteStyle(0),
                    new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + 44 * ii + TitledStripMenu.TITLE_OFFSET), false);
                string speciesText = character.BaseName + " / " + CharData.GetFullFormName(character.BaseForm);
                Stats[ii * 5] = new MenuText(speciesText,
                    new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + 44 * ii + TitledStripMenu.TITLE_OFFSET));
                Stats[ii * 5 + 1] = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"),
                    new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 16, GraphicsManager.MenuBG.TileHeight + 44 * ii + TitledStripMenu.TITLE_OFFSET), DirH.Right);
                Stats[ii * 5 + 2] = new MenuText(character.Level.ToString(),
                    new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 16 + GraphicsManager.TextFont.SubstringWidth(DataManager.Instance.MaxLevel.ToString()), GraphicsManager.MenuBG.TileHeight + 44 * ii + TitledStripMenu.TITLE_OFFSET), DirH.Right);
                if (Ending.UUID == character.OriginalUUID)
                {
                    Stats[ii * 5 + 3] = new MenuText(Text.FormatKey("MENU_TEAM_MET_AT", character.MetAt),
                    new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + 44 * ii + TitledStripMenu.TITLE_OFFSET));
                }
                else
                {
                    Stats[ii * 5 + 3] = new MenuText(Text.FormatKey("MENU_TEAM_TRADED_FROM", character.OriginalTeam),
                    new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + 44 * ii + TitledStripMenu.TITLE_OFFSET));
                }
                Stats[ii * 5 + 4] = new MenuText((String.IsNullOrEmpty(character.DefeatAt) ? "" : Text.FormatKey("MENU_TEAM_FELL_AT", character.DefeatAt)),
                    new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + 44 * ii + TitledStripMenu.TITLE_OFFSET));

                base.Initialize();
            }
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Div;

            foreach (MenuText text in Stats)
                yield return text;
            foreach (SpeakerPortrait portrait in Portraits)
                yield return portrait;
        }

        protected abstract string GetTitle();
        protected abstract EventedList<Character> GetChars();
    }


    public class PartyResultsMenu : TeamResultsMenu
    {
        public PartyResultsMenu(GameProgress ending)
        {
            Initialize(ending);
        }

        protected override string GetTitle()
        {
            if (Ending.ActiveTeam.Name != "")
                return Ending.ActiveTeam.GetDisplayName();
            else
                return Text.FormatKey("MENU_TEAM_TITLE");
        }

        protected override EventedList<Character> GetChars()
        {
            return Ending.ActiveTeam.Players;
        }


        public override void Update(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.Menu) || input.JustPressed(FrameInput.InputType.Confirm)
                || input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
            }
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new InvResultsMenu(Ending));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (Ending.ActiveTeam.Assembly.Count > 0)
                    MenuManager.Instance.ReplaceMenu(new AssemblyResultsMenu(Ending, 0));
                else
                    MenuManager.Instance.ReplaceMenu(new VersionResultsMenu(Ending, 0));
            }

        }
    }


    public class AssemblyResultsMenu : TeamResultsMenu
    {
        public int Page;

        public AssemblyResultsMenu(GameProgress ending, int page)
        {
            Page = page;

            Initialize(ending);
        }

        protected override string GetTitle()
        {
            if (Ending.ActiveTeam.Name != "")
                return Text.FormatKey("MENU_RESULTS_ASSEMBLY_TITLE", Ending.ActiveTeam.GetDisplayName(), Page + 1, MathUtils.DivUp(Ending.ActiveTeam.Assembly.Count, 4));
            else
                return Text.FormatKey("MENU_RESULTS_ASSEMBLY_TITLE_ANY", Ending.ActiveTeam.GetDisplayName(), Page + 1, MathUtils.DivUp(Ending.ActiveTeam.Assembly.Count, 4));
        }

        protected override EventedList<Character> GetChars()
        {
            EventedList<Character> characters = new EventedList<Character>();
            for (int ii = Page * 4; ii < Ending.ActiveTeam.Assembly.Count && ii < (Page + 1) * 4; ii++)
                characters.Add(Ending.ActiveTeam.Assembly[ii]);
            return characters;
        }

        public override void Update(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.Menu) || input.JustPressed(FrameInput.InputType.Confirm)
                || input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
            }
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (Page > 0)
                    MenuManager.Instance.ReplaceMenu(new AssemblyResultsMenu(Ending, Page - 1));
                else
                    MenuManager.Instance.ReplaceMenu(new PartyResultsMenu(Ending));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (Page < (Ending.ActiveTeam.Assembly.Count - 1) / 4)
                    MenuManager.Instance.ReplaceMenu(new AssemblyResultsMenu(Ending, Page+1));
                else
                    MenuManager.Instance.ReplaceMenu(new VersionResultsMenu(Ending, 0));
            }

        }
    }
}
