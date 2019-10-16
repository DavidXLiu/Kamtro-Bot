using Discord.WebSocket;
using Kamtro_Bot.Items;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// Node containing data on the user's invnetory
    /// (I think this was on Retropolis)
    /// To Be Implemented, currently here for a field in <see cref="UserDataNode"/>
    /// </summary>
    public class UserInventoryNode
    {
        public Dictionary<uint, int> Items;

        public UserInventoryNode() {
            Items = new Dictionary<uint, int>();
        }

        public void AddItem(uint id, int count = 1) {
            if (!Items.ContainsKey(id)) Items[id] = 0;

            Items[id] += count;
        }

        /// <summary>
        /// Takes away a certain number of an item from the user's inventory
        /// </summary>
        /// <remarks>
        /// Precondition:
        /// The interaction is valid, AKA They're not taking out more items than the user has,
        /// They're not taking out an invalid item, etc.
        /// </remarks>
        /// <param name="id">The ID of the item</param>
        /// <param name="count">The number to take away</param>
        public void LoseItem(uint id, int count = 1) {
            Items[id] -= count;

            ParseInventory();
        }

        public bool HasItem(uint id) {
            if (!Items.ContainsKey(id)) return false;

            if(Items[id] <= 0) {
                Items.Remove(id);
                UserInventoryManager.SaveInventories();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to craft an item
        /// </summary>
        /// <remarks>
        /// Only saves on successful crafting
        /// </remarks>
        /// <param name="toCraft">The ID of the item to be crafted</param>
        /// <returns>True if crafting was successful, false otherwise</returns>
        public bool TryCraft(uint toCraft) {
            Item i = ItemManager.GetItem(toCraft);

            if(i.IsCraftable() && CanCraft(toCraft)) {
                Craft(toCraft);
                UserInventoryManager.SaveInventories(); 
            } else {
                return false;
            }

            return true;
        }

        public bool CanCraft(uint item, int quantity = 1) {
            Item i = ItemManager.GetItem(item);

            foreach(uint k in i.GetRecipe().Keys) {
                if (!Items.ContainsKey(k)) return false;
                if (Items[k] < i.GetRecipe()[k]*quantity) return false;
            }

            return true;
        }

        /// <summary>
        /// Counts the number of items (or the number of a specific item) in the inventory.
        /// </summary>
        /// <param name="item">The ID of the item to get the count of</param>
        /// <returns>The count of the specified item, or the total number of items if no item is specified</returns>
        public int ItemCount(uint? item = null) {
            // check if an item was specified
            if (item == null) {
                // if it wasn't, get the total count of all items
                int t = 0;  // variable to store item total
                bool cleanup = false;  // bool for cleanup so if it 

                foreach(uint k in Items.Keys) {
                    int i = Items[k];

                    // check to make sure the items don't have bizarre counts
                    if (i <= 0) {
                        KLog.Debug($"[UIN] Item [{k}] had count {i}, and was removed.");
                        cleanup = true;  // if so, make sure to clean up the inventory at the end of the method
                        continue;  // and skip the item
                    }

                    t += i;  // add it to the total
                }

                if(cleanup) {
                    // if the inventory needs cleaning, do so.
                    ParseInventory();
                }

                return t;  // return the total
            } else {
                // The item was specified, so count it
                uint i = item.Value;

                if(!Items.ContainsKey(i)) {
                    // error checks
                    return 0;
                }

                if(Items[i] <= 0) {
                    // Fix broken inventories
                    ParseInventory();
                    return 0;
                }

                return Items[i];
            }
        }

        /// <summary>
        /// Counts the number of items (or the number of a specific item) in the inventory.
        /// </summary>
        /// <param name="i">The item to get the count of</param>
        /// <returns>The count of the specified item, or the total number of items if no item is specified</returns>
        public int ItemCount(Item i) {
            if (i == null) return 0;

            return ItemCount(i.Id);
        }

        /// <summary>
        /// Crafts the item. No checks in method
        /// </summary>
        /// <param name="item">The item to craft</param>
        private void Craft(uint itemId) {
            Item item = ItemManager.GetItem(itemId);

            foreach(uint k in item.GetRecipe().Keys) {
                LoseItem(k, item.GetRecipe()[k]);
            }

            AddItem(itemId);

            KLog.Info($"User {BotUtils.GetFullUsername(BotUtils.GetGUser(UserDataManager.UserData.FirstOrDefault(x => x.Value.Inventory == this).Key))} crafted item {item.Name} (ID: {itemId})");

            ParseInventory();
        }

        private void ParseInventory() {
            bool save = false;

            foreach(uint k in Items.Keys) {
                if (Items[k] <= 0) {
                    Items.Remove(k);
                    save = true;
                }
            }

            if (save) UserInventoryManager.SaveInventories();
        }
    }
}
