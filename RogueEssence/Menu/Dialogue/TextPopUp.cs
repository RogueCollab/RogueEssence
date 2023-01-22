using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class TextPopUp
    {
        public const int TEXT_HEIGHT = 12;
        public const int FADE_TIME = 50;
        
        protected DialogueText Text;
        
        public int HoldTime;

        private bool fading;
        private bool fadingIn;
        
        private FrameTick CurrentFadeTime;
        private FrameTick timeSinceUpdate;
        public bool Visible { get; set; }

        public Rect Bounds;

        DepthStencilState s1;
        DepthStencilState s2;
        AlphaTestEffect alphaTest;
        public TextPopUp()
        {
            s1 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };

            s2 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.LessEqual,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };
            alphaTest = new AlphaTestEffect(GraphicsManager.GraphicsDevice);
        }

        public void SetMessage(string msg, int holdTime)
        {
            if (msg.Length > 0)
            {
                CurrentFadeTime = FrameTick.FromFrames(0);
                Text = new DialogueText(msg, new Rect(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), TEXT_HEIGHT, false, false, -1);
                Loc textSize = Text.GetTextSize();
                Bounds = new Rect(4, (GraphicsManager.ScreenHeight - textSize.Y), textSize.X, textSize.Y);
                Text.Rect = Bounds;
                HoldTime = holdTime;
                
                timeSinceUpdate = new FrameTick();
                fading = true;
                fadingIn = true;
                Visible = true;
            }
        }
        
        public void Update(FrameTick elapsedTime)
        {
            if (fading)
            {
                if (fadingIn)
                {
                    CurrentFadeTime += elapsedTime;
                    if (CurrentFadeTime >= FADE_TIME)
                    {
                        CurrentFadeTime = FrameTick.FromFrames(FADE_TIME);
                        fading = false;
                    }
                }
                else
                {
                    CurrentFadeTime -= elapsedTime;
                    if (CurrentFadeTime <= 0)
                    {
                        CurrentFadeTime = FrameTick.FromFrames(0);
                        fading = false;
                        Visible = false;
                    }

                }
            }
            else
            {
                if (HoldTime > -1 && timeSinceUpdate >= HoldTime)
                {
                    fading = true;
                    fadingIn = false;
                }
                else
                {
                    timeSinceUpdate += elapsedTime;
                }
            }

            if (Visible)
                Text.TextOpacity = CurrentFadeTime.FractionOf(FADE_TIME);
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;

            spriteBatch.End();
            float scale = GraphicsManager.WindowZoom;
            Matrix zoomMatrix = Matrix.CreateScale(new Vector3(scale, scale, 1));
            Matrix orthMatrix = zoomMatrix * Matrix.CreateOrthographicOffCenter(
                0, GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, 0,
                0, 1);

            alphaTest.Projection = orthMatrix;
            BlendState blend = new BlendState();
            blend.ColorWriteChannels = ColorWriteChannels.None;
            spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, s1, null, alphaTest);

            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height), null, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, s2, null, null, zoomMatrix);
            
            Text.Draw(spriteBatch, Loc.Zero);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, zoomMatrix);
        }
    }
}