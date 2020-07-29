// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace Luis
{
    // Extends the partial MathBotclass with methods and properties that simplify accessing entities in the luis results
    public partial class MathBot
    {
        public string nameEntity 
        {
            get
            {
                var nameValue = Entities?._instance?.personName?.FirstOrDefault()?.Text;
                return nameValue;
            }
        }
    }
}