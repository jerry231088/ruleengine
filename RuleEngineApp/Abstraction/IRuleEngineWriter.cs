using System.Collections.Generic;

namespace RuleEngineApp.Abstraction
{
    interface IRuleEngineWriter
    {
        void SaveVoilatedRules(List<string> voilatedRuleDataList);
    }
}