using System.Collections.Generic;
using RuleEngineApp.Classes;

namespace RuleEngineApp.Abstraction
{
    interface IRuleEngine
    {
        List<string> LoadDataStreamInRuleEngine(string dataFileName);
        List<string> ProcessDataStreamInRuleEngine(List<RuleDataStream> ruleDataStreamList);
    }
}