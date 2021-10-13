using Core.Utilites.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Utilites.Business
{
    public class BusinessRules
    {
        public static IResult Run(params IResult[] logics)
        {
            // List<IResult> errorResults = new List<IResult>();
            foreach (var logic in logics)
            {
                if (!logic.Success)
                {
                   //  errorResults.Add(logic);
                   
                    return logic;
                }
            }
            return null;
        }
    }
}
