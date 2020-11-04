using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MemberStatsMenu : InteractableMenu
    {
        int teamSlot;
        bool assembly;
        bool allowAssembly;
        public MenuText Title;
        public MenuDivider Div;

        public SpeakerPortrait Portrait;
        public MenuText Name;
        public MenuText Level;
        public MenuText EXP;

        public MenuText Elements;
        public MenuDivider MainDiv;

        public MenuText StatsTitle;
        public MenuText HPLabel;
        public MenuText SpeedLabel;
        public MenuText AttackLabel;
        public MenuText DefenseLabel;
        public MenuText MAtkLabel;
        public MenuText MDefLabel;
        public MenuText HP;
        public MenuText Speed;
        public MenuText Attack;
        public MenuText Defense;
        public MenuText MAtk;
        public MenuText MDef;
        public MenuStatBar HPBar;
        public MenuStatBar SpeedBar;
        public MenuStatBar AttackBar;
        public MenuStatBar DefenseBar;
        public MenuStatBar MAtkBar;
        public MenuStatBar MDefBar;
        public MenuDivider ItemDiv;
        public MenuText Item;

        //allow moving up and down (but don't alter the team choice selection because it's hard)

        public MemberStatsMenu(int teamSlot, bool assembly, bool allowAssembly)
        {
            Bounds = Rect.FromPoints(new Loc(24, 16), new Loc(296, 224));

            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.allowAssembly = allowAssembly;

            Character player = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot] : DataManager.Instance.Save.ActiveTeam.Players[teamSlot];

            //TODO: align the page text properly
            Title = new MenuText(Text.FormatKey("MENU_STATS_TITLE") +" (2/3)", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Div = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);


            Portrait = new SpeakerPortrait(player.BaseForm, new EmoteStyle(0),
                Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), false);
            string speciesText = player.BaseName + " / " + CharData.GetFullFormName(player.BaseForm);
            Name = new MenuText(speciesText, Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET));

            ElementData element1 = DataManager.Instance.GetElement(player.Element1);
            ElementData element2 = DataManager.Instance.GetElement(player.Element2);

            string typeString = String.Format("{0}\u2060{1}", element1.Symbol, element1.Name.ToLocal());
            if (player.Element2 != 00)
                typeString += "/" + String.Format("{0}\u2060{1}", element2.Symbol, element2.Name.ToLocal());
            BaseMonsterForm monsterForm = DataManager.Instance.GetMonster(player.BaseForm.Species).Forms[player.BaseForm.Form];
            bool origElements = (player.Element1 == monsterForm.Element1);
            origElements &= (player.Element2 == monsterForm.Element2);
            Elements = new MenuText(Text.FormatKey("MENU_TEAM_ELEMENT", typeString), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 1 + TitledStripMenu.TITLE_OFFSET), origElements ? Color.White : Color.Yellow);

            Level = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", player.Level), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET));

            int expToNext = 0;
            if (player.Level < DataManager.Instance.MaxLevel)
            {
                int growth = DataManager.Instance.GetMonster(player.BaseForm.Species).EXPTable;
                GrowthData growthData = DataManager.Instance.GetGrowth(growth);
                expToNext = growthData.GetExpToNext(player.Level);
            }
            EXP = new MenuText(Text.FormatKey("MENU_TEAM_EXP", player.EXP, expToNext),
                Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET));

            MainDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);

            StatsTitle = new MenuText(Text.FormatKey("MENU_TEAM_STATS"), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET));

            HPLabel = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.HP.ToLocal("tiny")), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET));
            AttackLabel = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.Attack.ToLocal("tiny")), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET), player.ProxyAtk > -1 ? Color.Yellow : Color.White);
            DefenseLabel = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.Defense.ToLocal("tiny")), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), player.ProxyDef > -1 ? Color.Yellow : Color.White);
            MAtkLabel = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.MAtk.ToLocal("tiny")), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 8 + TitledStripMenu.TITLE_OFFSET), player.ProxyMAtk > -1 ? Color.Yellow : Color.White);
            MDefLabel = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.MDef.ToLocal("tiny")), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), player.ProxyMDef > -1 ? Color.Yellow : Color.White);
            SpeedLabel = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.Speed.ToLocal("tiny")), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET), player.ProxySpeed > -1 ? Color.Yellow : Color.White);

            HP = new MenuText(player.MaxHP.ToString(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            Attack = new MenuText(player.Atk.ToString(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxyAtk > -1 ? Color.Yellow : Color.White);
            Defense = new MenuText(player.Def.ToString(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxyDef > -1 ? Color.Yellow : Color.White);
            MAtk = new MenuText(player.MAtk.ToString(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 8 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxyMAtk > -1 ? Color.Yellow : Color.White);
            MDef = new MenuText(player.MDef.ToString(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxyMDef > -1 ? Color.Yellow : Color.White);
            Speed = new MenuText(player.Speed.ToString(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxySpeed > -1 ? Color.Yellow : Color.White);

            int hpLength = calcLength(Stat.HP, monsterForm, player.MaxHP, player.Level);
            HPBar = new MenuStatBar(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET), hpLength, calcColor(hpLength));
            int atkLength = calcLength(Stat.Attack, monsterForm, player.Atk, player.Level);
            AttackBar = new MenuStatBar(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET), atkLength, calcColor(atkLength));
            int defLength = calcLength(Stat.Defense, monsterForm, player.Def, player.Level);
            DefenseBar = new MenuStatBar(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), defLength, calcColor(defLength));
            int mAtkLength = calcLength(Stat.MAtk, monsterForm, player.MAtk, player.Level);
            MAtkBar = new MenuStatBar(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 8 + TitledStripMenu.TITLE_OFFSET), mAtkLength, calcColor(mAtkLength));
            int mDefLength = calcLength(Stat.MDef, monsterForm, player.MDef, player.Level);
            MDefBar = new MenuStatBar(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), mDefLength, calcColor(mDefLength));
            int speedLength = calcLength(Stat.Speed, monsterForm, player.Speed, player.Level);
            SpeedBar = new MenuStatBar(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET), speedLength, calcColor(speedLength));

            ItemDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 12), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);

            Item = new MenuText(player.EquippedItem.ID > -1 ? Text.FormatKey("MENU_HELD_ITEM", player.EquippedItem.GetName()) : Text.FormatKey("MENU_HELD_NO_ITEM"), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 11 + TitledStripMenu.TITLE_OFFSET));

        }

        private int calcLength(Stat stat, BaseMonsterForm form, int statVal, int level)
        {
            int avgLevel = 0;
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
                avgLevel += DataManager.Instance.Save.ActiveTeam.Players[ii].Level;
            avgLevel /= DataManager.Instance.Save.ActiveTeam.Players.Count;
            int baseStat = form.ReverseGetStat(stat, statVal, level);
            baseStat = baseStat * level / avgLevel;
            return Math.Min(Math.Max(1, baseStat * 140 / 120), 168);
        }

        private Color calcColor(int length)
        {
            if (length * 120 < 50 * 140)
                return new Color(248, 128, 88);
            else if (length * 120 < 80 * 140)
                return new Color(248, 232, 88);
            else if (length * 120 < 110 * 140)
                return new Color(88, 248, 88);
            else
                return new Color(88, 192, 248);
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Div;

            yield return Portrait;
            yield return Name;

            yield return Level;
            yield return Elements;
            yield return EXP;

            yield return MainDiv;

            yield return StatsTitle;
            yield return HPLabel;
            yield return SpeedLabel;
            yield return AttackLabel;
            yield return DefenseLabel;
            yield return MAtkLabel;
            yield return MDefLabel;

            yield return HP;
            yield return Speed;
            yield return Attack;
            yield return Defense;
            yield return MAtk;
            yield return MDef;

            yield return HPBar;
            yield return SpeedBar;
            yield return AttackBar;
            yield return DefenseBar;
            yield return MAtkBar;
            yield return MDefBar;

            yield return ItemDiv;
            yield return Item;
        }

        public override void Update(InputManager input)
        {
            Visible = true;
            if (input.JustPressed(FrameInput.InputType.Menu))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.ClearMenus();
            }
            else if (input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.RemoveMenu();
            }
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(teamSlot, assembly, allowAssembly));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(teamSlot, assembly, allowAssembly));
            }
            else if (IsInputting(input, Dir8.Up))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = (!assembly) ? DataManager.Instance.Save.ActiveTeam.Assembly.Count : DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (teamSlot - 1 < 0)
                        MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(amtLimit - 1, !assembly, allowAssembly));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(teamSlot - 1, assembly, allowAssembly));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberStatsMenu((teamSlot + DataManager.Instance.Save.ActiveTeam.Players.Count - 1) % DataManager.Instance.Save.ActiveTeam.Players.Count, false, allowAssembly));
            }
            else if (IsInputting(input, Dir8.Down))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly.Count : DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (teamSlot + 1 >= amtLimit)
                        MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(0, !assembly, allowAssembly));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(teamSlot + 1, assembly, allowAssembly));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberStatsMenu((teamSlot + 1) % DataManager.Instance.Save.ActiveTeam.Players.Count, false, allowAssembly));
            }
        }
    }
}
