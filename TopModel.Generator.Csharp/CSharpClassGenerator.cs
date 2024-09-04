﻿using System.Data;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.Model.Implementation;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Csharp;

public class CSharpClassGenerator : ClassGeneratorBase<CsharpConfig>
{
    private readonly ILogger<CSharpClassGenerator> _logger;

    public CSharpClassGenerator(ILogger<CSharpClassGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "CSharpClassGen";

    protected virtual Dictionary<string, string> NewableTypes { get; } = new()
    {
        ["IEnumerable"] = "List",
        ["ICollection"] = "List",
        ["IList"] = "List",
        ["List"] = "List",
        ["HashSet"] = "HashSet"
    };

    /// <summary>
    /// Génération de la déclaration de la classe.
    /// </summary>
    /// <param name="w">Writer</param>
    /// <param name="item">Classe à générer.</param>
    /// <param name="tag">Tag.</param>
    protected virtual void GenerateClassDeclaration(CSharpWriter w, Class item, string tag)
    {
        if (!item.Abstract)
        {
            if (item.Reference && Config.Kinetix)
            {
                if (!item.ReferenceKey!.Domain.AutoGeneratedValue)
                {
                    w.WriteAttribute("Reference", "true");
                }
                else
                {
                    w.WriteAttribute("Reference");
                }
            }

            if (item.Reference && item.DefaultProperty != null)
            {
                w.WriteAttribute("DefaultProperty", $@"nameof({item.DefaultProperty.NamePascal})");
            }

            if (Config.IsPersistent(item, tag))
            {
                var sqlName = Config.UseLowerCaseSqlNames ? item.SqlName.ToLower() : item.SqlName;
                if (Config.DbSchema != null)
                {
                    w.WriteAttribute("Table", $@"""{sqlName}""", $@"Schema = ""{Config.ResolveVariables(Config.DbSchema, tag, module: item.Namespace.Module.ToSnakeCase())}""");
                }
                else
                {
                    w.WriteAttribute("Table", $@"""{sqlName}""");
                }
            }
        }

        foreach (var annotation in Config.GetDecoratorAnnotations(item, tag))
        {
            w.WriteAttribute(annotation);
        }

        var extends = Config.GetClassExtends(item);
        var implements = Config.GetClassImplements(item);

        if (item.Abstract)
        {
            w.Write($"public interface I{item.NamePascal}");

            if (implements.Any())
            {
                w.Write($" : {string.Join(", ", implements)}");
            }

            w.WriteLine();
            w.WriteLine("{");
        }
        else
        {
            var isRecord = (Config.UseRecords & Target.Dto) > 0 && !Config.IsPersistent(item, tag) || (Config.UseRecords & Target.Persisted) > 0 && Config.IsPersistent(item, tag);
            w.WriteClassDeclaration(item.NamePascal, extends, isRecord, implements.ToArray());

            GenerateConstProperties(w, item);

            if (Config.DbContextPath == null && Config.Kinetix && Config.IsPersistent(item, tag))
            {
                GenerateEnumCols(w, item);
            }

            GenerateEnumValues(w, item);

            GenerateFlags(w, item);
        }

        GenerateProperties(w, item, tag);

        if (item.Abstract)
        {
            GenerateCreateMethod(w, item);
        }

        w.WriteLine("}");
    }

    /// <summary>
    /// Génération des constantes statiques.
    /// </summary>
    /// <param name="w">Writer.</param>
    /// <param name="item">La classe générée.</param>
    protected virtual void GenerateConstProperties(CSharpWriter w, Class item)
    {
        var consts = new List<(IProperty Prop, string Name, string Code, string Label)>();

        foreach (var refValue in item.Values)
        {
            var label = refValue.GetLabel(item);

            if (!Config.CanClassUseEnums(item, Classes) && item.EnumKey != null)
            {
                var code = refValue.Value[item.EnumKey];
                consts.Add((item.EnumKey, refValue.Name, code, label));
            }

            foreach (var uk in item.UniqueKeys.Where(uk =>
                uk.Count == 1
                && Config.GetType(uk.Single())?.TrimEnd('?') == "string"
                && refValue.Value.ContainsKey(uk.Single())))
            {
                var prop = uk.Single();

                if (!Config.CanClassUseEnums(item, Classes, prop))
                {
                    var code = refValue.Value[prop];
                    consts.Add((prop, $"{refValue.Name}{prop}", code, label));
                }
            }
        }

        var orderedConsts = consts.OrderBy(x => x.Name.ToPascalCase(strictIfUppercase: true), StringComparer.Ordinal).ToList();

        foreach (var @const in orderedConsts)
        {
            if (orderedConsts.IndexOf(@const) > 0)
            {
                w.WriteLine();
            }

            w.WriteSummary(1, @const.Label);
            w.WriteLine(1, $"public const {Config.GetType(@const.Prop).TrimEnd('?')} {@const.Name.ToPascalCase(strictIfUppercase: true)} = {(Config.ShouldQuoteValue(@const.Prop) ? $@"""{@const.Code}""" : @const.Code)};");
        }

        if (consts.Any())
        {
            w.WriteLine();
        }
    }

    protected virtual void GenerateCreateMethod(CSharpWriter w, Class item)
    {
        var writeProperties = item.Properties.Where(p => !p.Readonly);

        if (writeProperties.Any())
        {
            w.WriteLine();
            w.WriteSummary(1, "Factory pour instancier la classe.");
            foreach (var prop in writeProperties)
            {
                w.WriteParam(prop.NameCamel, prop.Comment);
            }

            w.WriteReturns(1, "Instance de la classe.");
            w.WriteLine(1, $"static abstract I{item.NamePascal} Create({string.Join(", ", writeProperties.Select(p => $"{Config.GetType(p)} {p.NameCamel} = null"))});");
        }
    }

    /// <summary>
    /// Génère le type énuméré présentant les colonnes persistentes.
    /// </summary>
    /// <param name="w">Writer.</param>
    /// <param name="item">La classe générée.</param>
    protected virtual void GenerateEnumCols(CSharpWriter w, Class item)
    {
        w.WriteLine(1, "#region Meta données");
        w.WriteLine();
        w.WriteSummary(1, "Type énuméré présentant les noms des colonnes en base.");

        if (item.Extends == null)
        {
            w.WriteLine(1, "public enum Cols");
        }
        else
        {
            w.WriteLine(1, "public new enum Cols");
        }

        w.WriteLine(1, "{");

        foreach (var property in item.Properties)
        {
            w.WriteSummary(2, "Nom de la colonne en base associée à la propriété " + property.NamePascal + ".");
            w.WriteLine(2, $"{property.SqlName},");
            if (item.Properties.IndexOf(property) != item.Properties.Count - 1)
            {
                w.WriteLine();
            }
        }

        w.WriteLine(1, "}");
        w.WriteLine();
        w.WriteLine(1, "#endregion");
        w.WriteLine();
    }

    /// <summary>
    /// Génère l'enum pour les valeurs statiques de références.
    /// </summary>
    /// <param name="w">Writer.</param>
    /// <param name="item">La classe générée.</param>
    protected virtual void GenerateEnumValues(CSharpWriter w, Class item)
    {
        bool WriteEnum(IProperty prop)
        {
            if (item.Extends != null && Config.CanClassUseEnums(item.Extends, Classes, prop))
            {
                return false;
            }

            var refs = GetAllValues(item)
                .OrderBy(x => x.Name, StringComparer.Ordinal)
                .ToList();

            w.WriteSummary(1, $"Valeurs possibles de la liste de référence {item}.");
            w.WriteLine(1, $"public enum {Config.GetEnumType(prop, true)}");
            w.WriteLine(1, "{");

            foreach (var refValue in refs)
            {
                w.WriteSummary(2, refValue.GetLabel(item));
                w.Write(2, refValue.Value[prop]);

                if (refs.IndexOf(refValue) != refs.Count - 1)
                {
                    w.WriteLine(",");
                }

                w.WriteLine();
            }

            w.WriteLine(1, "}");
            return true;
        }

        var hasLine = Config.CanClassUseEnums(item, Classes) && WriteEnum(item.EnumKey!);

        foreach (var uk in item.UniqueKeys.Where(uk => uk.Count == 1 && Config.CanClassUseEnums(item, Classes, uk.Single())))
        {
            if (hasLine)
            {
                w.WriteLine();
            }

            hasLine |= WriteEnum(uk.Single());
        }

        if (hasLine)
        {
            w.WriteLine();
        }
    }

    /// <summary>
    /// Génère les flags d'une liste de référence statique.
    /// </summary>
    /// <param name="w">Writer.</param>
    /// <param name="item">La classe générée.</param>
    protected virtual void GenerateFlags(CSharpWriter w, Class item)
    {
        if (item.FlagProperty != null && item.Values.Any())
        {
            w.WriteLine(1, "#region Flags");
            w.WriteLine();
            w.WriteSummary(1, "Flags");
            w.WriteLine(1, "public enum Flags");
            w.WriteLine(1, "{");

            var flagValues = item.Values.Where(refValue => refValue.Value.ContainsKey(item.FlagProperty) && int.TryParse(refValue.Value[item.FlagProperty], out var _)).ToList();
            foreach (var refValue in flagValues)
            {
                var flag = int.Parse(refValue.Value[item.FlagProperty]);
                w.WriteSummary(2, refValue.GetLabel(item));
                w.WriteLine(2, $"{refValue.Name} = 0b{Convert.ToString(flag, 2)},");
                if (flagValues.IndexOf(refValue) != flagValues.Count - 1)
                {
                    w.WriteLine();
                }
            }

            w.WriteLine(1, "}");
            w.WriteLine();
            w.WriteLine(1, "#endregion");
            w.WriteLine();
        }
    }

    /// <summary>
    /// Génère les propriétés.
    /// </summary>
    /// <param name="w">Writer.</param>
    /// <param name="item">La classe générée.</param>
    /// <param name="tag">Tag.</param>
    protected virtual void GenerateProperties(CSharpWriter w, Class item, string tag)
    {
        var sameColumnSet = new HashSet<string>(item.Properties
            .GroupBy(g => g.SqlName).Where(g => g.Count() > 1).Select(g => g.Key));

        foreach (var property in item.Properties.Where(p => p is not CompositionProperty cp || Classes.Contains(cp.Composition)))
        {
            if (item.Properties.IndexOf(property) > 0)
            {
                w.WriteLine();
            }

            GenerateProperty(w, property, sameColumnSet, tag);
        }
    }

    /// <summary>
    /// Génère la propriété concernée.
    /// </summary>
    /// <param name="w">Writer.</param>
    /// <param name="property">La propriété générée.</param>
    /// <param name="sameColumnSet">Sets des propriétés avec le même nom de colonne, pour ne pas les gérerer (genre alias).</param>
    /// <param name="tag">Tag.</param>
    protected virtual void GenerateProperty(CSharpWriter w, IProperty property, HashSet<string> sameColumnSet, string tag)
    {
        w.WriteSummary(1, property.Comment);

        var cp = property switch
        {
            CompositionProperty c => c,
            AliasProperty { Property: CompositionProperty c } => c,
            _ => null
        };

        var type = Config.GetType(property, Classes, nonNullable: cp != null && cp.Required || property.Required && Config.RequiredNonNullable(tag));

        if (!property.Class.Abstract)
        {
            var prop = (property as AliasProperty)?.PersistentProperty ?? property;
            if (
                (property.Class.IsPersistent || property is AliasProperty { PersistentProperty: not null, As: null } && !Config.NoColumnOnAlias)
                && Classes.Contains(prop.Class)
                && !Config.NoPersistence(tag)
                && !sameColumnSet.Contains(property.SqlName))
            {
                var sqlName = Config.UseLowerCaseSqlNames ? property.SqlName.ToLower() : property.SqlName;
                if (!Config.GetDomainAnnotations(property, tag).Any(a => a.TrimStart('[').StartsWith("Column")))
                {
                    w.WriteAttribute(1, "Column", $@"""{sqlName}""");
                }
            }

            if (property.Required && !Config.RequiredNonNullable(tag) && !property.PrimaryKey || property is AliasProperty { PrimaryKey: true } || property.PrimaryKey && property.Class.PrimaryKey.Count() > 1)
            {
                w.WriteAttribute(1, "Required");
            }

            if (Config.Kinetix)
            {
                var ap = (prop as AssociationProperty) ?? ((prop as AliasProperty)?.Property as AssociationProperty);
                if (ap != null && Classes.Contains(ap.Association) && ap.Association.IsPersistent && ap.Association.Reference)
                {
                    w.WriteAttribute(1, "ReferencedType", $"typeof({ap.Association.NamePascal})");
                }
                else if (property is AliasProperty alp2 && !alp2.AliasedPrimaryKey && alp2.Property.PrimaryKey && Classes.Contains(alp2.Property.Class) && alp2.Property.Class.Reference)
                {
                    w.WriteAttribute(1, "ReferencedType", $"typeof({alp2.Property.Class.NamePascal})");
                }
            }

            if (Config.Kinetix && property is not CompositionProperty and not AliasProperty { Property: CompositionProperty })
            {
                w.WriteAttribute(1, "Domain", $@"Domains.{property.Domain.CSharpName}");
            }

            if (type?.TrimEnd('?') == "string" && property.Domain.Length != null)
            {
                w.WriteAttribute(1, "StringLength", $"{property.Domain.Length}");
            }

            foreach (var annotation in Config.GetDomainAnnotations(property, tag))
            {
                w.WriteAttribute(1, annotation);
            }

            if (Config.IsPersistent(property.Class, tag) && property is AssociationProperty { Type: AssociationType.OneToMany or AssociationType.ManyToMany })
            {
                w.WriteAttribute(1, "NotMapped");
            }

            var isPk = property.Class.IsPersistent && property.PrimaryKey && property.Class.PrimaryKey.Count() == 1;

            if (isPk)
            {
                w.WriteAttribute(1, "Key");
            }

            var defaultValue = Config.GetValue(property, Classes);

            if (cp != null && cp.Required)
            {
                var newableType = GetNewableType(cp);
                if (newableType != null && (!Config.RequiredNonNullable(tag) || newableType != type))
                {
                    defaultValue = $"new {newableType}()";
                }
            }

            w.Write(1, "public");

            if (Config.RequiredNonNullable(tag) && property.Required && (!isPk || !property.Domain.AutoGeneratedValue) && defaultValue == "null")
            {
                w.Write(" required");
            }

            w.WriteLine($" {type} {property.NamePascal} {{ get; set; }}{(defaultValue != "null" ? $" = {defaultValue};" : string.Empty)}");
        }
        else
        {
            w.WriteLine(1, $"{type} {property.NamePascal} {{ get; }}");
        }
    }

    /// <summary>
    /// Génération des imports.
    /// </summary>
    /// <param name="w">Writer.</param>
    /// <param name="item">Classe concernée.</param>
    /// <param name="tag">Tag.</param>
    protected virtual void GenerateUsings(CSharpWriter w, Class item, string tag)
    {
        var usings = new List<string>();

        if (!item.Abstract)
        {
            if (item.Reference && item.DefaultProperty != null)
            {
                usings.Add("System.ComponentModel");
            }

            if (item.Properties.Any(p => p.Required || p.PrimaryKey || Config.GetType(p)?.TrimEnd('?') == "string" && p.Domain.Length != null))
            {
                usings.Add("System.ComponentModel.DataAnnotations");
            }

            if (item.Properties.Any(fp =>
                {
                    var prop = (fp as AliasProperty)?.PersistentProperty ?? fp;
                    return (fp.Class.IsPersistent || fp is AliasProperty { PersistentProperty: not null, As: null } && !Config.NoColumnOnAlias)
                        && Classes.Contains(prop.Class)
                        && !Config.NoPersistence(tag);
                }))
            {
                usings.Add("System.ComponentModel.DataAnnotations.Schema");
            }

            if (item.Properties.Any(p => p is not CompositionProperty and not AliasProperty { Property: CompositionProperty }) && Config.Kinetix)
            {
                usings.Add("Kinetix.Modeling.Annotations");
                usings.Add(Config.DomainNamespace);
            }

            if (item.Extends != null)
            {
                usings.Add(GetNamespace(item.Extends, tag));
            }
        }

        foreach (var @using in Config.GetDecoratorImports(item, tag))
        {
            usings.Add(@using);
        }

        foreach (var property in item.Properties)
        {
            usings.AddRange(Config.GetDomainImports(property, tag));
            usings.AddRange(Config.GetValueImports(property));

            switch (property)
            {
                case AssociationProperty ap when Classes.Contains(ap.Association) && (Config.CanClassUseEnums(ap.Association, Classes, ap.Property) || Config.Kinetix && ap.Association.IsPersistent && ap.Association.Reference):
                    usings.Add(GetNamespace(ap.Association, tag));
                    break;
                case AliasProperty { Property: AssociationProperty ap2 } when Classes.Contains(ap2.Association) && (Config.CanClassUseEnums(ap2.Association, Classes, ap2.Property) || Config.Kinetix && ap2.Association.IsPersistent && ap2.Association.Reference):
                    usings.Add(GetNamespace(ap2.Association, tag));
                    break;
                case AliasProperty { Property: RegularProperty rp } alp when Classes.Contains(rp.Class) && (Config.CanClassUseEnums(rp.Class, Classes, rp) || Config.Kinetix && !alp.AliasedPrimaryKey && rp.PrimaryKey && rp.Class.Reference):
                    usings.Add(GetNamespace(rp.Class, tag));
                    break;
                case CompositionProperty cp when Classes.Contains(cp.Composition):
                    usings.Add(GetNamespace(cp.Composition, tag));
                    break;
                case AliasProperty { Property: CompositionProperty cp } when Classes.Contains(cp.Composition):
                    usings.Add(GetNamespace(cp.Composition, tag));
                    break;
            }
        }

        var finalUsings = usings
            .Where(u => !GetNamespace(item, tag).StartsWith(u))
            .Distinct()
            .ToArray();

        w.WriteUsings(finalUsings);

        if (finalUsings.Length > 0)
        {
            w.WriteLine();
        }
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Config.GetClassFileName(classe, tag);
    }

    protected virtual string GetNamespace(Class classe, string tag)
    {
        return Config.GetNamespace(classe, classe.Tags.Contains(tag) ? tag : classe.Tags.Intersect(Config.Tags).FirstOrDefault() ?? tag);
    }

    protected virtual string? GetNewableType(CompositionProperty property)
    {
        var type = Config.GetType(property, nonNullable: true);
        var genericType = type.Split('<').First();

        if (property.Domain == null)
        {
            return type;
        }

        if (NewableTypes.TryGetValue(genericType, out var newableType))
        {
            return type.Replace(genericType, newableType);
        }

        return null;
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        using var w = new CSharpWriter(fileName, _logger);

        GenerateUsings(w, classe, tag);
        w.WriteNamespace(Config.GetNamespace(classe, tag));
        w.WriteSummary(classe.Comment);
        GenerateClassDeclaration(w, classe, tag);
    }
}