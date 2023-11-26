﻿using TopModel.Core.FileModel;

namespace TopModel.Core;

public class ClassMappings
{
#nullable disable
    public bool To { get; set; }

    public LocatedString Name { get; set; }

    public Class Class { get; set; }

    public ClassReference ClassReference { get; set; }

    public bool Required { get; set; } = true;

#nullable enable
    public string? Comment { get; set; }

    public Dictionary<IProperty, IProperty> Mappings { get; } = new();

    public Dictionary<Reference, Reference> MappingReferences { get; } = new();
}
