using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Utility;

public class ContextMenuHelper
{
    public static MenuItem CreateMenuItem(string header, string icon, Func<Task> onClick)
    {
        var item = new MenuItem
        {
            Header = header,
            Icon = App.CreateMenuIcon(icon)
        };
        item.Click += async (_, e) =>
        {
            await onClick();
            e.Handled = true;
        };
        return item;
    }
    
    public static ContextMenu CreateContextMenu(params Control[] items)
    {
        var menu = new ContextMenu();
        foreach (var item in items)
            menu.Items.Add(item);
        return menu;
    }
    
    public static ContextMenu CreateDataItemMenu(DataRootNode node, string key)
    {
        return CreateContextMenu(
            CreateMenuItem("Resave as File", "Icons.FileFill", () => node.ResaveItemAsFile(key)),
            CreateMenuItem("Resave as Patch", "Icons.FileTextFill", () => node.ResaveItemAsPatch(key))
        );
    }
    
    public static ContextMenu CreateSpriteItemMenu(SpriteRootNode node)
    {
        return CreateContextMenu(
            CreateMenuItem("Mass Import", "Icons.DownloadFill", async () => await node.MassImportAsync()),
            CreateMenuItem("Mass Export", "Icons.ExportFill", async () => await node.MassExportAsync())
        );
    }
}