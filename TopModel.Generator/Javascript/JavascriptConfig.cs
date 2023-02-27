﻿using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

/// <summary>
/// Paramètres pour la génération du Javascript.
/// </summary>
public class JavascriptConfig : GeneratorConfigBase
{
    /// <summary>
    /// Localisation du modèle, relative au répertoire de génération. Si non renseigné, aucun fichier ne sera généré.
    /// </summary>
    public string? ModelRootPath { get; set; }

    /// <summary>
    /// Localisation des ressources i18n, relative au répertoire de génération. Si non renseigné, aucun fichier ne sera généré.
    /// </summary>
    public string? ResourceRootPath { get; set; }

    /// <summary>
    /// Localisation des clients d'API, relative au répertoire de génération. Si non renseigné, aucun fichier ne sera généré.
    /// </summary>
    public string? ApiClientRootPath { get; set; }

    /// <summary>
    /// Chemin vers lequel sont créés les fichiers d'endpoints générés, relatif à la racine de l'API.
    /// </summary>
    public string ApiClientFilePath { get; set; } = "{module}";

    /// <summary>
    /// Chemin (ou alias commençant par '@') vers un 'fetch' personnalisé, relatif au répertoire de génération.
    /// </summary>
    public string FetchPath { get; set; } = "@focus4/core";

    /// <summary>
    /// Chemin (ou alias commençant par '@') vers le fichier 'domain', relatif au répertoire de génération.
    /// </summary>
    public string DomainPath { get; set; } = "./domains";

    /// <summary>
    /// Framework cible pour la génération.
    /// </summary>
    public TargetFramework TargetFramework { get; set; } = TargetFramework.FOCUS;

    /// <summary>
    /// Mode de génération (JS, JSON ou JSON Schema).
    /// </summary>
    public ResourceMode ResourceMode { get; set; }

    /// <summary>
    /// Mode de génération des listes de références (définitions ou valeurs).
    /// </summary>
    public ReferenceMode ReferenceMode { get; set; } = ReferenceMode.DEFINITION;

    /// <summary>
    /// Ajoute les commentaires dans les entités JS générées.
    /// </summary>
    public bool GenerateComments { get; set; }

    public override string[] PropertiesWithModuleVariableSupport => new[]
    {
        nameof(ApiClientFilePath)
    };

    public override string[] PropertiesWithTagVariableSupport => new[]
    {
        nameof(ModelRootPath),
        nameof(ResourceRootPath),
        nameof(ApiClientRootPath),
        nameof(FetchPath),
        nameof(DomainPath)
    };

    public string GetClassFileName(Class classe, string tag)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(ModelRootPath!, tag),
            classe.Namespace.ModulePathKebab,
            $"{classe.Name.ToKebabCase()}.ts")
        .Replace("\\", "/");
    }

    /// <summary>
    /// Récupère la valeur par défaut d'une propriété en JS.
    /// </summary>
    /// <param name="property">La propriété.</param>
    /// <returns>La valeur par défaut.</returns>
    public string GetDefaultValue(IProperty property)
    {
        var fp = property as IFieldProperty;

        if (fp?.DefaultValue == null || fp.DefaultValue == "null" || fp.DefaultValue == "undefined")
        {
            return "undefined";
        }

        if (fp.Domain?.TS?.Type == "string")
        {
            return $@"""{fp.DefaultValue}""";
        }

        return fp.DefaultValue;
    }

    public string GetEndpointsFileName(ModelFile file, string tag)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(ApiClientRootPath!, tag),
            ResolveVariables(ApiClientFilePath, module: file.Namespace.ModulePathKebab),
            $"{file.Options.Endpoints.FileName.ToKebabCase()}.ts")
        .Replace("\\", "/");
    }

    public List<(string Import, string Path)> GetEndpointImports(IEnumerable<Endpoint> endpoints, string tag, IEnumerable<Class> availableClasses)
    {
        return endpoints.SelectMany(e => e.ClassDependencies)
            .Select(dep => (
                Import: dep is { Source: IFieldProperty fp }
                    ? fp.GetPropertyTypeName().Replace("[]", string.Empty)
                    : dep.Classe.NamePascal,
                Path: GetImportPathForClass(dep, tag, availableClasses)!))
            .Concat(endpoints.SelectMany(d => d.DomainDependencies).Select(p => (Import: p.Domain.TS!.Type.ParseTemplate(p.Source).Replace("[]", string.Empty).Split("<").First(), Path: p.Domain.TS.Import!.ParseTemplate(p.Source))))
            .Where(i => i.Path != null)
            .GroupAndSort();
    }

    public string? GetImportPathForClass(ClassDependency dep, string tag, IEnumerable<Class> availableClasses)
    {
        string target;
        if (dep.Source is IFieldProperty fp)
        {
            if (fp.GetPropertyTypeName(availableClasses) != fp.Domain.TS!.Type && dep.Classe.EnumKey != null)
            {
                target = GetReferencesFileName(dep.Classe.Namespace, tag);
            }
            else
            {
                return null;
            }
        }
        else
        {
            target = GetClassFileName(dep.Classe, tag);
        }

        var source = dep.Source switch
        {
            IProperty { Class: Class classe } => GetClassFileName(classe, tag),
            IProperty { Endpoint: Endpoint endpoint } => GetEndpointsFileName(endpoint.ModelFile, tag),
            Class classe => GetClassFileName(classe, tag),
            _ => null
        };

        if (source == null)
        {
            return null;
        }

        var path = Path.GetRelativePath(string.Join('/', source.Split('/').SkipLast(1)), target)[..^3].Replace("\\", "/");

        if (!path.StartsWith("."))
        {
            path = $"./{path}";
        }

        return path;
    }

    public string GetReferencesFileName(Namespace ns, string tag)
    {
        return Path.Combine(OutputDirectory, ResolveVariables(ModelRootPath!, tag), ns.ModulePathKebab, "references.ts").Replace("\\", "/");
    }

    public string GetResourcesFilePath(Namespace ns, string tag, string lang)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(ResourceRootPath!, tag),
            lang,
            $"{ns.RootModule.ToKebabCase()}{(ResourceMode == ResourceMode.JS ? ".ts" : ".json")}")
        .Replace("\\", "/");
    }

    public string GetCommentResourcesFilePath(Namespace ns, string tag, string lang)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(ResourceRootPath!, tag),
            lang,
            $"{ns.RootModule.ToKebabCase()}.comments{(ResourceMode == ResourceMode.JS ? ".ts" : ".json")}")
        .Replace("\\", "/");
    }

    protected override string ResolveTagVariables(string value, string tag)
    {
        return base.ResolveTagVariables(value, tag).Trim('/');
    }
}
