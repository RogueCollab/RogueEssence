using System;
using System.Linq;
using System.Text.RegularExpressions;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Utility;


public interface ITitleFilterStrategy
{
    bool Matches(string title, string filter);
}

public class BeginningTitleFilterStrategy : ITitleFilterStrategy
{
    public bool Matches(string title, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        var tokens = Regex.Split(title, @"[\s_:]+");
        return tokens.Any(t => t.StartsWith(filter, StringComparison.OrdinalIgnoreCase));
    }
}

public static class NodeHelper
{
    public static bool FilterRecursive(NodeBase node, string filter, ITitleFilterStrategy filterStrategy)
    {
        bool match = string.IsNullOrWhiteSpace(filter);

       

        match |= filterStrategy.Matches(node.Title, filter);

        bool childMatch = false;
        foreach (var child in node.SubNodes)
        {
            if (FilterRecursive(child, filter, filterStrategy))
                childMatch = true;
        }

        node.IsVisible = match || childMatch;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            node.IsExpanded = match || childMatch;

            if (match && node.SubNodes.Any())
            {
                foreach (var child in node.SubNodes)
                    child.IsVisible = true;
            }
        }

        return node.IsVisible;
    }
}
