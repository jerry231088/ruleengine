using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuleEngineApp.Abstraction;

namespace RuleEngineApp.Classes
{
    public class RuleEngineReader : IRuleEngine
    {
        #region Class Variables
        private static RuleEngineReader _instance = null;
        private readonly string _dirPath;
        private string _strUserDefRule = string.Empty, _intUserDefRule = string.Empty, _dtUserDefRule = string.Empty;
        private Dictionary<string, RuleDataStream> _ruleDsWithValTypeMap = new Dictionary<string, RuleDataStream>();
        #endregion

        private RuleEngineReader(string dirPath)
        {
            _dirPath = dirPath;
        }

        public static RuleEngineReader GetInstance(string dirPath)
        {
            if (_instance == null)
                _instance = new RuleEngineReader(dirPath);
            return _instance;
        }

        #region Abstraction Method
        /// <summary>
        /// Method to Load Data stream json file
        /// </summary>
        /// <param name="dataFileName">json file path</param>
        /// <returns>List of string</returns>
        public List<string> LoadDataStreamInRuleEngine(string dataFileName)
        {
            //Get Rule Data stream details from json file.
            Console.WriteLine("Loading file data in memory.");
            List<RuleDataStream> ruleEngineInputData = Task.Factory.StartNew(() => LoadJsonFile(dataFileName)).Result;

            //Get Rule data stream map
            Dictionary<string, List<string>> ruleDsWithValTypeMap = CreateUserRuleDsMap(ruleEngineInputData);

            //Create user defined rules here
            DefineUserRule(ruleDsWithValTypeMap);

            //Process Rule data stream to get voilated data
            Console.WriteLine("Processing the rule datastream begins.");
            var voilatedSrcList = ProcessDataStreamInRuleEngine(ruleEngineInputData);
            Console.WriteLine("Processing of rule datastream ends.");
            return voilatedSrcList;
        }

        /// <summary>
        /// Method to process data stream in rule engine
        /// </summary>
        /// <param name="ruleDataStreamList">List of RuleDataStream class</param>
        /// <returns>List of string</returns>
        public List<string> ProcessDataStreamInRuleEngine(List<RuleDataStream> ruleDataStreamList)
        {
            List<string> ruleEngineVoilatedSrcList = new List<string>();
            string strVal = string.Empty;
            decimal? decVal = null;
            DateTime? dtVal = null;
            foreach (var rule in ruleDataStreamList)
            {
                //Add Rules here as per the user settings
                //For now 3 hardcoded rules are mentioned
                //ATL1 value should not rise above 240.00
                //ATL2 value should never be LOW
                //ATL3 should not be in future
                switch (rule.Value_Type.ToUpper())
                {
                    case "STRING":
                        strVal = Utility.ConvertToString(rule.Value);
                        break;
                    case "INTEGER":
                        decVal = Utility.ConvertToDecimal(rule.Value);
                        break;
                    case "DATETIME":
                        dtVal = Utility.ConvertToDateTime(rule.Value);
                        break;
                }

                bool isRuleVoilated = VerifyUserRules(strVal, decVal, dtVal);
                if (!isRuleVoilated) continue;
                if (!ruleEngineVoilatedSrcList.Contains(rule.Signal))
                    ruleEngineVoilatedSrcList.Add(rule.Signal);
            }
            return ruleEngineVoilatedSrcList;
        }
        #endregion

        #region Verify and create user's defined rules
        /// <summary>
        /// Method to create user rule data stream map
        /// </summary>
        /// <param name="ruleDataStreamList">List of RuleDataStream class</param>
        /// <returns>Dictionary of Key: string and Value: List of string</returns>
        private Dictionary<string, List<string>> CreateUserRuleDsMap(List<RuleDataStream> ruleDataStreamList)
        {
            Dictionary<string, List<string>> ruleDsWithValTypeMap = new Dictionary<string, List<string>>();
            foreach (RuleDataStream data in ruleDataStreamList)
            {
                if (!ruleDsWithValTypeMap.ContainsKey(data.Value_Type))
                {
                    ruleDsWithValTypeMap[data.Value_Type] = new List<string>
                    {
                        data.Value.ToString()
                    };
                }
                else
                {
                    if (!ruleDsWithValTypeMap[data.Value_Type].Contains(data.Value.ToString()))
                        ruleDsWithValTypeMap[data.Value_Type].Add(data.Value.ToString());
                    //TODO: verify else condition require or not
                }
            }
            return ruleDsWithValTypeMap;
        }

        /// <summary>
        /// Method to verify rules set by user is valid or not
        /// </summary>
        /// <param name="strVal"> string value</param>
        /// <param name="decVal">decimal value</param>
        /// <param name="dtVal">datetime value</param>
        /// <returns>boolean value</returns>
        private bool VerifyUserRules(string strVal, decimal? decVal, DateTime? dtVal)
        {
            bool ruleVoilated = false;
            if (!string.IsNullOrEmpty(strVal))
            {
                string strCheck = _strUserDefRule == Utility.StrRuleType.HIGH.ToString() ? Utility.StrRuleType.HIGH.ToString()
                    : Utility.StrRuleType.LOW.ToString();
                ruleVoilated = string.Equals(strVal.ToUpper(), strCheck) ? false : true;
            }
            else if (decVal != null)
            {
                var splitRule = _intUserDefRule.Split(' ', '\t');
                decimal val = Convert.ToDecimal(splitRule[1]);
                switch (splitRule[0])
                {
                    case "<":
                        ruleVoilated = decVal < val ? true : false;
                        break;
                    case "<=":
                        ruleVoilated = decVal <= val ? true : false;
                        break;
                    case ">=":
                        ruleVoilated = decVal >= val ? true : false;
                        break;
                    case "=":
                        ruleVoilated = decVal == val ? true : false;
                        break;
                    case "<>":
                        ruleVoilated = decVal != val ? true : false;
                        break;
                    case ">":
                    default:
                        ruleVoilated = decVal > val ? true : false;
                        break;
                }
            }
            else if (dtVal != null)
            {
                int result = DateTime.Compare(Convert.ToDateTime(dtVal), DateTime.Now);
                switch (_dtUserDefRule)
                {
                    case "FUTURE":
                        ruleVoilated = result > 0 ? true : false;
                        break;
                    case "PAST":
                        ruleVoilated = result <= 0 ? true : false;
                        break;
                }
            }
            return ruleVoilated;
        }

