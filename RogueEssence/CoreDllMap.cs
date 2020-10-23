/* CoreDllMap - .NET Core Implementation of Mono's DllMap
 *
 * Copyright (c) 2020 Caleb Cornett
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Caleb Cornett <caleb.cornett@outlook.com>
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

public static class CoreDllMap
{
    public static string OS => os;
    public static string CPU => cpu;

    private static string os;
    private static string cpu;

    private static Dictionary<Assembly, bool> registeredAssemblies
        = new Dictionary<Assembly, bool>();

    private static Dictionary<string, string> mapDictionary
        = new Dictionary<string, string>();

    public static void Init()
    {
        // Get platform and CPU
        os = GetCurrentPlatform();
        cpu = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
    }

    public static void Register(Assembly assembly)
    {
        // An assembly can only be registered once
        if (registeredAssemblies.ContainsKey(assembly))
            return;

        // Read config XML and store details within MapDictionary
        string xmlPath = Path.Combine(
            Path.GetDirectoryName(assembly.Location),
            Path.GetFileNameWithoutExtension(assembly.Location) + ".dll.config"
        );
        if (!File.Exists(xmlPath))
        {
            // Maybe it's called app.config?
            xmlPath = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                "app.config"
            );
            if (!File.Exists(xmlPath))
            {
                // Oh well!
                return;
            }
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);
        ParseXml(xmlDoc);

        // Set the resolver callback
        NativeLibrary.SetDllImportResolver(assembly, MapAndLoad);

        // Mark the assembly as registered
        registeredAssemblies.Add(assembly, true);
    }

    // The callback which loads the mapped library in place of the original
    private static IntPtr MapAndLoad(
        string libraryName,
        Assembly assembly,
        DllImportSearchPath? dllImportSearchPath
    )
    {
        string mappedName;
        if (!mapDictionary.TryGetValue(libraryName, out mappedName))
        {
            mappedName = libraryName;
        }
        return NativeLibrary.Load(mappedName, assembly, dllImportSearchPath);
    }

    private static void ParseXml(XmlDocument xml)
    {
        foreach (XmlNode node in xml.GetElementsByTagName("dllmap"))
        {
            // Ignore entries for other OSs
            if (!node.Attributes["os"].Value.Contains(os))
            {
                continue;
            }

            // Ignore entries for other CPUs
            XmlAttribute cpuAttribute = node.Attributes["cpu"];
            if (cpuAttribute != null && !cpuAttribute.Value.Contains(cpu))
            {
                continue;
            }

            string oldLib = node.Attributes["dll"].Value;
            string newLib = node.Attributes["target"].Value;
            if (string.IsNullOrWhiteSpace(oldLib) || string.IsNullOrWhiteSpace(newLib))
            {
                continue;
            }

            // Don't allow duplicates
            if (mapDictionary.ContainsKey(oldLib))
            {
                continue;
            }

            mapDictionary.Add(oldLib, newLib);
        }
    }

    private static string GetCurrentPlatform()
    {
        string[] platformNames = new string[]
        {
            "LINUX",
            "OSX",
            "WINDOWS",
            "FREEBSD",
            "NETBSD",
            "OPENBSD"
        };

        for (int i = 0; i < platformNames.Length; i += 1)
        {
            OSPlatform platform = OSPlatform.Create(platformNames[i]);
            if (RuntimeInformation.IsOSPlatform(platform))
            {
                return platformNames[i].ToLowerInvariant();
            }
        }

        return "unknown";
    }
}