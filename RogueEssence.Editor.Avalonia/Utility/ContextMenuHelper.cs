using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using RogueEssence.Data;
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
    
    public static ContextMenu CreateDataRootMenu(DataRootNode node)
    {
        var menu = CreateContextMenu(
            CreateMenuItem("Re-Index", "Icons.FileFill", async () => await node.ReIndexAsync())
        );

        if (node.DataType != DataManager.DataType.AutoTile)
        {
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateMenuItem("Resave all as File", "Icons.FileFill", () => node.ResaveAllAsync(false)));
            menu.Items.Add(CreateMenuItem("Resave all as Patch", "Icons.FileTextFill", () => node.ResaveAllAsync(true)));
        }
        return menu;
    }
    
    public static ContextMenu CreateDataItemMenu(DataRootNode node, string key)
    {
        return CreateContextMenu(
            CreateMenuItem("Resave as File", "Icons.FileFill", () => node.ResaveItemAsFile(key)),
            CreateMenuItem("Resave as Patch", "Icons.FileTextFill", () => node.ResaveItemAsPatch(key))
        );
    }
    
    public static ContextMenu CreateSpriteRootMenu(SpriteRootNode root)
    {
        var menu = CreateContextMenu(
            CreateMenuItem("Mass Import", "Icons.DownloadFill", async () => await root.MassImportAsync()),
            CreateMenuItem("Mass Export", "Icons.ExportFill", async () => await root.MassExportAsync())
        );
        
        if (root is SpriteTileRootNode node)
        {
            menu.Items.Add(new Separator());
            var reIndexItem = new MenuItem { Header = "Re-Index", Icon = App.CreateMenuIcon("Icons.ListNumbersFill") };
            reIndexItem.Click += async (_, _) => await node.ReIndexAsync();
            menu.Items.Add(reIndexItem);
        };

        return menu;

    }
    
    public static ContextMenu CreateUniversalRootMenu(UniversalNode node)
    {
        return CreateContextMenu(
            CreateMenuItem("Resave as File", "Icons.DownloadFill", async () => await node.ResaveAsFile()),
            CreateMenuItem("Resave as Patch", "Icons.ExportFill", async () => await node.ResaveAsDiff())
        );
    }
}