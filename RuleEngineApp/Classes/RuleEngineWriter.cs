using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RuleEngineApp.Abstraction;

namespace RuleEngineApp.Classes
{
    public class RuleEngineWriter : IRuleEngineWriter
    {
        #region Class Variables
        private static RuleEngineWriter _instance = null;
        private readonly string _filePath;
        #endregion

        private RuleEngineWriter(string filePath)
        {
            _filePath = filePath;
        }

        public static RuleEngineWriter GetInstance(string filePath)
        {
            if (_instance == null)
                _instance = new RuleEngineWriter(filePath);
            return _instance;
        }

        #region Save Voilated Data to Output file
        /// <summary>
        /// Method to save voilated data list
        /// </summary>
        /// <param name="voilatedRuleDataList">voilated signal names list</param>
        public void SaveVoilatedRules(List<string> voilatedRuleDataList)
        {
            Task.Factory.StartNew(() => SaveVoilatedOutPutToFile(voilatedRuleDataList)).Wait();
        }

        /// <summary>
        /// Method to save extracted voilated signal names list in text file using the rules defined by user
        /// </summary>
        /// <param name="voilatedRuleDataList">voilated signal names list</param>
        private void SaveVoilatedOutPutToFile(List<string> voilatedRuleDataList)
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
            using (StreamWriter sw = new StreamWriter(_filePath))
            {
                foreach (string voilatedRule in voilatedRuleDataList)
                {
                    sw.WriteLine(voilatedRule);
                }
            }
        }
        #endregion
    }
}