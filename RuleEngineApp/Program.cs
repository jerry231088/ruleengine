using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RuleEngineApp.Abstraction;
using RuleEngineApp.Classes;
using System.Diagnostics;

namespace RuleEngineApp
{
    class Program
    {
        private static Mutex mutex = null;
        static void Main(string[] args)
        {
            const string appName = "Rule Engine";
            mutex = new Mutex(true, appName, out bool createdNew);
            //Allow to open Single Instance Only
            if (createdNew)
                RunEngine();
        }

        /// <summary>
        /// Method to run Rule Engine
        /// </summary>
        private static void RunEngine()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Console.WriteLine("Rule Engine Starts!");
            Console.WriteLine("Do you want to run the rule engine on raw data? Please type Y or N?");
            char userInput = Convert.ToChar(Console.ReadLine());
            string GetDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            IRuleEngine ruleEngRdr = RuleEngineReader.GetInstance(GetDirectory);
            List<string> ruleEngineProcsdData = new List<string>();
            if (Equals(char.ToUpper(userInput), 'Y'))
            {
                Console.WriteLine("Please keep your rule engine data stream file at " + GetDirectory + " and press any key to continue.");
                Console.ReadLine();
                string[] filePath = Directory.GetFiles(GetDirectory, "*.json");
                string extFilePath = Utility.CheckForExtFile(GetDirectory, filePath);
                Console.WriteLine("Getting rules from file " + extFilePath + ".");
                ruleEngineProcsdData = ruleEngRdr.LoadDataStreamInRuleEngine(extFilePath);
            }
            else
            {
                Console.WriteLine("Getting rules from file raw_data.json.");
                ruleEngineProcsdData = ruleEngRdr.LoadDataStreamInRuleEngine(@"\raw_data.json");
            }

            Console.WriteLine("Preparing the output.");
            IRuleEngineWriter ruleEngRes = RuleEngineWriter.GetInstance(GetDirectory + @"\VoilatedRules.txt");
            ruleEngRes.SaveVoilatedRules(ruleEngineProcsdData);
            Console.WriteLine("Rule engine voilated result saved to " + GetDirectory + @"\VoilatedRules.txt");
            sw.Stop();
            Console.WriteLine("Time taken by app to complete the process: " + sw.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}