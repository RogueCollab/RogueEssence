using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using System.Xml;
using System.Collections.Generic;

namespace RogueEssence.Content
{
    //Responsible for managing all graphics and sound for the game
    public static class GraphicsManager
    {
        [Flags]
        public enum AssetType
        {
            None = 0,
            Font = 1,
            Chara = 2,
            Portrait = 4,
            Tile = 8,
            Item = 16,
            Particle = 32,
            Beam = 64,
            Icon = 128,
            Object = 256,
            BG = 512,
            Autotile = 1024,
            All = 2047,
            Count = 2048
        }

        public enum GameZoom
        {
            x8Near = -3,
            x4Near = -2,
            x2Near = -1,
            x1 = 0,
            x2Far = 1,
            x4Far = 2,
            x8Far = 3,
        }

        public static int ConvertZoom(this GameZoom zoom, int amount, bool reverse = false)
        {
            if (reverse)
                zoom = (GameZoom)(-(int)zoom);

            switch (zoom)
            {
                case GameZoom.x8Near:
                    return amount * 8;
                case GameZoom.x4Near:
                    return amount * 4;
                case GameZoom.x2Near:
                    return amount * 2;
                case GameZoom.x2Far:
                    return amount / 2;
                case GameZoom.x4Far:
                    return amount / 4;
                case GameZoom.x8Far:
                    return amount / 8;
                default:
                    return amount;
            }
        }

        public static float GetScale(this GameZoom zoom)
        {
            switch (zoom)
            {
                case GameZoom.x8Near:
                    return 8f;
                case GameZoom.x4Near:
                    return 4f;
                case GameZoom.x2Near:
                    return 2f;
                case GameZoom.x2Far:
                    return 0.5f;
                case GameZoom.x4Far:
                    return 0.25f;
                case GameZoom.x8Far:
                    return 0.125f;
                default:
                    return 1.0f;
            }
        }

        public const string SCREENSHOT_PATH = "SCREENSHOT/";


        public const string MUSIC_PATH = CONTENT_PATH + "Music/";
        public const string SOUND_PATH = CONTENT_PATH + "Sound/";
        public const string UI_PATH = CONTENT_PATH + "UI/";

        public const string CONTENT_PATH = "Content/";

        public const string CHARA_PATTERN = CONTENT_PATH + "Chara/{0}.chara";
        public const string PORTRAIT_PATTERN = CONTENT_PATH + "Portrait/{0}.portrait";
        public const string PARTICLE_PATTERN = CONTENT_PATH + "Particle/{0}.dir";
        public const string ITEM_PATTERN = CONTENT_PATH + "Item/{0}.dir";
        public const string BEAM_PATTERN = CONTENT_PATH + "Beam/{0}.beam";
        public const string ICON_PATTERN = CONTENT_PATH + "Icon/{0}.dir";
        public const string TILE_PATTERN = CONTENT_PATH + "Tile/{0}.tile";
        public const string OBJECT_PATTERN = CONTENT_PATH + "Object/{0}.dir";
        public const string BG_PATTERN = CONTENT_PATH + "BG/{0}.dir";
        public const string FONT_PATTERN = CONTENT_PATH + "Font/{0}.font";

        /// <summary>
        /// All textures are multiples of 8, right?
        /// This also controls the minimum units for a ground map.
        /// </summary>
        public const int TEX_SIZE = 8;

        /// <summary>
        /// The size of a dungeon tile, in Tex
        /// </summary>
        public static int DungeonTexSize;

        /// <summary>
        /// The size of a dungeon tile, in pixels
        /// </summary>
        public static int TileSize { get { return DungeonTexSize * TEX_SIZE; } }

        /// <summary>
        /// The size of the game screen, which will then be stretched by the resolution settings.
        /// </summary>
        public static int ScreenWidth;
        public static int ScreenHeight;


        public static string MoneySprite;
        public static int PortraitSize;
        public static List<EmotionType> Emotions;
        public static int SOSEmotion;
        public static int AOKEmotion;
        public static List<CharFrameType> Actions;
        public static int HurtAction;
        public static int WalkAction;
        public static int IdleAction;
        public static int SleepAction;
        public static int ChargeAction;

        public const int MAX_FPS = 60;

        private const int CHARA_CACHE_SIZE = 200;
        private const int PORTRAIT_CACHE_SIZE = 50;
        private const int VFX_CACHE_SIZE = 100;
        private const int ICON_CACHE_SIZE = 100;
        private const int ITEM_CACHE_SIZE = 100;
        private const int TILE_CACHE_SIZE_PIXELS = 2000000;
        private const int OBJECT_CACHE_SIZE = 500;
        private const int BG_CACHE_SIZE = 10;

        public static ulong TotalFrameTick;

        public static event Action ZoomChanged;

        private static GameZoom zoom;

        /// <summary>
        /// The zoom of the game map.  Only applies to ground and dungeon scenes.  Used only for debug.
        /// </summary>
        public static GameZoom Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;

