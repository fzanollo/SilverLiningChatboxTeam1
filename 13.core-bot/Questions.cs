using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace QuestionsOverview
{
    class Questions
    {
      private HashSet<Tuple<string,int>> qst = new HashSet<Tuple<string,int>>()
      {
        Tuple.Create("what's 5x5",25),
        Tuple.Create("what's 6x6",36),
        Tuple.Create("what's 7x7",49),
        Tuple.Create("what 1x1",1),
        Tuple.Create("what 2x2",4),
        Tuple.Create("what 3x3",9),
        Tuple.Create("what 4x4",16),
        Tuple.Create("what 1x2",2),
        Tuple.Create("what 1x3",3),
        Tuple.Create("what 4x3",12),
        Tuple.Create("what 2x3",6),
        Tuple.Create("what 8x7",56),
        Tuple.Create("what 1+1",2),
        Tuple.Create("what 1-1",0),
        Tuple.Create("what 7+7",14),
        Tuple.Create("what 2x7",14),
        Tuple.Create("what 4x5",20),
        Tuple.Create("what 12x2",24),
      };

        private static Random rng;

        public Questions()
        {
            rng = new Random();
        }

        public Tuple<string,int> getQuestion()
        {
            Tuple<string, int> element = qst.ElementAt(rng.Next(qst.Count));
            return element;
        }

        public void addQuestion(Tuple<string,int> tp)
        {
            qst.Add(tp);
        }
        
    }
}