using System.Collections.Generic;
using UI.Chat;

namespace TileEntity.Instances.Caves.Researcher
{
    internal abstract class CaveProcessorTerminalCommand
    {
        protected CaveProcessorUI caveProcessorUI;
        protected ChatCommandToken token;
        public abstract void Execute();

        protected CaveProcessorTerminalCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token)
        {
            this.caveProcessorUI = caveProcessorUI;
            this.token = token;
        }

        public abstract string GetHelpText();
        public abstract string GetDescription();
        public abstract List<string> GetAutoFill();
    }
}