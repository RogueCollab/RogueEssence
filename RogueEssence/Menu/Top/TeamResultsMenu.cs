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
                new Loc(GraphicsManager.ScreenWidth / 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.None);

            Div = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);

            List<Character> charList = GetChars();
            Stats = new MenuText[charList.Count * 4];
            Portraits = new SpeakerPortrait[charList.Count];
            for (int ii = 0; ii < charList.Count; ii++)
            {
                CharData character = charList[ii];
                MonsterData dexEntry = DataManager.Instance.GetMonster(character.BaseForm.Species);
                BaseMonsterForm formEntry = dexEntry.Forms[character.BaseForm.Form];
                Portraits[ii] = new SpeakerPortrait(character.BaseForm, new EmoteStyle(0),
                    Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + 44 * ii + TitledStripMenu.TITLE_OFFSET), false);
                string speciesText = character.BaseName + " / " + CharData.GetFullFormName(character.BaseForm);
                Stats[ii * 4] = new MenuText(speciesText,
                    Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + 44 * ii + TitledStripMenu.TITLE_OFFSET));
                Stats[ii * 4 + 1] = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", character.Level),
                    new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 2 - 24, Bounds.Y + GraphicsManager.MenuBG.TileHeight + 44 * ii + TitledStripMenu.TITLE_OFFSET));
                if (Ending.UUID == character.OriginalUUID)
                {
                    Stats[ii * 4 + 2] = new MenuText(Text.FormatKey("MENU_TEAM_MET_AT", character.MetAt),
                    Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + 44 * ii + TitledStripMenu.TITLE_OFFSET));
                }
                else
                {
                    Stats[ii * 4 + 2] = new MenuText(Text.FormatKey("MENU_TEAM_TRADED_FROM", character.OriginalTeam),
                    Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + 44 * ii + TitledStripMenu.TITLE_OFFSET));
                }
                Stats[ii * 4 + 3] = new MenuText((String.IsNullOrEmpty(character.DefeatAt) ? "" : Text.FormatKey("MENU_TEAM_FELL_AT", character.DefeatAt)),
                    Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + 44 * ii + TitledStripMenu.TITLE_OFFSET));

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
        protected abstract List<Character> GetChars();
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
                return Ending.ActiveTeam.Name;
            else
                return Text.FormatKey("MENU_TEAM_TITLE");
        }

        protected override List<Character> GetChars()
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
                    MenuManager.Instance.ReplaceMenu(new FinalResultsMenu(Ending));
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
                return Text.FormatKey("MENU_RESULTS_ASSEMBLY_TITLE", Ending.ActiveTeam.Name, Page + 1, (Ending.ActiveTeam.Assembly.Count - 1) / 4 + 1);
            else
                return Text.FormatKey("MENU_RESULTS_ASSEMBLY_TITLE_ANY", Ending.ActiveTeam.Name, Page + 1, (Ending.ActiveTeam.Assembly.Count - 1) / 4 + 1);
        }

        protected override List<Character> GetChars()
        {
            List<Character> characters = new List<Character>();
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
                    MenuManager.Instance.ReplaceMenu(new FinalResultsMenu(Ending));
            }

        }
    }
}
