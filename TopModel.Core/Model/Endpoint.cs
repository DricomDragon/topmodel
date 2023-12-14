﻿using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Core;

public class Endpoint : IPropertyContainer
{
    public Namespace Namespace { get; set; }

#nullable disable
    public ModelFile ModelFile { get; set; }

    public IEnumerable<string> Tags => ModelFile.Tags.Concat(OwnTags).Distinct();

    public LocatedString Name { get; set; }

    public string NamePascal => Name.Value.ToPascalCase();

    public string NameCamel => Name.Value.ToCamelCase();

    public string Method { get; set; }

    public string Route { get; set; }

    public string FullRoute
    {
        get
        {
            var endpointPrefix = (ModelFile.Options?.Endpoints?.Prefix ?? string.Empty).Trim('/');
            var endpointRoute = Route.Trim('/');
            return $"{endpointPrefix}{(string.IsNullOrEmpty(endpointPrefix) || string.IsNullOrEmpty(endpointRoute) ? string.Empty : "/")}{endpointRoute}";
        }
    }

    public string Description { get; set; }
#nullable enable

    public IProperty? Returns { get; set; }

    public IList<IProperty> Params { get; set; } = new List<IProperty>();

    public bool IsMultipart => Params.Any(p => p.Domain?.IsMultipart ?? false || p is CompositionProperty cp && cp.IsMultipart);

    public IList<IProperty> Properties => Params.Concat(new[] { Returns! }).Where(p => p != null).ToList();

    public bool PreservePropertyCasing { get; set; }

    public List<(Decorator Decorator, string[] Parameters)> Decorators { get; } = new();

    public IEnumerable<ClassDependency> ClassDependencies => Properties.GetClassDependencies();

    public List<DecoratorReference> DecoratorReferences { get; } = new();

#nullable disable
    internal Reference Location { get; set; }

    internal List<string> OwnTags { get; set; } = new();

#nullable enable

    public override string ToString()
    {
        return Name;
    }
}