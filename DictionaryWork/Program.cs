using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace DictionaryWork
{
    class Program
    {
        static void Main(string[] args)
        {
            BaseDictionry te = new BaseDictionry();

            te.CreateDictionary();
            te.DrawTable("good","goodTable.txt");
            te.DrawTable("bad", "badTable.txt");
            Console.WriteLine(te.Classifier("you gonna talk to me"));
            Console.WriteLine(te.Classifier("the wonderful ham for you"));
            Console.WriteLine(te.Classifier("i have no pizza"));
            Console.WriteLine(te.Classifier("as stop you pizza for me"));
            Console.WriteLine(te.Classifier("as stop you for me"));
            Console.WriteLine(te.Classifier("You sure your neighbors didnt pick it up"));
            Console.WriteLine(te.Classifier("Lol now I'm after that hot air balloon"));
            Console.WriteLine(te.Classifier("As a SIM subscriber, you are selected to receive a Bonus! Get it delivered to your door, Txt the word OK to No"));
            Console.WriteLine(te.Classifier("Please CALL 08712402779 immediately as there is an urgent message waiting for you"));
            Console.WriteLine(te.Classifier("INTERFLORA - еТIt's not too late to order Interflora flowers for christmas call 0800 505060 to place your order before Midnight tomorrow"));

        }     
    }
}
