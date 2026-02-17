using System;
using System.Linq;
using System.Reflection;
using System.Text;
using StardewModdingAPI;

internal static class ReflectionDumper
{
    public static void DumpPublicApi(
        IMonitor monitor,
        object obj,
        string title,
        LogLevel level = LogLevel.Info,
        bool declaredOnly = false,
        bool includeInheritedObjectMembers = false,
        int maxPropertyValueLength = 300,
        int maxLines = 500
    )
    {
        if (obj is null)
        {
            monitor.Log($"== {title}: <null> ==", level);
            return;
        }

        var type = obj.GetType();
        var flags = BindingFlags.Public | BindingFlags.Instance;
        if (declaredOnly) flags |= BindingFlags.DeclaredOnly;

        var sb = new StringBuilder();
        sb.AppendLine($"== {title} ==");
        sb.AppendLine($"Type: {type.FullName}");
        sb.AppendLine();

        // ---------- PROPERTIES ----------
        sb.AppendLine("-- Public properties --");

        var props = type.GetProperties(flags)
            .OrderBy(p => p.Name)
            .ToArray();

        foreach (var p in props)
        {
            // indexers poluem e podem estourar exceção
            if (p.GetIndexParameters().Length > 0)
            {
                sb.AppendLine($"{p.Name} : {p.PropertyType.Name} = <indexer>");
                continue;
            }

            if (!p.CanRead)
            {
                sb.AppendLine($"{p.Name} : {p.PropertyType.Name} = <no getter>");
                continue;
            }

            object? value = null;
            string valueText;

            try
            {
                value = p.GetValue(obj);
                valueText = value is null ? "null" : SafeToString(value, maxPropertyValueLength);
            }
            catch (Exception ex)
            {
                valueText = $"<throws {ex.GetType().Name}: {ex.Message}>";
            }

            sb.AppendLine($"{p.Name} : {PrettyType(p.PropertyType)} = {valueText}");
        }

        sb.AppendLine();

        // ---------- METHODS ----------
        sb.AppendLine("-- Public methods --");

        bool IsNoiseName(string name) =>
            name.StartsWith("get_", StringComparison.Ordinal) ||
            name.StartsWith("set_", StringComparison.Ordinal) ||
            name.StartsWith("add_", StringComparison.Ordinal) ||
            name.StartsWith("remove_", StringComparison.Ordinal) ||
            name.StartsWith("op_", StringComparison.Ordinal); // operadores

        bool IsObjectMethod(MethodInfo m) =>
            m.DeclaringType == typeof(object);

        var methods = type.GetMethods(flags)
            .Where(m => !m.IsSpecialName) // remove get_/set_ etc em muitos casos
            .Where(m => !IsNoiseName(m.Name))
            .Where(m => includeInheritedObjectMembers || !IsObjectMethod(m))
            .OrderBy(m => m.Name)
            .ThenBy(m => m.GetParameters().Length)
            .ToArray();

        foreach (var m in methods)
        {
            sb.AppendLine(FormatMethodSignature(m));
        }

        // ---------- OUTPUT (with line cap) ----------
        var text = sb.ToString();
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        if (lines.Length <= maxLines)
        {
            monitor.Log(text, level);
            return;
        }

        // se passar do limite, corta e avisa
        var trimmed = string.Join(Environment.NewLine, lines.Take(maxLines))
            + Environment.NewLine
            + $"... <trimmed: {lines.Length - maxLines} more lines>";
        monitor.Log(trimmed, level);
    }

    private static string PrettyType(Type t)
    {
        if (!t.IsGenericType) return t.Name;

        var defName = t.Name.Split('`')[0];
        var args = string.Join(", ", t.GetGenericArguments().Select(PrettyType));
        return $"{defName}<{args}>";
    }

    private static string SafeToString(object value, int maxLen)
    {
        string s;
        try
        {
            s = value.ToString() ?? "<null ToString()>";
        }
        catch (Exception ex)
        {
            return $"<ToString() throws {ex.GetType().Name}>";
        }

        if (s.Length <= maxLen) return s;
        return s.Substring(0, maxLen) + "...";
    }

    private static string FormatMethodSignature(MethodInfo m)
    {
        var ret = PrettyType(m.ReturnType);
        var name = m.Name;

        var generic = "";
        if (m.IsGenericMethodDefinition)
        {
            var genArgs = string.Join(", ", m.GetGenericArguments().Select(a => a.Name));
            generic = $"<{genArgs}>";
        }

        var pars = m.GetParameters()
            .Select(p =>
            {
                var modifier =
                    p.IsOut ? "out " :
                    p.ParameterType.IsByRef ? "ref " :
                    p.IsIn ? "in " : "";

                var pt = p.ParameterType.IsByRef ? p.ParameterType.GetElementType()! : p.ParameterType;
                return $"{modifier}{PrettyType(pt)} {p.Name}";
            });

        return $"{ret} {name}{generic}({string.Join(", ", pars)})";
    }
}
