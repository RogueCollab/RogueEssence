using System;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MemberStatsMenu : InteractableMenu
    {
        Team team;
        int teamSlot;
        bool assembly;
        bool allowAssembly;
        bool guest;
        bool boostView;

        public MenuText Title;
        public MenuText PageText;
        public MenuDivider Div;

        public SpeakerPortrait Portrait;
        public MenuText Name;
        public MenuText LevelLabel;
        public MenuText Level;
        public MenuText EXP;

        public MenuText CharElements;
        public MenuDivider MainDiv;

        public MenuText StatsTitle;
        public MenuText BonusTooltip;
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
        public MenuText HPBonus;
        public MenuText SpeedBonus;
        public MenuText AttackBonus;
        public MenuText DefenseBonus;
        public MenuText MAtkBonus;
        public MenuText MDefBonus;
        public MenuDivider ItemDiv;
        public MenuText Item;

        //allow moving up and down (but don't alter the team choice selection because it's hard)

        public MemberStatsMenu(Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest) : this(MenuLabel.SUMMARY_MENU_STATS, team, teamSlot, assembly, allowAssembly, guest, false) { }
        public MemberStatsMenu(Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest, bool bonusView) : this(MenuLabel.SUMMARY_MENU_STATS, team, teamSlot, assembly, allowAssembly, guest, bonusView) { }
        public MemberStatsMenu(string label, Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest) : this(label, team, teamSlot, assembly, allowAssembly, guest, false) { }
        public MemberStatsMenu(string label, Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest, bool bonusView)
        {
            Label = label;

            Bounds = Rect.FromPoints(new Loc(24, 16), new Loc(296, 224));

            this.team = team;
            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.allowAssembly = allowAssembly;
            this.guest = guest;

            Character player = null;

            if (guest)
            {
                player = team.Guests[teamSlot];
            }
            else
            {
                player = assembly ? ((ExplorerTeam)team).Assembly[teamSlot] : team.Players[teamSlot];
            }

            MonsterData dexEntry = DataManager.Instance.GetMonster(player.BaseForm.Species);
            BaseMonsterForm formEntry = dexEntry.Forms[player.BaseForm.Form];
            
            int totalLearnsetPages = (int) Math.Ceiling((double)MemberLearnsetMenu.getEligibleSkills(formEntry) / MemberLearnsetMenu.SLOTS_PER_PAGE);
            int totalOtherMemberPages = 3;
            int totalPages = totalLearnsetPages + totalOtherMemberPages;
            
            Title = new MenuText(Text.FormatKey("MENU_STATS_TITLE"), new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Title);
            PageText = new MenuText($"(2/{totalPages})", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight), DirH.Right);
            Elements.Add(PageText);
            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(Div);

            Portrait = new SpeakerPortrait(player.BaseForm, new EmoteStyle(0),
                new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), false);
            Elements.Add(Portrait);

            string speciesText = player.GetDisplayName(true) + " / " + CharData.GetFullFormName(player.BaseForm);
            Name = new MenuText(speciesText, new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(Name);

            ElementData element1 = DataManager.Instance.GetElement(player.Element1);
            ElementData element2 = DataManager.Instance.GetElement(player.Element2);

            string typeString = element1.GetIconName();
            if (player.Element2 != DataManager.Instance.DefaultElement)
                typeString += "/" + element2.GetIconName();
            BaseMonsterForm monsterForm = DataManager.Instance.GetMonster(player.BaseForm.Species).Forms[player.BaseForm.Form];
            bool origElements = (player.Element1 == monsterForm.Element1);
            origElements &= (player.Element2 == monsterForm.Element2);
            CharElements = new MenuText(Text.FormatKey("MENU_TEAM_ELEMENT", typeString), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 1 + TitledStripMenu.TITLE_OFFSET), origElements ? Color.White : Color.Yellow);
            Elements.Add(CharElements);

            LevelLabel = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(LevelLabel);
            Level = new MenuText(player.Level.ToString(),  new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48 + GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_LEVEL_SHORT")), GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET), DirH.Left);
            Elements.Add(Level);

            int expToNext = 0;
            if (player.Level < DataManager.Instance.Start.MaxLevel)
            {
                string growth = DataManager.Instance.GetMonster(player.BaseForm.Species).EXPTable;
                GrowthData growthData = DataManager.Instance.GetGrowth(growth);
                expToNext = growthData.GetExpToNext(player.Level);
            }
            EXP = new MenuText(Text.FormatKey("MENU_TEAM_EXP", player.EXP, expToNext),
                new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(EXP);

            MainDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MainDiv);

            StatsTitle = new MenuText(Text.FormatKey("MENU_TEAM_STATS"), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(StatsTitle);
            BonusTooltip = new MenuText(Text.FormatKey("MENU_TEAM_BONUS", DiagManager.Instance.GetControlString(FrameInput.InputType.SortItems)), new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            Elements.Add(BonusTooltip);

            HPLabel = new MenuText(Text.FormatKey("MENU_LABEL", Stat.HP.ToLocal("tiny")), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(HPLabel);
            AttackLabel = new MenuText(Text.FormatKey("MENU_LABEL", Stat.Attack.ToLocal("tiny")), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET), player.ProxyAtk > -1 ? Color.Yellow : Color.White);
            Elements.Add(AttackLabel);
            DefenseLabel = new MenuText(Text.FormatKey("MENU_LABEL", Stat.Defense.ToLocal("tiny")), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), player.ProxyDef > -1 ? Color.Yellow : Color.White);
            Elements.Add(DefenseLabel);
            MAtkLabel = new MenuText(Text.FormatKey("MENU_LABEL", Stat.MAtk.ToLocal("tiny")), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 8 + TitledStripMenu.TITLE_OFFSET), player.ProxyMAtk > -1 ? Color.Yellow : Color.White);
            Elements.Add(MAtkLabel);
            MDefLabel = new MenuText(Text.FormatKey("MENU_LABEL", Stat.MDef.ToLocal("tiny")), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), player.ProxyMDef > -1 ? Color.Yellow : Color.White);
            Elements.Add(MDefLabel);
            SpeedLabel = new MenuText(Text.FormatKey("MENU_LABEL", Stat.Speed.ToLocal("tiny")), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET), player.ProxySpeed > -1 ? Color.Yellow : Color.White);
            Elements.Add(SpeedLabel);

            HP = new MenuText(player.MaxHP.ToString(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            Elements.Add(HP);
            Attack = new MenuText(player.Atk.ToString(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxyAtk > -1 ? Color.Yellow : Color.White);
            Elements.Add(Attack);
            Defense = new MenuText(player.Def.ToString(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxyDef > -1 ? Color.Yellow : Color.White);
            Elements.Add(Defense);
            MAtk = new MenuText(player.MAtk.ToString(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 8 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxyMAtk > -1 ? Color.Yellow : Color.White);
            Elements.Add(MAtk);
            MDef = new MenuText(player.MDef.ToString(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxyMDef > -1 ? Color.Yellow : Color.White);
            Elements.Add(MDef);
            Speed = new MenuText(player.Speed.ToString(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 72, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, player.ProxySpeed > -1 ? Color.Yellow : Color.White);
            Elements.Add(Speed);

            int hpLength = calcLength(team, Stat.HP, monsterForm, player.MaxHP, player.Level, guest);
            HPBar = new MenuStatBar(new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET), hpLength, calcColor(hpLength));
            Elements.Add(HPBar);
            int atkLength = calcLength(team, Stat.Attack, monsterForm, player.Atk, player.Level, guest);
            AttackBar = new MenuStatBar(new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET), atkLength, calcColor(atkLength));
            Elements.Add(AttackBar);
            int defLength = calcLength(team, Stat.Defense, monsterForm, player.Def, player.Level, guest);
            DefenseBar = new MenuStatBar(new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), defLength, calcColor(defLength));
            Elements.Add(DefenseBar);
            int mAtkLength = calcLength(team, Stat.MAtk, monsterForm, player.MAtk, player.Level, guest);
            MAtkBar = new MenuStatBar(new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 8 + TitledStripMenu.TITLE_OFFSET), mAtkLength, calcColor(mAtkLength));
            Elements.Add(MAtkBar);
            int mDefLength = calcLength(team, Stat.MDef, monsterForm, player.MDef, player.Level, guest);
            MDefBar = new MenuStatBar(new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), mDefLength, calcColor(mDefLength));
            Elements.Add(MDefBar);
            int speedLength = calcLength(team, Stat.Speed, monsterForm, player.Speed, player.Level, guest);
            SpeedBar = new MenuStatBar(new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 76, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET), speedLength, calcColor(speedLength));
            Elements.Add(SpeedBar);


            HPBonus = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            Elements.Add(HPBonus);
            AttackBonus = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET),  DirH.Right);
            Elements.Add(AttackBonus);
            DefenseBonus = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            Elements.Add(DefenseBonus);
            MAtkBonus = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 8 + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            Elements.Add(MAtkBonus);
            MDefBonus = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            Elements.Add(MDefBonus);
            SpeedBonus = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            Elements.Add(SpeedBonus);

            ItemDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 12), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(ItemDiv);

            Item = new MenuText(!String.IsNullOrEmpty(player.EquippedItem.ID) ? Text.FormatKey("MENU_HELD_ITEM", player.EquippedItem.GetDisplayName()) : Text.FormatKey("MENU_HELD_NO_ITEM"), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 11 + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(Item);

            if (bonusView)
                SwitchView();
        }

        private int calcBoostLength(Stat stat, Character player)
        {
            int MAX_STAT_BOOST = 256;
            int statBonus = 0;
            switch(stat)
            {
                case Stat.HP: statBonus = player.MaxHPBonus; break;
                case Stat.Attack: statBonus = player.AtkBonus; break;
                case Stat.Defense: statBonus = player.DefBonus; break;
                case Stat.MAtk: statBonus = player.MAtkBonus; break;
                case Stat.MDef: statBonus = player.MDefBonus; break;
                case Stat.Speed: statBonus = player.SpeedBonus; break;
            }
            return Math.Min(Math.Max(0, statBonus * 128 / MAX_STAT_BOOST), 128)+1;
        }

        private int calcLength(Team team, Stat stat, BaseMonsterForm form, int statVal, int level, bool guest)
        {
            int avgLevel = 0;
            if (guest)
            {
                for (int ii = 0; ii < team.Guests.Count; ii++)
                    avgLevel += team.Guests[ii].Level;
                avgLevel /= team.Guests.Count;
            }
            else
            {
                for (int ii = 0; ii < team.Players.Count; ii++)
                    avgLevel += team.Players[ii].Level;
                avgLevel /= team.Players.Count;
            }
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
            else if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                GameManager.Instance.SE("Menu/Confirm");
                SwitchView();
            }
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, teamSlot, assembly, allowAssembly, guest));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, teamSlot, assembly, allowAssembly, guest));
            }
            else if (IsInputting(input, Dir8.Up))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = (!assembly) ? ((ExplorerTeam)team).Assembly.Count : team.Players.Count;
                    if (teamSlot - 1 < 0)
                        MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, amtLimit - 1, !assembly, true, false, boostView));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, teamSlot - 1, assembly, true, false, boostView));
                }
                else if (guest)
                {
                    MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, (teamSlot + team.Guests.Count - 1) % team.Guests.Count, false, false, true, boostView));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, (teamSlot + team.Players.Count - 1) % team.Players.Count, false, false, false, boostView));
            }
            else if (IsInputting(input, Dir8.Down))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = assembly ? ((ExplorerTeam)team).Assembly.Count : team.Players.Count;
                    if (teamSlot + 1 >= amtLimit)
                        MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, 0, !assembly, true, false, boostView));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, teamSlot + 1, assembly, true, false, boostView));
                }
                else if (guest)
                {
                    MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, (teamSlot + 1) % team.Guests.Count, false, false, true, boostView));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, (teamSlot + 1) % team.Players.Count, false, false, false, boostView));
            }
        }

        public void SwitchView()
        {
            boostView = !boostView;
            int MAX_STAT_BOOST = 256;

            Character player = null;

            if (guest)
            {
                player = team.Guests[teamSlot];
            }
            else
            {
                player = assembly ? ((ExplorerTeam)team).Assembly[teamSlot] : team.Players[teamSlot];
            }

            BaseMonsterForm monsterForm = DataManager.Instance.GetMonster(player.BaseForm.Species).Forms[player.BaseForm.Form];

            int mhp = player.MaxHPBonus;
            int atk = player.AtkBonus;
            int def = player.DefBonus;
            int mat = player.MAtkBonus;
            int mdf = player.MDefBonus;
            int spd = player.SpeedBonus;

            BonusTooltip.Color = boostView ? Color.Yellow : Color.White;
            HPBar.Length = boostView ? calcBoostLength(Stat.HP, player) : calcLength(team, Stat.HP, monsterForm, player.MaxHP, player.Level, guest);
            AttackBar.Length = boostView ? calcBoostLength(Stat.Attack, player) : calcLength(team, Stat.Attack, monsterForm, player.Atk, player.Level, guest);
            DefenseBar.Length = boostView ? calcBoostLength(Stat.Defense, player) : calcLength(team, Stat.Defense, monsterForm, player.Def, player.Level, guest);
            MAtkBar.Length = boostView ? calcBoostLength(Stat.MAtk, player) : calcLength(team, Stat.MAtk, monsterForm, player.MAtk, player.Level, guest);
            MDefBar.Length = boostView ? calcBoostLength(Stat.MDef, player) : calcLength(team, Stat.MDef, monsterForm, player.MDef, player.Level, guest);
            SpeedBar.Length = boostView ? calcBoostLength(Stat.Speed, player) : calcLength(team, Stat.Speed, monsterForm, player.Speed, player.Level, guest);
            HPBar.Color = calcColor(HPBar.Length);
            AttackBar.Color = calcColor(AttackBar.Length);
            DefenseBar.Color = calcColor(DefenseBar.Length);
            MAtkBar.Color = calcColor(MAtkBar.Length);
            MDefBar.Color = calcColor(MDefBar.Length);
            SpeedBar.Color = calcColor(SpeedBar.Length);
            HPBonus.SetText(boostView ? (mhp < MAX_STAT_BOOST ? mhp.ToString() : Text.FormatKey("MENU_TEAM_MAX")) : "");
            AttackBonus.SetText(boostView ? (atk < MAX_STAT_BOOST ? atk.ToString() : Text.FormatKey("MENU_TEAM_MAX")) : "");
            DefenseBonus.SetText(boostView ? (def < MAX_STAT_BOOST ? def.ToString() : Text.FormatKey("MENU_TEAM_MAX")) : "");
            MAtkBonus.SetText(boostView ? (mat < MAX_STAT_BOOST ? mat.ToString() : Text.FormatKey("MENU_TEAM_MAX")) : "");
            MDefBonus.SetText(boostView ? (mdf < MAX_STAT_BOOST ? mdf.ToString() : Text.FormatKey("MENU_TEAM_MAX")) : "");
            SpeedBonus.SetText(boostView ? (spd < MAX_STAT_BOOST ? spd.ToString() : Text.FormatKey("MENU_TEAM_MAX")) : "");
        }
    }
}
