#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using RogueEssence.Content;
using RogueEssence.Dev;
using RogueEssence.Script;
using RogueElements;
#endregion

namespace RogueEssence
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameBase : Game
    {
        private const int SPLASH_BLINK_FRAMES = 40;
        private const int SPLASH_FADE_FRAMES = 20;

        public enum LoadPhase
        {
            Error = -1,
            System = 0,
            Content = 1,
            Ready = 2,
            Unload = 3
        }

        public static LoadPhase CurrentPhase;
        public static object lockObj = new object();

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        private int fadeFrames;
        private int splashFrames;
        private bool drawTurn;
        private bool backgroundLoaded;
        private bool firstUpdate;

        public GameBase()
            : base()
        {
            IsFixedTimeStep = true;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            GraphicsManager.InitBase(graphics, DiagManager.Instance.CurSettings.Window);
            IsMouseVisible = true;
            Window.Title = Text.FormatKey("GAME_TITLE");
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            DiagManager.Instance.LoadMsg = "Loading System";
            GraphicsManager.InitSystem(GraphicsDevice);

            CurrentPhase = LoadPhase.Content;
            
            LuaEngine.InitInstance();
            GraphicsManager.InitStatic();
            
#if NO_THREADING
            LoadInBackground();
#else
            Thread thread = new Thread(LoadInBackground);
            thread.IsBackground = true;
            //thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            //thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            thread.Start();
#endif
            base.Initialize();
        }

        void LoadInBackground()
        {
            try
            {
                SoundManager.InitStatic();

                DiagManager.Instance.LoadMsg = "Loading Content";
                Data.DataManager.InitInstance();
                Data.DataManager.Instance.InitData();
                Network.NetworkManager.InitInstance();
                Menu.MenuManager.InitInstance();
                GameManager.InitInstance();
                Dungeon.ZoneManager.InitInstance();
                Dungeon.DungeonScene.InitInstance();
                Ground.GroundScene.InitInstance();
            
                DungeonEditScene.InitInstance();
                GroundEditScene.InitInstance();
                //Notify script engine
                LuaEngine.Instance.OnDataLoad();
                GameManager.Instance.Begin();

          
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            backgroundLoaded = true;
            DiagManager.Instance.LoadMsg = "Press any key to continue";
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            //while (!backgroundLoaded)
            //    Thread.Sleep(100);

            Data.DataManager.Instance.Unload();
            GraphicsManager.Unload();
            DiagManager.Instance.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Updating completely ignores delta time and updates by-frames
            //It also waits until drawing ends (which is locked at 60FPS) to ensure that updating does not take *shorter* than 1/60 of a second
            //This is because the following needs to be achieved:
            //Perfect simulation whether the updates were fast or slow
            //And for the game to not frame skip if the updates were too slow (deltatime too high), even if it meant slowing down the game itself.
            if (!drawTurn && CurrentPhase > LoadPhase.Error)
            {
                if (!firstUpdate && CurrentPhase > LoadPhase.System)
                {
                    splashFrames++;
                    if (backgroundLoaded)
                    {
                        if (DiagManager.Instance.DevMode)
                        {
                            try
                            {
                                DiagManager.Instance.DevEditor.Load(this);
                            }
                            catch (Exception ex)
                            {
                                DiagManager.Instance.LogError(ex);
                                throw;
                            }

                            while (!DiagManager.Instance.DevEditor.LoadComplete)
                                Thread.Sleep(10);

                            CurrentPhase = LoadPhase.Ready;
                        }
                        else if (fadeFrames == 0)
                        {
                            if (Keyboard.GetState().GetPressedKeys().Length > 0 || GamePad.GetState(PlayerIndex.One).Buttons != new GamePadButtons())
                                fadeFrames++;
                        }
                        else
                        {
                            if (fadeFrames < SPLASH_FADE_FRAMES)
                                fadeFrames++;
                            else
                                CurrentPhase = LoadPhase.Ready;
                        }
                    }
                }

                if (CurrentPhase == LoadPhase.Ready)
                {
                    try
                    {
                        DiagManager.Instance.DevEditor.Update(gameTime);

                        lock (lockObj)
                        {
                            SoundManager.NewFrame();

                            FrameInput input = new FrameInput();
                            if (DiagManager.Instance.ActiveDebugReplay != null && DiagManager.Instance.DebugReplayIndex < DiagManager.Instance.ActiveDebugReplay.Count)
                            {
                                input = DiagManager.Instance.ActiveDebugReplay[DiagManager.Instance.DebugReplayIndex];
                                DiagManager.Instance.DebugReplayIndex++;
                                if (IsActive)
                                    input.ReadDevInput(Keyboard.GetState(), Mouse.GetState(), !DiagManager.Instance.DevEditor.AteKeyboard, !DiagManager.Instance.DevEditor.AteMouse);
                            }
                            else //set this frame's input
                                input = new FrameInput(GamePad.GetState(PlayerIndex.One), Keyboard.GetState(), Mouse.GetState(), !DiagManager.Instance.DevEditor.AteKeyboard, !DiagManager.Instance.DevEditor.AteMouse, IsActive, GraphicsManager.GetGameScreenOffset());
                            
                            if (DiagManager.Instance.ActiveDebugReplay == null)
                                DiagManager.Instance.LogInput(input);
                            
                            DiagManager.Instance.UpdateGamePadActive(input.HasGamePad);

                            GameManager.Instance.SetMetaInput(input);
                            GameManager.Instance.UpdateMeta();
                            GameManager.Instance.SetFrameInput(input);
                            GameManager.Instance.Update();
                            LuaEngine.Instance.Update(gameTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        DiagManager.Instance.LogError(ex);
                    }

                    firstUpdate = true;//allow drawing now
                }
                else if (CurrentPhase == LoadPhase.Unload)
                    Exit();
                drawTurn = true;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (CurrentPhase == LoadPhase.Content || CurrentPhase == LoadPhase.Error)
            {
                float scale = GraphicsManager.WindowZoom;
                Matrix matrix = Matrix.CreateScale(new Vector3(scale, scale, 1));
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, matrix);

                //draw the splash screen
                float alpha = (float)(SPLASH_FADE_FRAMES - fadeFrames) / SPLASH_FADE_FRAMES;
                GraphicsManager.Splash.Draw(spriteBatch,
                    new Vector2((GraphicsManager.ScreenWidth - GraphicsManager.Splash.Width) / 2, (GraphicsManager.ScreenHeight - GraphicsManager.Splash.Height) / 2),
                    null, Color.White * (alpha * 0.5f));

                if (CurrentPhase == LoadPhase.Content)
                {
                    GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.ScreenWidth / 2, 16,
                        Text.FormatKey("GAME_SPLASH"), null, DirV.Up, DirH.None, Color.White * alpha);

                    if (DiagManager.Instance.DevMode)
                    {
                        GraphicsManager.SysFont.DrawText(spriteBatch, 0, GraphicsManager.ScreenHeight,
                                    DiagManager.Instance.LoadMsg, null, DirV.Down, DirH.Left);
                    }
                    else if (backgroundLoaded && fadeFrames == 0 && splashFrames / SPLASH_BLINK_FRAMES % 2 == 0)
                    {
                        GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.ScreenWidth / 2, GraphicsManager.ScreenHeight - 2,
                                        DiagManager.Instance.LoadMsg, null, DirV.Down, DirH.None);
                    }
                }
                else
                {
                    GraphicsManager.SysFont.DrawText(spriteBatch, GraphicsManager.ScreenWidth / 2, 16,
                        Text.FormatKey("GAME_ERROR_SPLASH"), null, DirV.Up, DirH.None, Color.White * alpha);
                }


                spriteBatch.End();
            }
            else if (CurrentPhase == LoadPhase.Ready && firstUpdate)
            {
                lock (lockObj)
                {
                    try
                    {
                        GameManager.Instance.Draw(spriteBatch, gameTime.ElapsedGameTime.TotalSeconds);

                    }
                    catch (Exception ex)
                    {
                        DiagManager.Instance.LogError(ex);
                        try
                        {
                            spriteBatch.End();
                        }
                        catch (Exception ex2)
                        {
                            DiagManager.Instance.LogError(ex2);
                        }
                    }
                }

                DiagManager.Instance.DevEditor.Draw();
            }

            drawTurn = false;

            base.Draw(gameTime);
        }
    }
}
