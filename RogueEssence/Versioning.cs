using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RogueEssence
{
    public static class Versioning
    {
        public static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static string GetDotNetInfo()
        {
            return RuntimeInformation.FrameworkDescription;
        }
    }
}
