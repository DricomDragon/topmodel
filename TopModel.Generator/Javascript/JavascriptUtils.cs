﻿using TopModel.Core;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

public static class JavascriptUtils
{
    public static string GetPropertyTypeName(this IProperty property, IEnumerable<Class>? availableClasses = null)
    {
        return property switch
        {
            CompositionProperty cp => cp.Kind switch
            {
                "object" => cp.Composition.NamePascal,
                "list" or "async-list" => $"{cp.Composition.NamePascal}[]",
                string _ when cp.DomainKind!.TS!.Type.Contains("{composition.name}") => cp.DomainKind.TS.Type.ParseTemplate(cp),
                string _ => $"{cp.DomainKind.TS.Type}<{{composition.name}}>".ParseTemplate(cp)
            },
            AssociationProperty { Association: Class assoc } ap when assoc.IsEnum(availableClasses, ap.Property) => $"{assoc}{ap.Property}{(ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.ManyToMany ? "[]" : string.Empty)}",
            AliasProperty { Property: AssociationProperty { Association: Class assoc } ap, AsList: var asList } when assoc.IsEnum(availableClasses, ap.Property) => $"{assoc}{ap.Property}{(asList || ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.ManyToMany ? "[]" : string.Empty)}",
            RegularProperty { Class: Class classe } rp when classe.IsEnum(availableClasses, rp) => $"{classe}{rp}",
            AliasProperty { Property: RegularProperty { Class: Class alClass } rp, AsList: var asList } when alClass.IsEnum(availableClasses, rp) => $"{alClass}{rp}{(asList ? "[]" : string.Empty)}",
            IFieldProperty fp => fp.Domain.TS?.Type.ParseTemplate(fp) ?? string.Empty,
            _ => string.Empty
        };
    }

    public static bool IsEnum(this Class classe, IEnumerable<Class>? availableClasses, IFieldProperty? prop = null)
    {
        if (availableClasses != null && !availableClasses.Contains(classe))
        {
            return false;
        }

        prop ??= classe.EnumKey;

        bool CheckProperty(IFieldProperty fp)
        {
            return (fp == classe.EnumKey || classe.UniqueKeys.Where(uk => uk.Count == 1).Select(uk => uk.Single()).Contains(prop))
                && classe.Values.All(r => r.Value.ContainsKey(fp));
        }

        return classe.Enum && CheckProperty(prop!);
    }

    public static bool IsJSReference(this Class classe)
    {
        return classe.EnumKey != null || classe.Reference && !classe.ReferenceKey!.Domain.AutoGeneratedValue;
    }

    public static List<(string Import, string Path)> GroupAndSort(this IEnumerable<(string Import, string Path)> imports)
    {
        return imports
             .GroupBy(i => i.Path)
             .Select(i => (Import: string.Join(", ", i.Select(l => l.Import).Distinct().OrderBy(x => x)), Path: i.Key))
             .OrderBy(i => i.Path.StartsWith(".") ? i.Path : $"...{i.Path}")
             .ToList();
    }

    public static void WriteReferenceDefinition(FileWriter fw, Class classe)
    {
        fw.Write("export const ");
        fw.Write(classe.NameCamel);
        fw.Write(" = {type: {} as ");
        fw.Write(classe.NamePascal);
        fw.Write(", valueKey: \"");
        fw.Write(classe.ReferenceKey!.NameCamel);
        fw.Write("\", labelKey: \"");
        fw.Write(classe.DefaultProperty?.NameCamel);
        fw.Write("\"} as const;\r\n");
    }
}
