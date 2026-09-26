using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace RogueEssence.Dev.Native
{
    [SupportedOSPlatform("macOS")]
    internal class MacOS : OS.IBackend
    {
        public void SetupApp(AppBuilder builder)
        {
            builder.With(new MacOSPlatformOptions()
            {
                DisableDefaultApplicationMenuItems = true,
            });
        }

        public void SetupWindow(Window window)
        {
            window.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
            window.ExtendClientAreaToDecorationsHint = true;
        }

    }
}
