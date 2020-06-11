using System.Collections.Generic;
using Networking;
using UI;
using Utility;

namespace GameDirection
{
    public class FreeplayDirector : GameDirector
    {
        public override GameModeEnum GameMode => GameModeEnum.FreePlay;

        public override List<EscapeMenuButtonInfo> GetEscapeMenuButtonInfo()
        {
            var result = base.GetEscapeMenuButtonInfo();
            result.Add(
                new EscapeMenuButtonInfo("Reset Robot", MetaUtility.UnityEventFromFunc(ResetLocalRobot)));
            
            return result;
        }
    }
}