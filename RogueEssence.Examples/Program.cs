#region Using Statements
using System;
using System.Threading;
using System.Globalization;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using RogueEssence;
#endregion

namespace RogueEssence.Examples
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //TODO: figure out how to set this switch in appconfig
            AppContext.SetSwitch("Switch.System.Runtime.Serialization.SerializationGuard.AllowFileWrites", true);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            DiagManager.InitInstance();
            //GraphicsManager.InitParams();
            //Text.Init();
            //Text.SetCultureCode("");


            try
            {
                DiagManager.Instance.LogInfo("=========================================");
                DiagManager.Instance.LogInfo(String.Format("SESSION STARTED: {0}", String.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)));
                DiagManager.Instance.LogInfo("Version: " + Versioning.GetVersion().ToString());
                DiagManager.Instance.LogInfo(Versioning.GetDotNetInfo());
                DiagManager.Instance.LogInfo("=========================================");


                DiagManager.Instance.DevEditor = new EmptyEditor();

                using (GameBase game = new GameBase())
                    game.Run();

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                throw ex;
            }
        }
    }
}
