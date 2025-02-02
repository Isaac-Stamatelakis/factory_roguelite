using System.Collections.Generic;
using UI.Chat;

namespace TileEntity.Instances.Caves.Researcher
{
   internal class CaveListCommand : CaveProcessorTerminalCommand
    {
        private const string UNKNOWN_FLAG = "-u";
        private const string RESEARCHED_FLAG = "-r";
        private const string TIER_FLAG = "-ut";
        public CaveListCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token) : base(caveProcessorUI, token)
        {
        }

        public override void Execute()
        {
            if (token.Parameters.Length > 1)
                throw new ChatParseException("Too many arguments. At most one option may be provided.");
            List<string> caves = GetCaves();
            string message = TextChatUI.FromArray(caves.ToArray(), " ");
            caveProcessorUI.SendTerminalMessage(message);
        }

        private List<string> GetCaves()
        {
            if (token.Parameters.Length == 0) return caveProcessorUI.GetCaveIds();
            switch (token.Parameters[0])
            {
                case UNKNOWN_FLAG:
                    return GetUnResearchedCaves();
                case TIER_FLAG:
                    List<string> unResearchedCaves = GetUnResearchedCaves(); // TODO
                    return unResearchedCaves;
                case RESEARCHED_FLAG:
                {
                    List<string> known = new List<string>();
                    foreach (string id in caveProcessorUI.GetCaveIds())
                    {
                        if (!caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(id)) continue;
                        known.Add(id);
                    }
                    return known;
                }
                default:
                    throw new ChatParseException($"Unknown flag {token.Parameters[0]}");
            }
        }

        private  List<string> GetUnResearchedCaves()
        {
            List<string> unknown = new List<string>();
            foreach (string id in caveProcessorUI.GetCaveIds())
            {
                if (caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(id)) continue;
                unknown.Add(id);
            }

            return unknown;
        }

        public override string GetHelpText()
        {
            return $"{token.Command} [OPTION]\n" +
                   $" {RESEARCHED_FLAG}    List researched caves\n" +
                   $" {UNKNOWN_FLAG}    List un-researched caves" +
                   $" {TIER_FLAG}   Lists un-researched caves of the lowest tier";
        }

        public override string GetDescription()
        {
            return "Lists caves with optional filters";
        }

        public override List<string> GetAutoFill()
        {
            return new List<string>
            {
                RESEARCHED_FLAG,
                UNKNOWN_FLAG,
                TIER_FLAG
            };
        }
    }
}