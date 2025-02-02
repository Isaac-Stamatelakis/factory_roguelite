using System.Collections.Generic;
using UI.Chat;

namespace TileEntity.Instances.Caves.Researcher
{
    internal class HelpCommand : CaveProcessorTerminalCommand
    {
        public HelpCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token) : base(caveProcessorUI, token)
        {
        }

        public override void Execute()
        {
            List<string> commands = CaveProcessorCommandFactory.GetCommands();
            foreach (string command in commands)
            {
                string message = $"{command}\t{CaveProcessorCommandFactory.GetCommandDescriptionText(command).Replace("\n","")}";
                caveProcessorUI.SendTerminalMessage(message);
            }
        }

        public override string GetHelpText()
        {
            return GetDescription();
        }

        public override string GetDescription()
        {
            return "Lists all available commands with a brief description";
        }

        public override List<string> GetAutoFill()
        {
            return CaveProcessorCommandFactory.GetCommands();
        }
    }
}