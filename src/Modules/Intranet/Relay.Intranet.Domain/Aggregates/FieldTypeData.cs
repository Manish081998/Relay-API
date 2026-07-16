using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relay.Intranet.Domain.Aggregates
{
    public class FieldTypeData
    {
        // (modelname.Upper, macpacfield.Upper) pairs from xref WHERE typenumeric=1 OR typefractional=1
        public HashSet<(string Model, string Field)> XrefEntries { get; init; } = [];

        // fieldname.Upper values from MacPacFieldTypes WHERE TypeFractional=1
        public HashSet<string> FractionalFields { get; init; } = [];

        public static readonly FieldTypeData Empty = new();

        /// <summary>
        /// Returns "Fraction", "NUM", or "Text" for a given field + model combination,
        /// matching the precedence in GetFieldType(): Fraction checked before NUM.
        /// </summary>
        public string GetFieldType(string fieldName, string modelName)
        {
            var model = modelName.Trim().ToUpperInvariant();
            var field = fieldName.Trim().ToUpperInvariant();

            if (!XrefEntries.Contains((model, field)))
                return "Text";

            return FractionalFields.Contains(field) ? "Fraction" : "NUM";
        }
    }
}
