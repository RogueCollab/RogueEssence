using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Linq;


namespace RogueEssence.Dev.Views {



public class AutoCompleteBinderBox<T> : AutoCompleteBox
{
    /// <summary>
    /// Optional callback invoked when the user accepts the typed text.
    /// </summary>
    public Action<T> AcceptAction { get; set; }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (e.Key == Key.Enter)
        {
            AcceptTypedText();
            e.Handled = true;
        }
        else
        {
            // Open the dropdown if the box is empty
            if (string.IsNullOrWhiteSpace(Text))
            {
                IsDropDownOpen = true;
                FilterItems();
            }
        }
    }

    protected override void OnLostFocus(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        AcceptTypedText();
    }

    private void AcceptTypedText()
    {
        if (SelectedItem != null) // already selected
        {
            AcceptAction?.Invoke((T)SelectedItem);
            return;
        }

        // Try to match typed text
        if (!string.IsNullOrEmpty(Text))
        {
            var match = ItemsSource.Cast<T>().FirstOrDefault(item =>
                item.ToString().StartsWith(Text, StringComparison.InvariantCultureIgnoreCase));

            if (match != null)
            {
                SelectedItem = match;
                AcceptAction?.Invoke(match);
            }
        }
    }

    /// <summary>
    /// Forces dropdown to show all items when empty
    /// </summary>
    private void FilterItems()
    {
        // Avalonia AutoCompleteBox will filter based on Text.
        // Setting Text="" and reopening dropdown shows all items.
        IsDropDownOpen = true;
    }
}
public class AutoCompleteStringBox : AutoCompleteBinderBox<string>
{
    // Nothing extra is needed — just a type alias for XAML use
}

}