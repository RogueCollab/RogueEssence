
using System.Collections.Generic;

using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
  //TODO: still don't know what class to extend.
  public class DepositAllMenu {
    
    // Store all items, except for held items.
    public DepositAllMenu() {

      List<InvItem> items = new List<InvItem>();
      int item_count = DataManager.Instance.Save.ActiveTeam.GetInvCount();
    
      for (int i = 0; i < item_count; i++) {
        // Get a list of inventory items.
        InvItem item = DataManager.Instance.Save.ActiveTeam.GetInv(i);
        items.Add(item);
      };

      // Store all items in the inventory.
      DataManager.Instance.Save.ActiveTeam.StoreItems(items);

     // Remove the items back to front to prevent removing them in the wrong order.
     for (int i = items.Count - 1; i >= 0; i--) {
        DataManager.Instance.Save.ActiveTeam.RemoveFromInv(i);
     }

      }
      
    }

  }


