// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace Luis
{
    // Extends the partial MathBotclass with methods and properties that simplify accessing entities in the luis results
    public partial class MathBot
    {
        public string NameEntity 
        {
            get
            {
                var nameValue = Entities?._instance?.personName?.FirstOrDefault()?.Text;
                return nameValue;
            }
        }

        public string ChallengeType => Entities?._instance?.Type_of_Game?.FirstOrDefault()?.Text;

        public string NumberEntity
        {
            get
            {
                var numValue = Entities?._instance?.number?.FirstOrDefault()?.Text;
                return numValue;
            }
        }
        //add for answer/number entity
    }
}