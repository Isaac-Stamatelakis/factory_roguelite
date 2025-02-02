using System;
using System.Collections.Generic;
using System.Linq;
using UI.Chat;

namespace TileEntity.Instances.Caves.Researcher
{
    internal static class CaveProcessorCommandFactory
    {
        private static readonly Dictionary<string, Func<CaveProcessorUI, ChatCommandToken, CaveProcessorTerminalCommand>> commandMap = new()
        {
            { "help", (processorUI, token) => new HelpCommand(processorUI, token) },
            { "research-cave", (processorUI, token) => new ResearchCommand(processorUI, token) },
            { "download-cave", (processorUI, token) => new DownloadCommand(processorUI, token) },
            { "cave-list", (processorUI, token) => new CaveListCommand(processorUI, token) }
        };

        public static CaveProcessorTerminalCommand GetCommand(CaveProcessorUI processorUI, ChatCommandToken token)
        {
            if (commandMap.TryGetValue(token.Command, out var commandConstructor))
            {
                return commandConstructor(processorUI, token);
            }
            return null;
        }
        
        public static string GetCommandDescriptionText(string command)
        {
            if (commandMap.TryGetValue(command, out var commandConstructor))
            {
                return commandConstructor(null, new ChatCommandToken(command,null)).GetDescription();
            }
            return null;
        }

        public static List<string> GetCommands()
        {
            return commandMap.Keys.ToList();
        }
    }
}
