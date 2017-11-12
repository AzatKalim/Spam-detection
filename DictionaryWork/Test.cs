using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace DictionaryWork
{
    class BaseDictionry
    {
        const int MESSAGES_COUNT = 50;
        static Dictionary<string, int> categories;
        static Dictionary<string, Dictionary<string, int>> features;
        public BaseDictionry()
        {
            categories = new Dictionary<string, int>();
            features = new Dictionary<string, Dictionary<string, int>>();
            categories.Add("good", MESSAGES_COUNT);
            categories.Add("bad", MESSAGES_COUNT);        
        }

        static string[] GetWords(string text)
        {
            text = text.ToLowerInvariant();
            string pattern = @"\b([a-z]+)\b";
            Regex regex = new Regex(pattern);
            List<string> words = new List<string>();
            foreach (Match match in regex.Matches(text))
            {
                if (!words.Contains(match.Groups[0].Value))
                {
                    words.Add(match.Groups[0].Value);
                }
            }
            return words.ToArray();
        }

        public  void AddFeatures(string word, string category)
        {
            if (!features.ContainsKey(word))
            {
                Dictionary<string, int> categories = new Dictionary<string, int>();
                categories.Add(category, 1);
                features.Add(word, categories);
            }
            else if (!features[word].ContainsKey(category))
            {
                features[word].Add(category, 1);
            }
            else
            {
                features[word][category]++;
            }
        }

        int GetCountOfOccurrencesInCategory(string word, string category)
        {
            if (!features.ContainsKey(word))
            {
                return 0;
            }
            else if (!features[word].ContainsKey(category))
            {
                return 0;
            }
            return features[word][category];
        }

        int GetCountOfExamplesInCategory(string category)
        {
            if (categories.ContainsKey(category))
            {
                return categories[category];
            }
            return 0;
        }

        List<string> GetAllCategories()
        {
            return new List<string>(categories.Keys);
        }

        public void CreateDictionary(string hamFile="good.txt",string spamFile="bad.txt")
        {
            string dictionaryFile = @"dictionary.txt";
            string message;
            StreamReader sr = new StreamReader(hamFile);
            for (int i = 0; i < categories["good"]; i++)
            {
                message = sr.ReadLine();
                string[] words = GetWords(message);
                for (int j = 0; j < words.Length; j++)
                {
                    AddFeatures(words[j], "good");
                }
            }
            sr = new StreamReader(spamFile);
            for (int i = 0; i < categories["bad"]; i++)
            {
                message = sr.ReadLine();
                string[] words = GetWords(message);
                for (int j = 0; j < words.Length; j++)
                {
                    AddFeatures(words[j], "bad");
                }
            }
            sr.Close();
            StringBuilder output = new StringBuilder();
            StreamWriter sw = new StreamWriter(dictionaryFile);
            foreach (var feature in features)
            {
                output.Append("<");
                output.Append(feature.Key.ToString());
                output.Append(", <");
                foreach (var category in feature.Value)
                {
                    output.Append(category.Key.ToString());
                    output.Append(": ");
                    output.Append(category.Value.ToString());
                    output.Append(", ");
                }
                output.Remove(output.Length - 2, 2);
                output.Append(">>");
                sw.WriteLine(output);
                output.Clear();
            }
            sw.Close();
        }

        double ConditionalProbability(string word, string category)   //P(word|category)
        {
            return (double)GetCountOfOccurrencesInCategory(word, category) / GetCountOfExamplesInCategory(category);
        }

        int GetCountOfOccurrencesInAllCategories(string word)      //N(word)
        {
            int N = 0;
            foreach (var category in categories)
            {
                N += GetCountOfOccurrencesInCategory(word, category.Key);
            }
            return N;
        }

        double WeightedProbability(string word, string category)     //Pw
        {
            double Ap = 0.5;
            int w = 1;
            int N = GetCountOfOccurrencesInAllCategories(word);
            double P = ConditionalProbability(word, category);
            return (w * Ap + N * P) / (N + w);
        }

        public void DrawTable(string category,string fileName)
        {
            StreamWriter sw = new StreamWriter(fileName);
            string line = "|-----------------------------------------------------------------------------------------------------------|";
            sw.WriteLine(line);
            string str = string.Format("| {0, -15} | {1, -15} | {2, -15} | {3, -15} | {4, -15} | {5, -15} |",
                    "Слово",
                    "P(word|" + category + ")",
                    "Pw(word|" + category + ")",
                    "N(word&" + category + ")",
                    "N(word)",
                    "N(" + category + ")");
            sw.WriteLine(str);
            sw.WriteLine(line);
            foreach (var feature in features)
            {
                str = string.Format("| {0, -15} | {1,15} | {2,15} | {3,15} | {4,15} | {5,15} |",
                    feature.Key,
                    Math.Round(ConditionalProbability(feature.Key, category), 5),
                    Math.Round(WeightedProbability(feature.Key, category), 5),
                    GetCountOfOccurrencesInCategory(feature.Key, category),
                    GetCountOfOccurrencesInAllCategories(feature.Key),
                    categories[category]);
                sw.WriteLine(str);
            }
            sw.WriteLine(line);
            sw.Close();
        }

        public  double ProbabilityDocInCategory(string[] words, string category)
        {
            double result = 1;
            for (int i = 0; i < words.Length; i++)
            {
                if (features.ContainsKey(words[i]))
                {
                    result *= WeightedProbability(words[i], category);
                    //result *= ConditionalProbability(words[i], category);
                }
            }
            return result;
        }
        public double ProbabilityCategory(string category)
        {
            double temp = (double)GetCountOfExamplesInCategory(category) / GetCountOfAllExamples();
            return temp;
        }
        public int GetCountOfAllExamples()
        {
            int result = 0;
            foreach (var category in categories)
            {
                result += GetCountOfExamplesInCategory(category.Key);
            }
            return result;
        }
        public  double ProbabilityCategoryInDoc(string category, string[] words)
        {
            return ProbabilityDocInCategory(words, category) * ProbabilityCategory(category);
        }

        public  string Classifier(string message)
        {
            string[] words = GetWords(message);
            double[] probabilities = new double[categories.Count];
            int i = 0;
            foreach (var category in categories)
            {
                probabilities[i] = ProbabilityCategoryInDoc(category.Key, words);
                i++;
            }

            int k = 3;
            if (probabilities[1] > k * probabilities[0])
            {
                return "bad";
            }
            if (probabilities[0] > probabilities[1])
            {
                return "good";
            }
            else
            {
                return "unknown";
            }
        }
    }
}
