using System;
using System.Linq;
using System.Text.RegularExpressions;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Utility;


public interface ITitleFilterStrategy
{
    bool Matches(string title, string filter);
}

// Matches the beginning of each token word in the title
public class BeginningTitleFilterStrategy : ITitleFilterStrategy
{

    public bool Matches(string title, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        var titleTokens = Regex.Split(title, @"[\s_:]+");
        var filterTokens = Regex.Split(filter.Trim(), @"[\s_:]+");
        
        for (int i = 0; i <= titleTokens.Length - filterTokens.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < filterTokens.Length; j++)
            {
                bool isLast = j == filterTokens.Length - 1;
                bool tokenMatch = isLast
                    ? titleTokens[i + j].StartsWith(filterTokens[j], StringComparison.OrdinalIgnoreCase)
                    : titleTokens[i + j].Equals(filterTokens[j], StringComparison.OrdinalIgnoreCase);

                if (!tokenMatch)
                {
                    match = false; break;
                }
            }
            if (match) return true;
        }

        return false;
    }
}

// Matches any titles that satisfies any one of the strategies
public class CombinedTitleFilterStrategy : ITitleFilterStrategy
{
    private readonly ITitleFilterStrategy[] _strategies;

    public CombinedTitleFilterStrategy(params ITitleFilterStrategy[] strategies)
    {
        _strategies = strategies;
    }

    public bool Matches(string title, string filter)
    {
        return _strategies.Any(s => s.Matches(title, filter));
    }
}

// Matches the index number of the monster, e.g "#299" with "299"
public class IndexTitleFilterStrategy : ITitleFilterStrategy
{
    public bool Matches(string title, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        var match = Regex.Match(title, @"#(\d+)");
        if (!match.Success)
            return false;

        return match.Groups[1].Value.StartsWith(filter.TrimStart('#'), StringComparison.OrdinalIgnoreCase);
    }
}

public static class NodeHelper
{
    
    // Expands the top level nodes if the child matches the filter. Also the node expands itself if it matches the filter.
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
    
    public static void ExpandIfMatches(NodeBase node, string filter, ITitleFilterStrategy filterStrategy)
    {

        if (filterStrategy.Matches(node.Title, filter))
        {
            ExpandAll(node, true);
        }
        else
        { 
            node.IsVisible = false;
        }
    }

    public static void ExpandAll(NodeBase node, bool expanded)
    {
        node.IsVisible = true;
        node.IsExpanded = expanded;
        foreach (var child in node.SubNodes)
            ExpandAll(child, expanded);
    }
    
    public static void ExpandParents(NodeBase node, bool expanded)
    {
        node.IsExpanded = expanded;
    
        var parent = node.Parent;
        while (parent != null)
        {
            parent.IsExpanded = expanded;
            parent = parent.Parent;
        }
    }
}
