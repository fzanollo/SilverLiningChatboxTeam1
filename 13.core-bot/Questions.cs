using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace QuestionsOverview
{
    class Questions
    {

      private List<Tuple<string,int>> qst = new List<Tuple<string,int>>()
      {
        Tuple.Create("whats5x5",25),
        Tuple.Create("what 6x6",36),
        Tuple.Create("what 7x7",49),
      };
        private int index;

        private int listSize = 3;

        private static Random rng;

        private static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public Questions()
        {
            rng = new Random();
            Shuffle(qst);
            index = 0;
        }

        public Tuple<string,int> getQuestion()
        {
            
            return (index<listSize)?(Tuple.Create(qst[index++].Item1, qst[index++].Item2)):(Tuple.Create("error",-1));
        }

        public void addQuestion(Tuple<string,int> tp)
        {
            qst.Add(tp);
            listSize++;
        }
        
    }
}