using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Item.Slot;
using Items;
using Items.Tags;
using Newtonsoft.Json;
using Player;
using PlayerModule;

namespace UI.Chat {
    public class ItemTagCommand : ChatCommand, IAutoFillChatCommand
    {
        public ItemTagCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        private readonly Dictionary<string, Action<ItemSlot,string[],TextChatUI>> funcOptionMap = new Dictionary<string, Action<ItemSlot,string[],TextChatUI>>
        {
            { "print", PrintTagData },
            { "set", SetTagData}
        };

        public override void execute()
        {
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            ItemSlot selected = playerScript.PlayerInventory.getSelectedItemSlot();
            if (ItemSlotUtils.IsItemSlotNull(selected)) throw new ChatParseException("Cannot assign tag as no item selected");

            string commandType = parameters[0];
    
            if (!funcOptionMap.TryGetValue(commandType, out var value)) throw new ChatParseException("Invalid command modifier: " + commandType);
            value.Invoke(selected, parameters,chatUI);
            
        }

        private static void PrintTagData(ItemSlot itemSlot, string[] parameters, TextChatUI textChatUI)
        {
            if (itemSlot.tags?.Dict == null || itemSlot.tags.Dict.Count == 0)
            {
                textChatUI.sendMessage("No tag data");
                return;
            }
            
            foreach (var (tag, tagData) in itemSlot.tags.Dict)
            {
                string sData = tag.serialize(itemSlot.tags);
                textChatUI.sendMessage($"{tag} : {sData}");
            }
        }
    
        private static void SetTagData(ItemSlot selected, string[] parameters, TextChatUI textChatUI)
        {
            string tagName = parameters[1];
            ItemTag? nullableTag = FromString(tagName);
            if (nullableTag == null) throw new ChatParseException($"Unknown tag '{tagName}'");
            ItemTag tag = (ItemTag)nullableTag;
            
            string data = parameters[2];

            if (selected.tags?.Dict == null) selected.tags = new ItemTagCollection(new Dictionary<ItemTag, object>());

            try
            {
                var dataObject = tag.deseralize(data);
                if (dataObject == null) throw new ChatParseException("Invalid tag data");
                selected.tags.Dict[tag] = dataObject;
            }
            catch (Exception e) when (e is JsonSerializationException or NullReferenceException)
            {
                throw new ChatParseException("Invalid tag data");
            }
        }
        public static ItemTag? FromString(string tagName)
        {
            var tags = (ItemTag[])Enum.GetValues(typeof(ItemTag));
            List<string> tagStrings = new List<string>();
            foreach (var tag in tags)
            {
                if (tagName.Equals(tag.ToString().ToLower())) return tag;
            }

            return null;
        }
        
        public override string getDescription()
        {
            return "/itemtag ('print' or 'set') 'tag' 'json'\nAssigns item the given tag with data";
        }

        public List<string> getAutoFill(int paramIndex)
        {
            switch (paramIndex)
            {
                case 0:
                    return funcOptionMap.Keys.ToList();
                case 1:
                {
                    var tags = (ItemTag[])Enum.GetValues(typeof(ItemTag));
                    List<string> tagStrings = new List<string>();
                    foreach (var tag in tags)
                    {
                        tagStrings.Add(tag.ToString().ToLower());
                    }

                    return tagStrings;
                }
                default:
                    return null;
            }
        }
    }
}

