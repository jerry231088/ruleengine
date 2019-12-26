using System;
using System.Collections.Generic;

namespace RuleEngineApp.Classes
{
    public static class Utility
    {
        /// <summary>
        /// Method to convert object data type to decimal
        /// </summary>
        /// <param name="value">object data type</param>
        /// <returns>decimal value</returns>
        public static decimal? ConvertToDecimal(object value)
        {
            bool parseToDec = false;
            decimal val;
            parseToDec = decimal.TryParse(value.ToString(), out val);
            if (parseToDec) return val;
            else return null;
        }

        /// <summary>
        /// Method to convert object data type to datetime
        /// </summary>
        /// <param name="value">object data type</param>
        /// <returns>datetime value</returns>
        public static DateTime? ConvertToDateTime(object value)
        {
            bool parseToDt = false;
            DateTime val;
            parseToDt = DateTime.TryParse(value.ToString(), out val);
            if (parseToDt) return val;
            else return null;
        }

        /// <summary>
        /// Method to convert object data type to string
        /// </summary>
        /// <param name="value">object data type</param>
        /// <returns>string value</returns>
        public static string ConvertToString(object value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Method to create list of decimal value
        /// </summary>
        /// <param name="valtypeValue">List of string</param>
        /// <returns>List of decimal values</returns>
        public static List<decimal> CreateDecList(List<string> valtypeValue)
        {
            var intList = new List<decimal>();
            foreach (var val in valtypeValue)
            {
                bool valParsed = false;
                valParsed = decimal.TryParse(val, out decimal conVal);
                if (valParsed)
                {
                    if (!intList.Contains(conVal))
                        intList.Add(conVal);
                }
            }
            return intList;
        }

        /// <summary>
        /// Method to check user provided external json file or not
        /// </summary>
        /// <param name="dirPath">Application directory path</param>
        /// <param name="filePath">string array</param>
        /// <returns>string i.e. external file path</returns>
        public static string CheckForExtFile(string dirPath, string[] filePath)
        {
            string extFilepath = string.Empty;
            foreach (string file in filePath)
            {
                var pathSplit = file.Split('\\');
                if (pathSplit[pathSplit.Length - 1] == "raw_data.json") continue;
                extFilepath = pathSplit[pathSplit.Length - 1];
                break;
            }
            if (string.IsNullOrEmpty(extFilepath))
            {
                Console.WriteLine("No files found at " + dirPath + @" .Please keep a .json file and try again");
                Console.ReadLine();
                CheckForExtFile(dirPath, filePath);
            }
            return extFilepath;
        }

        public enum StrRuleType
        {
            HIGH = 0,
            LOW = 1
        }
    }
}