                ZoomChanged?.Invoke();
            }
        }

        private static int windowZoom;

        /// <summary>
        /// Game zoom based on window settings, affecting all graphics.  Independent of Zoom, which is used to zoom the map.
        /// </summary>
        public static int WindowZoom { get { return windowZoom; } }
        public static bool FullScreen { get { return graphics.IsFullScreen; } }

        public static void SetWindowMode(int mode)
        {
            if (mode == 0)
            {
                graphics.IsFullScreen = true;
                DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                int resize = Math.Min(displayMode.Width / ScreenWidth, displayMode.Height / ScreenHeight);
                windowZoom = resize;

                float ratio = Math.Max((float)WindowWidth / displayMode.Width, (float)WindowHeight / displayMode.Height);
                graphics.PreferredBackBufferWidth = (int)(displayMode.Width * ratio);
                graphics.PreferredBackBufferHeight = (int)(displayMode.Height * ratio);

                //graphics.PreferredBackBufferWidth = displayMode.Width;
                //graphics.PreferredBackBufferHeight = displayMode.Height;
            }
            else
            {
                graphics.IsFullScreen = false;
                windowZoom = mode;
                graphics.PreferredBackBufferWidth = WindowWidth;
                graphics.PreferredBackBufferHeight = WindowHeight;
            }
            graphics.ApplyChanges();

            ZoomChanged?.Invoke();
        }

        /// <summary>
        /// For letterboxing
        /// </summary>
        /// <returns></returns>
        public static Loc GetGameScreenOffset()
        {
            return new Loc((GraphicsDevice.PresentationParameters.BackBufferWidth - WindowWidth) / 2,
                (GraphicsDevice.PresentationParameters.BackBufferHeight - WindowHeight) / 2);
        }

        /// <summary>
        /// The actual size of the game window in pixels.  Used for debug drawing that doesn't try to maintain pixel-perfection.
        /// </summary>
        public static int WindowWidth { get { return windowZoom * ScreenWidth; } }
        public static int WindowHeight { get { return windowZoom * ScreenHeight; } }

        private static GraphicsDeviceManager graphics;
        public static GraphicsDevice GraphicsDevice { get { return graphics.GraphicsDevice; } }

        public static int GlobalIdle;

        public static RenderTarget2D GameScreen;

        private static Texture2D defaultTex;
        public static BaseSheet Pixel { get; private set; }

        private static LRUCache<CharID, CharSheet> spriteCache;
        private static LRUCache<CharID, PortraitSheet> portraitCache;
        private static LRUCache<string, IEffectAnim> vfxCache;
        private static LRUCache<string, DirSheet> iconCache;
        private static LRUCache<string, DirSheet> itemCache;
        private static LRUCache<string, DirSheet> objectCache;
        private static LRUCache<TileAddr, BaseSheet> tileCache;
        private static LRUCache<string, DirSheet> bgCache;

        public static FontSheet TextFont { get; private set; }
        public static FontSheet DungeonFont { get; private set; }
        public static FontSheet DamageFont { get; private set; }
        public static FontSheet HealFont { get; private set; }
        public static FontSheet EXPFont { get; private set; }
        public static FontSheet SysFont { get; private set; }

        public static TileGuide TileIndex { get; private set; }
        public static CharaIndexNode CharaIndex { get; private set; }
        public static CharaIndexNode PortraitIndex { get; private set; }

        public static BaseSheet DivTex { get; private set; }
        public static TileSheet MenuBG { get; private set; }
        public static TileSheet MenuBorder { get; private set; }
        public static TileSheet PicBorder { get; private set; }
        public static TileSheet Arrows { get; private set; }
        public static TileSheet Cursor { get; private set; }
        public static TileSheet BattleFactors { get; private set; }
        public static TileSheet Shadows { get; private set; }
        public static BaseSheet MarkerShadow { get; private set; }
        public static TileSheet Tiling { get; private set; }
        public static TileSheet Darkness { get; private set; }
        public static TileSheet Strip { get; private set; }
        public static TileSheet Buttons { get; private set; }
        public static TileSheet HPMenu { get; private set; }
        public static BaseSheet MiniHP { get; private set; }
        public static TileSheet MapSheet { get; private set; }
        public static BaseSheet Splash { get; private set; }


        public static BaseSheet Title { get; private set; }
        public static BaseSheet Subtitle { get; private set; }


        public static string HungerSE { get; private set; }
        public static string NullDmgSE { get; private set; }
        public static string CursedSE { get; private set; }
        public static string PickupSE { get; private set; }
        public static string PickupFoeSE { get; private set; }
        public static string ReplaceSE { get; private set; }
        public static string PlaceSE { get; private set; }
        public static string EquipSE { get; private set; }
        public static string MoneySE { get; private set; }
        public static string LeaderSE { get; private set; }

        public static string TitleBG { get; private set; }

        public static string TitleBGM { get; private set; }
        public static string MonsterBGM { get; private set; }

        public static bool Loaded;

        public static void InitParams()
        {
            string path = PathMod.BASE_PATH + "GFXParams.xml";
            //try to load from file

            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(path);

                XmlNode tileSize = xmldoc.DocumentElement.SelectSingleNode("TileSize");
                DungeonTexSize = Int32.Parse(tileSize.InnerText) / TEX_SIZE;

                XmlNode portraitSize = xmldoc.DocumentElement.SelectSingleNode("PortraitSize");
                PortraitSize = Int32.Parse(portraitSize.InnerText);

                XmlNode screenWidth = xmldoc.DocumentElement.SelectSingleNode("ScreenWidth");
                ScreenWidth = Int32.Parse(screenWidth.InnerText);

                XmlNode screenHeight = xmldoc.DocumentElement.SelectSingleNode("ScreenHeight");
                ScreenHeight = Int32.Parse(screenHeight.InnerText);

                XmlNode moneySprite = xmldoc.DocumentElement.SelectSingleNode("MoneySprite");
                MoneySprite = moneySprite.InnerText;

                {
                    Emotions = new List<EmotionType>();
                    List<List<string>> fallbacks = new List<List<string>>();
                    XmlNode emotions = xmldoc.DocumentElement.SelectSingleNode("Emotions");
                    foreach (XmlNode emotion in emotions.SelectNodes("Emotion"))
                    {
                        XmlNode emotionName = emotion.SelectSingleNode("Name");
                        XmlNode emotionRandom = emotion.SelectSingleNode("Random");
                        Emotions.Add(new EmotionType(emotionName.InnerText, Boolean.Parse(emotionRandom.InnerText)));
                        List<string> emotionFallbacks = new List<string>();
                        foreach (XmlNode fallback in emotion.SelectNodes("Fallback"))
                            emotionFallbacks.Add(fallback.InnerText);
                        fallbacks.Add(emotionFallbacks);
                    }
                    for (int ii = 0; ii < fallbacks.Count; ii++)
                    {
                        foreach (string fallback in fallbacks[ii])
                        {
                            int fallbackIndex = Emotions.FindIndex((a) => { return a.Name.Equals(fallback, StringComparison.OrdinalIgnoreCase); });
                            Emotions[ii].Fallbacks.Add(fallbackIndex);
                        }
                    }
                }

                XmlNode sosEmotion = xmldoc.DocumentElement.SelectSingleNode("SOSEmotion");
                SOSEmotion = Int32.Parse(sosEmotion.InnerText);

                XmlNode aokEmotion = xmldoc.DocumentElement.SelectSingleNode("AOKEmotion");
                AOKEmotion = Int32.Parse(aokEmotion.InnerText);

                {
                    Actions = new List<CharFrameType>();
                    List<List<string>> fallbacks = new List<List<string>>();
                    XmlNode actions = xmldoc.DocumentElement.SelectSingleNode("Actions");
                    foreach (XmlNode action in actions.SelectNodes("Action"))
                    {
                        XmlNode actionName = action.SelectSingleNode("Name");
                        XmlNode actionDash = action.SelectSingleNode("Dash");
                        Actions.Add(new CharFrameType(actionName.InnerText, Boolean.Parse(actionDash.InnerText)));
                        List<string> actionFallbacks = new List<string>();
                        foreach (XmlNode fallback in action.SelectNodes("Fallback"))
                            actionFallbacks.Add(fallback.InnerText);
                        fallbacks.Add(actionFallbacks);
                    }
                    for (int ii = 0; ii < fallbacks.Count; ii++)
                    {
                        foreach (string fallback in fallbacks[ii])
                        {
                            int fallbackIndex = GraphicsManager.GetAnimIndex(fallback);
                            Actions[ii].Fallbacks.Add(fallbackIndex);
                        }
                    }
                }

                XmlNode hurtAction = xmldoc.DocumentElement.SelectSingleNode("HurtAction");
                HurtAction = Int32.Parse(hurtAction.InnerText);

                XmlNode walkAction = xmldoc.DocumentElement.SelectSingleNode("WalkAction");
                WalkAction = Int32.Parse(walkAction.InnerText);

                XmlNode idleAction = xmldoc.DocumentElement.SelectSingleNode("IdleAction");
                IdleAction = Int32.Parse(idleAction.InnerText);

                XmlNode sleepAction = xmldoc.DocumentElement.SelectSingleNode("SleepAction");
                SleepAction = Int32.Parse(sleepAction.InnerText);

                XmlNode chargeAction = xmldoc.DocumentElement.SelectSingleNode("ChargeAction");
                ChargeAction = Int32.Parse(chargeAction.InnerText);

                return;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                throw;
            }
        }


        public static void InitBase(GraphicsDeviceManager newGraphics, int mode)
        {
            graphics = newGraphics;
            Zoom = GameZoom.x1;
            SetWindowMode(mode);
        }

        public static void InitSystem(GraphicsDevice graphics)
        {
            defaultTex = new Texture2D(graphics, 32, 32);
            for (int ii = 0; ii < 16; ii++)
                BaseSheet.BlitColor((ii % 2 == (ii / 4 % 2)) ? Color.Black : new Color(255, 0, 255, 255), defaultTex, 8, 8, ii % 4 * 8, ii / 4 * 8);
            //Set graphics device
            BaseSheet.InitBase(graphics, defaultTex);

            //load onepixel
            Pixel = new BaseSheet(1, 1);
            Pixel.BlitColor(Color.White, 1, 1, 0, 0);

            Splash = BaseSheet.Import(PathMod.BASE_PATH + "Splash.png");
            MarkerShadow = BaseSheet.Import(PathMod.BASE_PATH + "MarkerShadow.png");

            SysFont = LoadFontFull(PathMod.BASE_PATH + "system.font");
        }

        public static void loadStatic()
        {
            //load menu data
            MenuBG = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "MenuBG.png")), 8, 8);
               
            //load menu data
            MenuBorder = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "MenuBorder.png")), 8, 8);

            //load menu data
            PicBorder = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "PortraitBorder.png")), 4, 4);
            Arrows = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Arrows.png")), 8, 8);

            Cursor = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Cursor.png")), 11, 11);

            BattleFactors = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "BattleFactors.png")), 16, 16);

            Tiling = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Tiling.png")), TileSize, TileSize);

            Darkness = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Dark.png")), 8, 8);

            Strip = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Strip.png")), 8, 8);

            Buttons = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Buttons.png")), 16, 16);

            HPMenu = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "HP.png")), 8, 8);

            MiniHP = BaseSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "MiniHP.png")));

            MapSheet = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Map.png")), 4, 4);
            
            Shadows = TileSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Shadows.png")), 32, 16);

            //Load divider texture
            DivTex = BaseSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Divider.png")));
            
            Title = BaseSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Title.png")));
            Subtitle = BaseSheet.Import(Text.ModLangPath(PathMod.ModPath(UI_PATH + "Enter.png")));

            DiagManager.Instance.LoadMsg = "Loading Font";

            //load font
            TextFont = LoadFont("text");
            DungeonFont = LoadFont("banner");
            DamageFont = LoadFont("yellow");
            HealFont = LoadFont("green");
            EXPFont = LoadFont("blue");
        }
        public static void InitStatic()
        {
            DiagManager.Instance.LoadMsg = "Loading Graphics";

            loadStatic();

            LoadContentParams();
            
            DiagManager.Instance.LoadMsg = "Loading Headers";

            //initialize caches
            spriteCache = new LRUCache<CharID, CharSheet>(CHARA_CACHE_SIZE);
            spriteCache.OnItemRemoved = DisposeCachedObject;
            portraitCache = new LRUCache<CharID, PortraitSheet>(PORTRAIT_CACHE_SIZE);
            portraitCache.OnItemRemoved = DisposeCachedObject;
            vfxCache = new LRUCache<string, IEffectAnim>(VFX_CACHE_SIZE);
            vfxCache.OnItemRemoved = DisposeCachedObject;
            iconCache = new LRUCache<string, DirSheet>(ICON_CACHE_SIZE);
            iconCache.OnItemRemoved = DisposeCachedObject;
            itemCache = new LRUCache<string, DirSheet>(ITEM_CACHE_SIZE);
            itemCache.OnItemRemoved = DisposeCachedObject;
            tileCache = new LRUCache<TileAddr, BaseSheet>(TILE_CACHE_SIZE_PIXELS);
            tileCache.ItemCount = CountPixels;
            tileCache.OnItemRemoved = DisposeCachedObject;
            bgCache = new LRUCache<string, DirSheet>(BG_CACHE_SIZE);
            bgCache.OnItemRemoved = DisposeCachedObject;
            objectCache = new LRUCache<string, DirSheet>(OBJECT_CACHE_SIZE);
            objectCache.OnItemRemoved = DisposeCachedObject;

            //load guides
            CharaIndex = LoadCharaIndices(CONTENT_PATH + "Chara/");
            PortraitIndex = LoadCharaIndices(CONTENT_PATH + "Portrait/");
            TileIndex = LoadTileIndices(CONTENT_PATH + "Tile/");

            Loaded = true;
            //Notify script engine
            LuaEngine.Instance.OnGraphicsLoad();
        }

        public static void ReloadStatic()
        {
            unloadStatic();
            loadStatic();

            LoadContentParams();

            ClearCaches(AssetType.All);
        }

        private static void unloadStatic()
        {
            DiagManager.Instance.LoadMsg = "Unloading Graphics";
            Subtitle.Dispose();
            Title.Dispose();
            MarkerShadow.Dispose();
            Splash.Dispose();
            MapSheet.Dispose();
            MiniHP.Dispose();
            HPMenu.Dispose();
            Buttons.Dispose();
            Shadows.Dispose();
            DivTex.Dispose();
            Darkness.Dispose();
            BattleFactors.Dispose();
            Strip.Dispose();
            Cursor.Dispose();
            Arrows.Dispose();
            PicBorder.Dispose();
            MenuBorder.Dispose();
            MenuBG.Dispose();
            EXPFont.Dispose();
            HealFont.Dispose();
            DamageFont.Dispose();
            DungeonFont.Dispose();
            TextFont.Dispose();
        }

        public static void Unload()
        {
            unloadStatic();

            tileCache.Clear();
            //no need to clear tileIndexCache; it should happen automatically with all OnRemoves
            objectCache.Clear();
            bgCache.Clear();
            itemCache.Clear();
            iconCache.Clear();
            vfxCache.Clear();
            portraitCache.Clear();
            spriteCache.Clear();
            
            SysFont.Dispose();

            Pixel.Dispose();
            defaultTex.Dispose();

            Loaded = false;
            //Notify script engine
            LuaEngine.Instance.OnGraphicsUnload();
        }

        /// <summary>
        /// Bakes all assets from the "Raw Asset" directory specified in the flags.
        /// </summary>
        /// <param name="conversionFlags">Chooses which asset type to bake</param>
        public static void RunConversions(AssetType conversionFlags)
        {
            if ((conversionFlags & AssetType.Font) != AssetType.None)
            {
                Dev.ImportHelper.ImportFonts(PathMod.DEV_PATH + "Font/", PathMod.BASE_PATH + "{0}.font", "system");
                Dev.ImportHelper.ImportFonts(PathMod.DEV_PATH + "Font/", PathMod.HardMod(FONT_PATTERN), "green", "blue", "yellow", "text", "text.zh-hans", "banner");
            }
            if ((conversionFlags & AssetType.Chara) != AssetType.None)
            {
                Dev.ImportHelper.ImportAllChars(PathMod.DEV_PATH + "Sprite/", PathMod.HardMod(CHARA_PATTERN));
                Dev.ImportHelper.BuildCharIndex(CHARA_PATTERN);
            }
            if ((conversionFlags & AssetType.Portrait) != AssetType.None)
            {
                Dev.ImportHelper.ImportAllPortraits(PathMod.DEV_PATH + "Portrait/", PathMod.HardMod(PORTRAIT_PATTERN));
                Dev.ImportHelper.BuildCharIndex(PORTRAIT_PATTERN);
            }

            if ((conversionFlags & AssetType.Tile) != AssetType.None)
                Dev.ImportHelper.ImportAllTiles(PathMod.DEV_PATH + "Tile/", PathMod.HardMod(TILE_PATTERN));

            if ((conversionFlags & AssetType.Item) != AssetType.None)
                Dev.ImportHelper.ImportAllNameDirs(PathMod.DEV_PATH + "Item/", PathMod.HardMod(ITEM_PATTERN));
            if ((conversionFlags & AssetType.Particle) != AssetType.None)
                Dev.ImportHelper.ImportAllNameDirs(PathMod.DEV_PATH + "Particle/", PathMod.HardMod(PARTICLE_PATTERN));
            if ((conversionFlags & AssetType.Beam) != AssetType.None)
                Dev.ImportHelper.ImportAllBeams(PathMod.DEV_PATH + "Beam/", PathMod.HardMod(BEAM_PATTERN));

            if ((conversionFlags & AssetType.Icon) != AssetType.None)
                Dev.ImportHelper.ImportAllNameDirs(PathMod.DEV_PATH + "Icon/", PathMod.HardMod(ICON_PATTERN));
            if ((conversionFlags & AssetType.Object) != AssetType.None)
                Dev.ImportHelper.ImportAllNameDirs(PathMod.DEV_PATH + "Object/", PathMod.HardMod(OBJECT_PATTERN));
            if ((conversionFlags & AssetType.BG) != AssetType.None)
                Dev.ImportHelper.ImportAllNameDirs(PathMod.DEV_PATH + "BG/", PathMod.HardMod(BG_PATTERN));



            if ((conversionFlags & AssetType.Autotile) != AssetType.None)
            {
                // New format (image data & auto tiles):
                Dev.DtefImportHelper.ImportAllDtefTiles(PathMod.DEV_PATH + "TileDtef/", PathMod.HardMod(TILE_PATTERN));
                
                Dev.DevHelper.IndexNamedData(DataManager.DATA_PATH + "AutoTile/");
            }
            
            if ((conversionFlags & AssetType.Tile) != AssetType.None || (conversionFlags & AssetType.Autotile) != AssetType.None)
                Dev.ImportHelper.BuildTileIndex(TILE_PATTERN);
        }

        public static void InitContentFolders(string baseFolder)
        {
            Directory.CreateDirectory(Path.Join(baseFolder, CONTENT_PATH));

            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(CHARA_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(PORTRAIT_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(PARTICLE_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(ITEM_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(BEAM_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(ICON_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(TILE_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(OBJECT_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(BG_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, Path.GetDirectoryName(FONT_PATTERN)));
            Directory.CreateDirectory(Path.Join(baseFolder, MUSIC_PATH));
            Directory.CreateDirectory(Path.Join(baseFolder, SOUND_PATH));
            Directory.CreateDirectory(Path.Join(baseFolder, UI_PATH));
        }

        public static void LoadContentParams()
        {
            string path = PathMod.ModPath(CONTENT_PATH + "ContentParams.xml");
            //try to load from file
            if (File.Exists(path))
            {
                try
                {

                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    XmlNode sysSounds = xmldoc.DocumentElement.SelectSingleNode("Sounds");

                    HungerSE = sysSounds.SelectSingleNode("Hunger").InnerText;
                    NullDmgSE = sysSounds.SelectSingleNode("NullDmg").InnerText;
                    CursedSE = sysSounds.SelectSingleNode("Cursed").InnerText;
                    PickupSE = sysSounds.SelectSingleNode("Pickup").InnerText;
                    PickupFoeSE = sysSounds.SelectSingleNode("PickupFoe").InnerText;
                    ReplaceSE = sysSounds.SelectSingleNode("Replace").InnerText;
                    PlaceSE = sysSounds.SelectSingleNode("Place").InnerText;
                    EquipSE = sysSounds.SelectSingleNode("Equip").InnerText;
                    MoneySE = sysSounds.SelectSingleNode("Money").InnerText;
                    LeaderSE = sysSounds.SelectSingleNode("Leader").InnerText;

                    TitleBG = xmldoc.DocumentElement.SelectSingleNode("TitleBG").InnerText;

                    TitleBGM = xmldoc.DocumentElement.SelectSingleNode("TitleBGM").InnerText;
                    MonsterBGM = xmldoc.DocumentElement.SelectSingleNode("MonsterBGM").InnerText;

                    return;
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
        }

        public static void RebuildIndices(AssetType assetType)
        {
            if ((assetType & AssetType.Tile) != AssetType.None)
                Dev.ImportHelper.BuildTileIndex(TILE_PATTERN);

            if ((assetType & AssetType.Chara) != AssetType.None)
                Dev.ImportHelper.BuildCharIndex(CHARA_PATTERN);

            if ((assetType & AssetType.Portrait) != AssetType.None)
                Dev.ImportHelper.BuildCharIndex(PORTRAIT_PATTERN);
        }

        public static void ClearCaches(AssetType assetType)
        {
            if ((assetType & AssetType.Tile) != AssetType.None)
            {
                TileIndex = LoadTileIndices(CONTENT_PATH + "Tile/");
                tileCache.Clear();
                //no need to clear tileIndexCache; it should happen automatically with all OnRemoves
                DiagManager.Instance.LogInfo("Tilesets Reloaded.");
            }

            if ((assetType & AssetType.BG) != AssetType.None)
            {
                bgCache.Clear();
                DiagManager.Instance.LogInfo("Backgrounds Reloaded.");
            }

            if ((assetType & AssetType.Chara) != AssetType.None)
            {
                CharaIndex = LoadCharaIndices(CONTENT_PATH + "Chara/");
                spriteCache.Clear();
                DiagManager.Instance.LogInfo("Characters Reloaded.");
            }

            if ((assetType & AssetType.Portrait) != AssetType.None)
            {
                PortraitIndex = LoadCharaIndices(CONTENT_PATH + "Portrait/");
                portraitCache.Clear();
                DiagManager.Instance.LogInfo("Portraits Reloaded.");
            }

            if ((assetType & AssetType.Particle) != AssetType.None || (assetType & AssetType.Beam) != AssetType.None)
            {
                vfxCache.Clear();
                DiagManager.Instance.LogInfo("Effects Reloaded.");
            }

            if ((assetType & AssetType.Item) != AssetType.None)
            {
                itemCache.Clear();
                DiagManager.Instance.LogInfo("Items Reloaded.");
            }

            if ((assetType & AssetType.Icon) != AssetType.None)
            {
                iconCache.Clear();
                DiagManager.Instance.LogInfo("Icons Reloaded.");
            }

            if ((assetType & AssetType.Object) != AssetType.None)
            {
                objectCache.Clear();
                DiagManager.Instance.LogInfo("Objects Reloaded.");
            }
        }

        private static FontSheet LoadFont(string prefix)
        {
            return LoadFontFull(Text.ModLangPath(PathMod.ModPath(String.Format(FONT_PATTERN, prefix))));
        }
        private static FontSheet LoadFontFull(string path)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    FontSheet font = FontSheet.Load(reader);
                    return font;
                }
            }
        }

        public static CharaIndexNode LoadCharaIndices(string charaDir)
        {
            CharaIndexNode fullGuide = null;
            try
            {
                Dictionary<int, CharaIndexNode> nodes = new Dictionary<int, CharaIndexNode>();
                foreach (string modPath in PathMod.FallforthPaths(charaDir + "index.idx"))
                {
                    using (FileStream stream = File.OpenRead(modPath))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            CharaIndexNode guide = CharaIndexNode.Load(reader);
                            foreach (int key in guide.Nodes.Keys)
                                nodes[key] = guide.Nodes[key];
                        }
                    }
                }
                fullGuide = new CharaIndexNode();
                fullGuide.Nodes = nodes;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error reading header at " + charaDir + "\n", ex));
            }
            return fullGuide;
        }

        public static CharID GetFallbackForm(CharaIndexNode guide, CharID data)
        {
            CharID fallback = CharID.Invalid;
            CharID buffer = CharID.Invalid;
            if (guide.Nodes.ContainsKey(data.Species))
            {
                buffer.Species = data.Species;
                if (guide.Nodes[data.Species].Position > 0)
                    fallback = buffer;
                if (guide.Nodes[data.Species].Nodes.ContainsKey(data.Form))
                {
                    buffer.Form = data.Form;
                    if (guide.Nodes[data.Species].Nodes[data.Form].Position > 0)
                        fallback = buffer;
                    int trialSkin = data.Skin;
                    while (trialSkin > -1)
                    {
                        if (guide.Nodes[data.Species].Nodes[data.Form].Nodes.ContainsKey(trialSkin))
                        {
                            buffer.Skin = trialSkin;
                            if (guide.Nodes[data.Species].Nodes[data.Form].Nodes[trialSkin].Position > 0)
                                fallback = buffer;
                            if (guide.Nodes[data.Species].Nodes[data.Form].Nodes[trialSkin].Nodes.ContainsKey(data.Gender))
                            {
                                buffer.Gender = data.Gender;
                                if (guide.Nodes[data.Species].Nodes[data.Form].Nodes[trialSkin].Nodes[data.Gender].Position > 0)
                                    fallback = buffer;
                            }
                            break;
                        }
                        trialSkin--;
                    }
                }
            }
            if (!fallback.IsValid())
                fallback = new CharID(0, -1, -1, -1);
            return fallback;
        }

        public static CharSheet GetChara(CharID data)
        {
            data = GetFallbackForm(CharaIndex, data);

            CharSheet sheet;
            if (spriteCache.TryGetValue(data, out sheet))
                return sheet;

            if (data.IsValid())
            {
                try
                {
                    using (FileStream stream = File.OpenRead(PathMod.ModPath(String.Format(CHARA_PATTERN, data.Species))))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            long position = CharaIndex.GetPosition(data.Species, data.Form, data.Skin, data.Gender);
                            // Jump to the correct position
                            stream.Seek(position, SeekOrigin.Begin);
                            sheet = CharSheet.Load(reader);
                            spriteCache.Add(data, sheet);
                            return sheet;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(new Exception("Error loading chara " + data.Species + " " + data.Form + "-" + data.Skin + "-" + data.Gender + "\n", ex));
                }
            }

            //add error sheet
            CharSheet error = CharSheet.LoadError();
            spriteCache.Add(data, error);
            return error;
        }



        public static PortraitSheet GetPortrait(CharID data)
        {
            data = GetFallbackForm(PortraitIndex, data);

            PortraitSheet sheet;
            if (portraitCache.TryGetValue(data, out sheet))
                return sheet;

            if (data.IsValid())
            {
                try
                {
                    using (FileStream stream = File.OpenRead(PathMod.ModPath(String.Format(PORTRAIT_PATTERN, data.Species))))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            long position = PortraitIndex.GetPosition(data.Species, data.Form, data.Skin, data.Gender);
                            // Jump to the correct position
                            stream.Seek(position, SeekOrigin.Begin);
                            sheet = PortraitSheet.Load(reader);
                            portraitCache.Add(data, sheet);
                            return sheet;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(new Exception("Error loading portrait " + data.Species + " " + data.Form + "-" + data.Skin + "-" + data.Gender + "\n", ex));
                }
            }

            //add error sheet
            PortraitSheet error = PortraitSheet.LoadError();
            portraitCache.Add(data, error);
            return error;
        }

        public static BeamSheet GetBeam(string num)
        {
            IEffectAnim sheet;
            if (vfxCache.TryGetValue("Beam-" + num, out sheet))
                return (BeamSheet)sheet;

            string beamPath = PathMod.ModPath(String.Format(BEAM_PATTERN, num));
            try
            {
                if (File.Exists(beamPath))
                {
                    using (FileStream stream = File.OpenRead(beamPath))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            sheet = BeamSheet.Load(reader);
                            vfxCache.Add("Beam-" + num, sheet);
                            return (BeamSheet)sheet;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error loading " + beamPath + "\n", ex));
            }

            BeamSheet newSheet = BeamSheet.LoadError();
            vfxCache.Add("Beam-" + num, newSheet);
            return newSheet;
        }

        public static DirSheet GetAttackSheet(string name)
        {
            IEffectAnim sheet;
            if (vfxCache.TryGetValue("Particle-" + name, out sheet))
                return (DirSheet)sheet;

            string particlePath = PathMod.ModPath(String.Format(PARTICLE_PATTERN, name));
            try
            {
                if (File.Exists(particlePath))
                {
                    //read file and read binary data
                    using (FileStream fileStream = File.OpenRead(particlePath))
                    {
                        using (BinaryReader reader = new BinaryReader(fileStream))
                        {
                            sheet = DirSheet.Load(reader);
                            vfxCache.Add("Particle-" + name, sheet);
                            return (DirSheet)sheet;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error loading " + particlePath + "\n", ex));
            }
            DirSheet newSheet = DirSheet.LoadError();
            vfxCache.Add("Particle-" + name, newSheet);
            return newSheet;
        }

        public static DirSheet GetIcon(string num)
        {
            return GetDirSheet(AssetType.Icon, num);
        }

        public static DirSheet GetItem(string num)
        {
            return GetDirSheet(AssetType.Item, num);
        }

        public static DirSheet GetBackground(string num)
        {
            return GetDirSheet(AssetType.BG, num);
        }

        public static DirSheet GetObject(string num)
        {
            return GetDirSheet(AssetType.Object, num);
        }

        public static DirSheet GetDirSheet(AssetType assetType, string num)
        {
            switch (assetType)
            {
                case AssetType.Object:
                    return getDirSheetCache(num, GetPattern(assetType), objectCache);
                case AssetType.BG:
                    return getDirSheetCache(num, GetPattern(assetType), bgCache);
                case AssetType.Item:
                    return getDirSheetCache(num, GetPattern(assetType), itemCache);
                case AssetType.Icon:
                    return getDirSheetCache(num, GetPattern(assetType), iconCache);

            }
            throw new ArgumentException(String.Format("Invalid asset type: {0}", assetType));
        }

        public static string GetPattern(AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Font:
                    return FONT_PATTERN;
                case AssetType.Chara:
                    return CHARA_PATTERN;
                case AssetType.Portrait:
                    return PORTRAIT_PATTERN;
                case AssetType.Tile:
                    return TILE_PATTERN;
                case AssetType.Item:
                    return ITEM_PATTERN;
                case AssetType.Particle:
                    return PARTICLE_PATTERN;
                case AssetType.Beam:
                    return BEAM_PATTERN;
                case AssetType.Icon:
                    return ICON_PATTERN;
                case AssetType.Object:
                    return OBJECT_PATTERN;
                case AssetType.BG:
                    return BG_PATTERN;
            }
            throw new ArgumentException(String.Format("Invalid asset type: {0}", assetType));
        }

        private static DirSheet getDirSheetCache(string num, string pattern, LRUCache<string, DirSheet> cache)
        {
            DirSheet sheet;
            if (cache.TryGetValue(num, out sheet))
                return sheet;

            string dirPath = PathMod.ModPath(String.Format(pattern, num));
            try
            {
                if (File.Exists(dirPath))
                {
                    //read file and read binary data
                    using (FileStream stream = File.OpenRead(dirPath))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            sheet = DirSheet.Load(reader);
                            cache.Add(num, sheet);
                            return sheet;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error loading " + dirPath + "\n", ex));
            }
            DirSheet newSheet = DirSheet.LoadError();
            cache.Add(num, newSheet);
            return newSheet;
        }


        public static TileGuide LoadTileIndices(string tileDir)
        {
            TileGuide fullGuide = null;
            try
            {
                Dictionary<string, TileIndexNode> nodes = new Dictionary<string, TileIndexNode>();
                foreach (string modPath in PathMod.FallforthPaths(tileDir + "index.idx"))
                {
                    using (FileStream stream = File.OpenRead(modPath))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            TileGuide guide = TileGuide.Load(reader);
                            foreach (string key in guide.Nodes.Keys)
                                nodes[key] = guide.Nodes[key];
                        }
                    }
                }
                fullGuide = new TileGuide();
                fullGuide.Nodes = nodes;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error reading header at " + tileDir + "\n", ex));
            }
            return fullGuide;
        }

        public static BaseSheet GetTile(TileFrame tileTex)
        {
            long tilePos = TileIndex.GetPosition(tileTex.Sheet, tileTex.TexLoc);
            TileAddr addr = new TileAddr(tilePos, tileTex.Sheet);

            BaseSheet sheet;
            if (tileCache.TryGetValue(addr, out sheet))
                return sheet;

            if (tilePos > 0)
            {
                try
                {
                    using (FileStream stream = new FileStream(PathMod.ModPath(String.Format(TILE_PATTERN, tileTex.Sheet)), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            // Seek to the location of the tile
                            reader.BaseStream.Seek(tilePos, SeekOrigin.Begin);

                            sheet = BaseSheet.Load(reader);
                            tileCache.Add(addr, sheet);
                            return sheet;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(new Exception("Error retrieving tile " + tileTex.TexLoc.X + ", " + tileTex.TexLoc.Y + " from Tileset #" + tileTex.Sheet + "\n", ex));
                }
            }

            BaseSheet newSheet = BaseSheet.LoadError();
            tileCache.Add(addr, newSheet);
            return newSheet;
        }

        public static int GetAnimIndex(string anim)
        {
            return Actions.FindIndex((e) => { return (String.Compare(e.Name, anim, true) == 0); });
        }

        public static int CountPixels(BaseSheet obj)
        {
            return obj.Width * obj.Height;
        }

        public static void DisposeCachedObject(IDisposable obj)
        {
            obj.Dispose();
        }

        public static void SaveScreenshot(Texture2D gameScreen)
        {
            if (!Directory.Exists(PathMod.FromApp(SCREENSHOT_PATH)))
                Directory.CreateDirectory(PathMod.FromApp(SCREENSHOT_PATH));
            string outPath = Text.GetNonConflictingSavePath(PathMod.FromApp(SCREENSHOT_PATH), String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now), ".png");
            using (Stream stream = new FileStream(PathMod.FromApp(SCREENSHOT_PATH) + outPath + ".png", FileMode.Create, FileAccess.Write, FileShare.None))
                BaseSheet.ExportTex(stream, gameScreen);
        }
    }

}
