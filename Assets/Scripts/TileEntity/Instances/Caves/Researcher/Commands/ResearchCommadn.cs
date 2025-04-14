using System.Collections.Generic;
using UI.Chat;
using WorldModule.Caves;

namespace TileEntity.Instances.Caves.Researcher
{
    internal class ResearchCommand : CaveProcessorTerminalCommand
    {
        private const string CANCEL_FLAG = "-c";
        private const string LOW_TIER_FLAG = "-t";
        public ResearchCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token) : base(caveProcessorUI, token)
        {
        }

        public override void Execute()
        {
            string parameter = token.Parameters[0];
            if (parameter == CANCEL_FLAG)
            {
                if (caveProcessorUI.CaveProcessorInstance.ResearchDriveProcess == null)
                    throw new ChatParseException($"No research in progress");
            
                caveProcessorUI.CaveProcessorInstance.ResearchDriveProcess = null;
                return;
            }
            
            List<string> ids = caveProcessorUI.GetCaveIds();
            if (parameter == LOW_TIER_FLAG)
            {
                parameter = GetIdOfLowestUnresearchedTier(ids);
                if (parameter == null) 
                    throw new ChatParseException("No caves to research");
            }

            string id = parameter;
            
            if (caveProcessorUI.CaveProcessorInstance.ResearchDriveProcess != null)
                throw new ChatParseException("Already researching cave");
            
            CaveObject caveObject = caveProcessorUI.LookUpCave(id);
            if (!ids.Contains(id) || !caveObject) 
                throw new ChatParseException($"Invalid research cave ID: {id}");
            
            if (caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(id))
                throw new ChatParseException($"Cave already researched: {id}");

            Tier tier = GetCaveTier(id);
            ResearchDriveProcess researchDriveProcess = new ResearchDriveProcess(0f,false,id);
            caveProcessorUI.CaveProcessorInstance.ResearchDriveProcess = researchDriveProcess;
            if (caveObject.ResearchCost != null && caveObject.ResearchCost.Count > 0)
            {
                caveProcessorUI.SendTerminalMessage($"Research of cave {id} of tier '{tier}' queued up. Insert required items to begin research");
            }
            
            caveProcessorUI.SetDisplayableCave(id);
            caveProcessorUI.DisplayCaveResearchCost();
        }

        private string GetIdOfLowestUnresearchedTier(List<string> caveIds)
        {
            Tier? lowestTier = null;
            string lowestId = null;
            foreach (string id in caveIds)
            {
                if (caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(id)) continue;
                Tier tier = GetCaveTier(id);


                if (lowestTier != null && ((int)tier >= (int)lowestTier)) continue;
                lowestTier = tier;
                lowestId = id;

            }

            return lowestId;
        }
        private Tier GetCaveTier(string id)
        {
            List<CaveObject> caves = caveProcessorUI.Caves;
            foreach (CaveObject cave in caves)
            {
                if (cave.name.Equals(id)) return cave.tier;
            }

            return Tier.Basic;
        }

        public override string GetHelpText()
        {
            return $"{token.Command} [CAVE_ID]... [OPTION]...\n" +
                   $" {CANCEL_FLAG} Cancels current research" +
                   $" {LOW_TIER_FLAG} Researches cave of lowest tier";
        }

        public override string GetDescription()
        {
            return "Begins researching a cave";
        }

        public override List<string> GetAutoFill()
        {
            List<string> values = new List<string>();
            values.Add(CANCEL_FLAG);
            values.Add(LOW_TIER_FLAG);
            foreach (string id in caveProcessorUI.GetCaveIds())
            {
                if (caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(id)) continue;
                values.Add(id);
            }
            return values;
        }
    }
}