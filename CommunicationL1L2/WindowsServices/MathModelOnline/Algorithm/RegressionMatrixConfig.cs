using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathModelOnline.Algorithm
{
    public enum TermType
    {
        X,
        U,
        Zero
    }
    public class RegressionMatrixConfig
    {
        public List<List<(TermType Term, int Index)>> RegressionMatrix { get; private set; }

        public RegressionMatrixConfig()
        {
            RegressionMatrix = new List<List<(TermType Term, int Index)>>();
        }

        public RegressionMatrixConfig AddRegression(params string[] terms)
        {
            var termsList = new List<(TermType Term, int Index)>();
            foreach (var term in terms)
            {
                termsList.Add(ConvertStringToTerm(term, out var index));
            }
            RegressionMatrix.Add(termsList);

            return this;
        }

        public RegressionMatrixConfig AddRegression(params (TermType Term, int Index)[] terms)
        {
            RegressionMatrix.Add(new List<(TermType Term, int Index)>(terms));
            return this;
        }

        public List<List<(TermType Term, int Index)>> Build()
        {
            return RegressionMatrix;
        }

        private (TermType Term, int Index) ConvertStringToTerm(string term, out int index)
        {
            index = -1;
            if (term.StartsWith("X"))
            {
                int.TryParse(term.Substring(1), out index);
                return (TermType.X, index);
            }
            else if (term.StartsWith("U"))
            {
                int.TryParse(term.Substring(1), out index);
                return (TermType.U, index);
            }
            else if (term == "0")
            {
                return (TermType.Zero, 0);
            }
            throw new ArgumentException($"Invalid term: {term}");
        }

        private string ConvertTermToString((TermType Term, int Index) term)
        {
            return term.Term switch
            {
                TermType.X => $"X{term.Index}",
                TermType.U => $"U{term.Index}",
                TermType.Zero => "0",
                _ => throw new ArgumentException($"Invalid term: {term.Term}")
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var row in RegressionMatrix)
            {
                var rowStrings = row.Select(ConvertTermToString).ToList();
                sb.AppendLine(string.Join(", ", rowStrings));
            }
            return sb.ToString();
        }
    }
}