        /// <summary>
        /// Method to give ability to user to set rules
        /// </summary>
        /// <param name="ruleDsWithValTypeMap">Dictionary of Key: string and Value: List of string</param>
        private void DefineUserRule(Dictionary<string, List<string>> ruleDsWithValTypeMap)
        {
            string strUserRule = string.Empty, intUserRule = string.Empty, dtUserRule = string.Empty;
            foreach (var valtype in ruleDsWithValTypeMap)
            {
                bool valEnteredInRange = false;
                Console.WriteLine("Please define the rule for Value type: " + valtype.Key);
                switch (valtype.Key.ToUpper())
                {
                    case "STRING":
                        string valSelect = string.Join(" or ", valtype.Value);
                        Console.WriteLine("Please select from the given values: " + valSelect);
                        strUserRule = Console.ReadLine();
                        valEnteredInRange = CheckUserInputRules(strUserRule, valtype.Value, valtype.Key.ToUpper());
                        if (valEnteredInRange) _strUserDefRule = strUserRule;
                        break;
                    case "INTEGER":
                        var intList = new List<decimal>();
                        intList = Utility.CreateDecList(valtype.Value);
                        var maxVal = intList.Max();
                        var minVal = intList.Min();
                        Console.WriteLine("Please enter your rule between the given range: " + minVal + " and " + maxVal +
                            ". (Please specify like this > 230, keep one space between comparison and value and use comparison within (> , <, <=,>=,=,<>))");
                        intUserRule = Console.ReadLine();
                        valEnteredInRange = CheckUserInputRules(intUserRule, valtype.Value, valtype.Key.ToUpper());
                        if (valEnteredInRange) _intUserDefRule = intUserRule;
                        break;
                    case "DATETIME":
                        DateTime minDate = DateTime.MaxValue;
                        DateTime maxDate = DateTime.MinValue;
                        foreach (string dateString in valtype.Value)
                        {
                            DateTime date = DateTime.Parse(dateString);
                            if (date < minDate)
                                minDate = date;
                            if (date > maxDate)
                                maxDate = date;
                        }
                        Console.WriteLine("Given are the lowest and highest dates: " + minDate + " - " + maxDate +
                            " . Please specify 'Past or Future'.");
                        dtUserRule = Console.ReadLine();
                        valEnteredInRange = CheckUserInputRules(dtUserRule, valtype.Value, valtype.Key.ToUpper());
                        if (valEnteredInRange) _dtUserDefRule = dtUserRule;
                        break;
                }
            }
        }

        /// <summary>
        /// Method to check rules input by user
        /// </summary>
        /// <param name="valEntered">value entered as per value type</param>
        /// <param name="strList">List of string</param>
        /// <param name="valType">value type as mentioned in json file</param>
        /// <returns>boolean value</returns>
        private bool CheckUserInputRules(string valEntered, List<string> strList, string valType)
        {
            bool correct = false;
            switch (valType)
            {
                case "STRING":
                    if (!valEntered.ToUpper().Equals("HIGH") && !valEntered.ToUpper().Equals("LOW")) correct = false;
                    else correct = true;
                    break;
                case "INTEGER":
                    var newVal = valEntered.Split(' ', '\t');
                    if (newVal.Length > 1)
                    {
                        var intList = new List<decimal>();
                        intList = Utility.CreateDecList(strList);
                        var maxVal = intList.Max();
                        var minVal = intList.Min();
                        bool parsed = decimal.TryParse(newVal[1], out decimal newintVal);
                        if (parsed)
                            if (newintVal >= minVal && newintVal <= maxVal)
                                correct = true;
                    }
                    break;
                case "DATETIME":
                    if (!valEntered.ToUpper().Equals("PAST") && !valEntered.ToUpper().Equals("FUTURE")) correct = false;
                    else correct = true;
                    break;
            }
            if (!correct)
            {
                Console.WriteLine("Incorrect value entered. Please enter as specified.");
                valEntered = Console.ReadLine();
                correct = CheckUserInputRules(valEntered, strList, valType);
            }
            return correct;
        }

        private List<RuleDataStream> LoadJsonFile(string dataFileName)
        {
            List<RuleDataStream> ruleEngineInputData = new List<RuleDataStream>();
            using (StreamReader sr = new StreamReader(_dirPath + dataFileName))
            {
                string jsonData = sr.ReadToEnd();
                ruleEngineInputData = JsonConvert.DeserializeObject<List<RuleDataStream>>(jsonData);
            }
            return ruleEngineInputData;
        }
        #endregion
    }
}