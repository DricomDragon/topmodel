﻿using TopModel.Core;
using TopModel.Core.Model.Implementation;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaModelPropertyGenerator
{
    private readonly IEnumerable<Class> _classes;
    private readonly JpaConfig _config;
    private readonly Dictionary<string, string> _newableTypes;

    public JpaModelPropertyGenerator(JpaConfig config, IEnumerable<Class> classes, Dictionary<string, string> newableTypes)
    {
        _classes = classes;
        _config = config;
        _newableTypes = newableTypes;
    }

    public void WriteCompositePrimaryKeyClass(JavaWriter fw, Class classe, string tag)
    {
        if (classe.PrimaryKey.Count() <= 1 || !classe.IsPersistent)
        {
            return;
        }

        fw.WriteLine();
        fw.AddImport("java.io.Serializable");
        fw.WriteLine(1, @$"public static class {classe.NamePascal}Id implements Serializable {{");
        foreach (var pk in classe.PrimaryKey)
        {
            fw.WriteLine(2, $"private {_config.GetType(pk, _classes, true)} {pk.NameByClassCamel};");
            fw.WriteLine();
        }

        foreach (var pk in classe.PrimaryKey)
        {
            WriteGetter(fw, classe, tag, pk, 2);
            WriteSetter(fw, classe, tag, pk, 2);
        }

        fw.WriteLine();
        fw.WriteLine(2, "public boolean equals(Object o) {");
        fw.WriteLine(3, $@"if(!(o instanceof {classe.NamePascal}Id)) {{");
        fw.WriteLine(4, "return false;");
        fw.WriteLine(3, "}");
        fw.WriteLine();
        fw.WriteLine(3, $"{classe.NamePascal}Id oId = ({classe.NamePascal}Id) o;");
        fw.WriteLine(3, $@"return !({string.Join("\n || ", classe.PrimaryKey.Select(pk => $@"!this.{pk.NameByClassCamel}.equals(oId.{pk.NameByClassCamel})"))});");
        fw.WriteLine(2, "}");

        fw.WriteLine();
        fw.WriteLine(2, "@Override");
        fw.WriteLine(2, "public int hashCode() {");
        fw.WriteLine(3, $"return Objects.hash({string.Join(", ", classe.PrimaryKey.Select(pk => pk.NameByClassCamel))});");
        fw.AddImport("java.util.Objects");
        fw.WriteLine(2, "}");
        fw.WriteLine(1, "}");
    }

    public void WriteGetter(JavaWriter fw, Class classe, string tag, IProperty property, int indentLevel = 1)
    {
        var propertyName = _config.UseJdbc ? property.NameCamel : property.NameByClassCamel;
        fw.WriteLine();
        fw.WriteDocStart(indentLevel, $"Getter for {propertyName}");
        fw.WriteReturns(indentLevel, $"value of {{@link {classe.GetImport(_config, tag)}#{propertyName} {propertyName}}}");
        fw.WriteDocEnd(indentLevel);

        var getterPrefix = _config.GetType(property, _classes, true) == "boolean" ? "is" : "get";
        var getterName = propertyName.ToPascalCase().WithPrefix(getterPrefix);
        if (property.Class.PreservePropertyCasing)
        {
            getterName = propertyName.ToFirstUpper().WithPrefix(getterPrefix);
        }

        fw.WriteLine(indentLevel, @$"public {_config.GetType(property, useClassForAssociation: classe.IsPersistent && !_config.UseJdbc)} {getterName}() {{");
        if (property is AssociationProperty ap && ap.Type.IsToMany())
        {
            var type = _config.GetType(ap, _classes, useClassForAssociation: classe.IsPersistent && !_config.UseJdbc).Split('<').First();
            if (_newableTypes.TryGetValue(type, out var newableType))
            {
                fw.WriteLine(indentLevel + 1, $"if(this.{propertyName} == null)");
                fw.AddImport($"java.util.{newableType}");
                fw.WriteLine(indentLevel + 2, $"this.{propertyName} = new {newableType}<>();");
            }
        }

        fw.WriteLine(indentLevel + 1, @$"return this.{propertyName};");
        fw.WriteLine(indentLevel, "}");
    }

    public void WriteProperties(JavaWriter fw, Class classe, string tag)
    {
        var properties = _config.UseJdbc ? classe.Properties.Where(p => !(p is AssociationProperty ap && (ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.ManyToMany))) : classe.GetProperties(_classes);
        foreach (var property in properties)
        {
            WriteProperty(fw, classe, property, tag);
        }
    }

    public void WriteProperty(JavaWriter fw, CompositionProperty property)
    {
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"private {_config.GetType(property)} {property.NameCamel};");
    }

    public void WriteProperty(JavaWriter fw, Class classe, IProperty property, string tag)
    {
        fw.WriteLine();
        fw.WriteDocStart(1, property.Comment);
        switch (property)
        {
            case CompositionProperty cp:
                WriteProperty(fw, cp);
                break;
            case AssociationProperty { Association.IsPersistent: true } ap:
                WriteProperty(fw, classe, ap, tag);
                break;
            case IFieldProperty fp:
                WriteProperty(fw, classe, fp, tag);
                break;
        }
    }

    public void WriteSetter(JavaWriter fw, Class classe, string tag, IProperty property, int indentLevel = 1)
    {
        var propertyName = _config.UseJdbc ? property.NameCamel : property.NameByClassCamel;
        fw.WriteLine();
        fw.WriteDocStart(indentLevel, $"Set the value of {{@link {classe.GetImport(_config, tag)}#{propertyName} {propertyName}}}");
        fw.WriteLine(indentLevel, $" * @param {propertyName} value to set");
        fw.WriteDocEnd(indentLevel);
        fw.WriteLine(indentLevel, @$"public void {propertyName.WithPrefix("set")}({_config.GetType(property, useClassForAssociation: classe.IsPersistent && !_config.UseJdbc)} {propertyName}) {{");
        fw.WriteLine(indentLevel + 1, @$"this.{propertyName} = {propertyName};");
        fw.WriteLine(indentLevel, "}");
    }

    private void WriteManyToMany(JavaWriter fw, Class classe, AssociationProperty property)
    {
        var role = property.Role is not null ? "_" + property.Role.ToConstantCase() : string.Empty;
        var fk = ((IFieldProperty)property).SqlName;
        var pk = classe.PrimaryKey.Single().SqlName + role;
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        if (!_config.CanClassUseEnums(property.Association))
        {
            fw.AddImport($"{javaOrJakarta}.persistence.CascadeType");
        }

        var cascade = _config.CanClassUseEnums(property.Association) ? string.Empty : $", cascade = {{ CascadeType.PERSIST, CascadeType.MERGE }}";
        if (property is ReverseAssociationProperty rap)
        {
            fw.WriteLine(1, @$"@{property.Type}(fetch = FetchType.LAZY, mappedBy = ""{rap.ReverseProperty.NameByClassCamel}""{cascade})");
        }
        else
        {
            fw.AddImport($"{javaOrJakarta}.persistence.JoinTable");
            fw.WriteLine(1, @$"@{property.Type}(fetch = FetchType.LAZY{cascade})");
            fw.WriteLine(1, @$"@JoinTable(name = ""{property.Class.SqlName}_{property.Association.SqlName}{(property.Role != null ? "_" + property.Role.ToConstantCase() : string.Empty)}"", joinColumns = @JoinColumn(name = ""{pk}""), inverseJoinColumns = @JoinColumn(name = ""{fk}""))");
            fw.AddImport($"{javaOrJakarta}.persistence.JoinColumn");
        }
    }

    private void WriteManyToOne(JavaWriter fw, AssociationProperty property)
    {
        var fk = ((IFieldProperty)property).SqlName;
        var apk = property.Property.SqlName;
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        fw.WriteLine(1, @$"@{property.Type}(fetch = FetchType.LAZY, optional = {(property.Required ? "false" : "true")}, targetEntity = {property.Association.NamePascal}.class)");
        fw.WriteLine(1, @$"@JoinColumn(name = ""{fk}"", referencedColumnName = ""{apk}"")");
        fw.AddImport($"{javaOrJakarta}.persistence.JoinColumn");
    }

    private void WriteOneToMany(JavaWriter fw, Class classe, AssociationProperty property)
    {
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        fw.AddImport($"{javaOrJakarta}.persistence.CascadeType");
        if (property is ReverseAssociationProperty rap)
        {
            fw.WriteLine(1, @$"@{property.Type}(cascade = {{CascadeType.PERSIST, CascadeType.MERGE}}, fetch = FetchType.LAZY, mappedBy = ""{rap.ReverseProperty.NameByClassCamel}"")");
        }
        else
        {
            var pk = classe.PrimaryKey.Single().SqlName;
            var hasReverse = property.Class.Namespace.RootModule == property.Association.Namespace.RootModule;
            fw.WriteLine(1, @$"@{property.Type}(cascade = CascadeType.ALL, fetch = FetchType.LAZY{(hasReverse ? @$", mappedBy = ""{property.Class.NameCamel}{property.Role ?? string.Empty}""" : string.Empty)})");
            if (!hasReverse)
            {
                fw.WriteLine(1, @$"@JoinColumn(name = ""{pk}"", referencedColumnName = ""{pk}"")");
                fw.AddImport($"{javaOrJakarta}.persistence.JoinColumn");
            }
        }
    }

    private void WriteOneToOne(JavaWriter fw, AssociationProperty property)
    {
        var fk = ((IFieldProperty)property).SqlName;
        var apk = property.Property.SqlName;
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        fw.AddImport($"{javaOrJakarta}.persistence.CascadeType");
        fw.WriteLine(1, @$"@{property.Type}(fetch = FetchType.LAZY, cascade = CascadeType.ALL, optional = {(!property.Required).ToString().ToLower()})");
        fw.WriteLine(1, @$"@JoinColumn(name = ""{fk}"", referencedColumnName = ""{apk}"", unique = true)");
        fw.AddImport($"{javaOrJakarta}.persistence.JoinColumn");
    }

    private void WriteProperty(JavaWriter fw, Class classe, AssociationProperty property, string tag)
    {
        fw.WriteDocEnd(1);
        if (!_config.UseJdbc)
        {
            var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
            fw.AddImport($"{javaOrJakarta}.persistence.FetchType");
            fw.AddImport($"{javaOrJakarta}.persistence.{property.Type}");
            switch (property.Type)
            {
                case AssociationType.ManyToOne:
                    WriteManyToOne(fw, property);
                    break;
                case AssociationType.OneToMany:
                    WriteOneToMany(fw, classe, property);
                    break;
                case AssociationType.ManyToMany:
                    WriteManyToMany(fw, classe, property);
                    break;
                case AssociationType.OneToOne:
                    WriteOneToOne(fw, property);
                    break;
            }

            if (property.Type == AssociationType.ManyToMany || property.Type == AssociationType.OneToMany)
            {
                if (property.Association.OrderProperty != null && _config.GetType(property, _classes, classe.IsPersistent).Contains("List"))
                {
                    fw.WriteLine(1, @$"@OrderBy(""{property.Association.OrderProperty.NameByClassCamel} ASC"")");
                    fw.AddImport($"{javaOrJakarta}.persistence.OrderBy");
                }
            }

            var suffix = string.Empty;
            if (property.Association.PrimaryKey.Count() == 1 && _config.CanClassUseEnums(property.Association, _classes, prop: property.Association.PrimaryKey.Single()))
            {
                var defaultValue = _config.GetValue(property, _classes);
                if (defaultValue != "null")
                {
                    fw.AddImport($"{_config.GetEnumPackageName(classe, tag)}.{_config.GetType(property.Association.PrimaryKey.Single())}");
                    suffix = $" = new {property.Association.NamePascal}({defaultValue})";
                }
            }

            if (property.PrimaryKey)
            {
                fw.AddImport($"{javaOrJakarta}.persistence.Id");
                fw.WriteLine(1, "@Id");
            }

            fw.WriteLine(1, $"private {_config.GetType(property, useClassForAssociation: classe.IsPersistent)} {property.NameByClassCamel}{suffix};");
        }
        else
        {
            if (property.PrimaryKey && classe.PrimaryKey.Count() <= 1)
            {
                fw.AddImport("org.springframework.data.annotation.Id");
                fw.WriteLine(1, "@Id");
            }

            fw.AddImport("org.springframework.data.relational.core.mapping.Column");
            fw.WriteLine(1, $@"@Column(""{property.Property.SqlName.ToLower()}"")");
            fw.WriteLine(1, $"private {_config.GetType(property)} {property.NameCamel};");
        }
    }

    private void WriteProperty(JavaWriter fw, Class classe, IFieldProperty property, string tag)
    {
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        if (property is AliasProperty alp)
        {
            fw.WriteLine(1, $" * Alias of {{@link {alp.Property.Class.GetImport(_config, tag)}#get{alp.Property.NameCamel.ToFirstUpper()}() {alp.Property.Class.NamePascal}#get{alp.Property.NameCamel.ToFirstUpper()}()}} ");
        }

        fw.WriteDocEnd(1);
        if (property.PrimaryKey && classe.IsPersistent)
        {
            if (!_config.UseJdbc)
            {
                fw.AddImport($"{javaOrJakarta}.persistence.Id");

                if (property.Domain.AutoGeneratedValue && classe.PrimaryKey.Count() == 1)
                {
                    fw.AddImports(new List<string>
                {
                    $"{javaOrJakarta}.persistence.GeneratedValue",
                    $"{javaOrJakarta}.persistence.GenerationType"
                });

                    if (_config.Identity.Mode == IdentityMode.IDENTITY)
                    {
                        fw.WriteLine(1, @$"@GeneratedValue(strategy = GenerationType.IDENTITY)");
                    }
                    else if (_config.Identity.Mode == IdentityMode.SEQUENCE)
                    {
                        fw.AddImport($"{javaOrJakarta}.persistence.SequenceGenerator");
                        var seqName = $"SEQ_{classe.SqlName}";
                        var initialValue = _config.Identity.Start != null ? $", initialValue = {_config.Identity.Start}" : string.Empty;
                        var increment = _config.Identity.Increment != null ? $", allocationSize = {_config.Identity.Increment}" : string.Empty;
                        fw.WriteLine(1, @$"@SequenceGenerator(name = ""{seqName}"", sequenceName = ""{seqName}""{initialValue}{increment})");
                        fw.WriteLine(1, @$"@GeneratedValue(strategy = GenerationType.SEQUENCE, generator = ""{seqName}"")");
                    }
                }
            }
            else
            {
                fw.AddImport("org.springframework.data.annotation.Id");
            }

            fw.WriteLine(1, "@Id");
        }

        if ((classe.IsPersistent || _config.UseJdbc) && !_config.GetImplementation(property.Domain)!.Annotations
        .Where(i =>
                classe.IsPersistent && (Target.Persisted & i.Target) > 0
            || !classe.IsPersistent && (Target.Dto & i.Target) > 0)
            .Any(a => a.Text.Replace("@", string.Empty).StartsWith("Column")))
        {
            string column = string.Empty;
            if (!_config.UseJdbc)
            {
                column = @$"@Column(name = ""{property.SqlName}"", nullable = {(!property.Required).ToString().ToFirstLower()}";
                if (property.Domain.Length != null)
                {
                    if (_config.GetImplementation(property.Domain)?.Type?.ToUpper() == "STRING")
                    {
                        column += $", length = {property.Domain.Length}";
                    }
                    else
                    {
                        column += $", precision = {property.Domain.Length}";
                    }
                }

                if (property.Domain.Scale != null)
                {
                    column += $", scale = {property.Domain.Scale}";
                }

                column += @$", columnDefinition = ""{property.Domain.Implementations["sql"].Type}""";
                column += ")";
                fw.AddImport($"{javaOrJakarta}.persistence.Column");
            }
            else
            {
                fw.AddImport("org.springframework.data.relational.core.mapping.Column");
                column = $@"@Column(""{property.SqlName.ToLower()}"")";
            }

            fw.WriteLine(1, column);
        }

        if (property.Required && !property.PrimaryKey && (!classe.IsPersistent || _config.UseJdbc))
        {
            fw.WriteLine(1, @$"@NotNull");
            fw.AddImport($"{javaOrJakarta}.validation.constraints.NotNull");
        }

        if (_config.CanClassUseEnums(classe) && property.PrimaryKey && !_config.UseJdbc)
        {
            fw.AddImports(new List<string>
            {
                $"{javaOrJakarta}.persistence.Enumerated",
                $"{javaOrJakarta}.persistence.EnumType",
            });
            fw.WriteLine(1, "@Enumerated(EnumType.STRING)");
        }

        foreach (var annotation in _config.GetDomainAnnotations(property, tag))
        {
            fw.WriteLine(1, $"{(annotation.StartsWith("@") ? string.Empty : '@')}{annotation}");
        }

        var defaultValue = _config.GetValue(property, _classes);
        var suffix = defaultValue != "null" ? $" = {defaultValue}" : string.Empty;
        fw.WriteLine(1, $"private {_config.GetType(property, useClassForAssociation: classe.IsPersistent && !_config.UseJdbc)} {property.NameByClassCamel}{suffix};");
    }
}
