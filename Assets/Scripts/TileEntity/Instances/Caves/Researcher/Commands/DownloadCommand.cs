using System.Collections.Generic;
using System.Linq;
using UI.Chat;

namespace TileEntity.Instances.Caves.Researcher
{
    internal class DownloadCommand : CaveProcessorTerminalCommand
    {
        private const string VERBOSE_FLAG = "-v";
        private const string RESET_FLAG = "-r";
        public DownloadCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token) : base(caveProcessorUI, token)
        {
        }

        public override void Execute()
        {
            string parameter = token.Parameters[0];

            if (parameter == VERBOSE_FLAG)
            {
                string message = caveProcessorUI.CaveProcessorInstance.CurrentlyCopyingCave ?? "None"; 
                caveProcessorUI.SendTerminalMessage($"Copy cave set to '{message}'");
                return;
            }

            if (parameter == RESET_FLAG)
            {
                caveProcessorUI.CaveProcessorInstance.CurrentlyCopyingCave = null;
                caveProcessorUI.SendTerminalMessage($"Copy cave set to 'None'");
                return;
            }
            string cave = parameter;
            if (!caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(cave))
                throw new ChatParseException($"Cannot download unknown cave '{cave}'");
            if (cave.Equals(caveProcessorUI.CaveProcessorInstance.CurrentlyCopyingCave))
                throw new ChatParseException($"Already downloading '{cave}'");
            
            caveProcessorUI.CaveProcessorInstance.CurrentlyCopyingCave = cave;
            caveProcessorUI.CaveProcessorInstance.InventoryUpdate(0);
        }

        public override string GetHelpText()
        {
            return $"{token.Command} [CAVE_ID]... [OPTION]...\n" +
                   $" {VERBOSE_FLAG}    Prints current cave being copied into drives" +
                   $" {RESET_FLAG} Sets copy cave to none";
        }

        public override string GetDescription()
        {
            return "Sets a researched cave to be copied into drives";
        }

        public override List<string> GetAutoFill()
        {
            List<string> strings = caveProcessorUI.CaveProcessorInstance.ResearchedCaves.ToList(); // Create copy
            strings.Add(VERBOSE_FLAG);
            strings.Add(RESET_FLAG);
            return strings;
        }
    }
}