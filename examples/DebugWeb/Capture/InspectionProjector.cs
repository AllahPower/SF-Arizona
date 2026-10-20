using System.Collections;
using System.Globalization;
using System.Reflection;
using SFSharp.Abstractions.Parsing;

namespace SFSharp.Examples.DebugWeb.Capture;

internal static class InspectionProjector
{
    public static InspectionNode Project(object? value) => Visit("Parsed", value, new HashSet<object>(ReferenceEqualityComparer.Instance), 0);

    private static InspectionNode Visit(string name, object? value, HashSet<object> ancestors, int depth)
    {
        if (value is null) return new(name, "null", "null", null, []);
        Type type = value.GetType();
        string typeName = type.Name;
        if (value is string or char) return new(name, typeName, "string", value.ToString(), []);
        if (value is bool boolean) return new(name, typeName, "boolean", boolean ? "true" : "false", []);
        if (type.IsEnum) return new(name, typeName, "enum", $"{value} ({((Enum)value).ToString("D")})", []);
        if (type.IsPrimitive || value is decimal) return new(name, typeName, "number", Convert.ToString(value, CultureInfo.InvariantCulture), []);
        if (value is DateTime or DateTimeOffset or Guid or TimeSpan) return new(name, typeName, "string", Convert.ToString(value, CultureInfo.InvariantCulture), []);
        if (depth >= 64) return new(name, typeName, "limit", "Maximum inspection depth (64) reached; raw bytes remain available.", []);
        if (!type.IsValueType && !ancestors.Add(value)) return new(name, typeName, "reference", "Circular reference", []);
        try
        {
            List<InspectionNode> children = [];
            if (value is ReadOnlyMemory<byte> memory) value = memory.ToArray();
            else if (value is Memory<byte> writableMemory) value = writableMemory.ToArray();
            if (value is IEnumerable sequence)
            {
                int index = 0;
                foreach (object? item in sequence) children.Add(Visit($"[{index++}]", item, ancestors, depth + 1));
                return new(name, typeName, "array", index.ToString(CultureInfo.InvariantCulture), children.ToArray());
            }
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0
                    || (property.Name == "Detail" && value is IParsedPacket or IParsedRpc)) continue;
                try { children.Add(Visit(property.Name, property.GetValue(value), ancestors, depth + 1)); }
                catch (Exception ex) { children.Add(new(property.Name, property.PropertyType.Name, "error", ex.GetBaseException().Message, [])); }
            }
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                children.Add(Visit(field.Name, field.GetValue(value), ancestors, depth + 1));
            return new(name, typeName, "object", null, children.ToArray());
        }
        finally { if (!type.IsValueType) ancestors.Remove(value); }
    }
}
