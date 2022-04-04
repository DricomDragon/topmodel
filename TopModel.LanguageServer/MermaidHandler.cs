using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using TopModel.Core;

namespace TopModel.LanguageServer;

public class MermaidHandler : IRequestHandler<MermaidRequest, Mermaid>, IJsonRpcHandler
{
    ModelStore _modelStore;
    ILanguageServerFacade _facade;

    public MermaidHandler(ModelStore modelStore, ILanguageServerFacade facade)
    {
        _modelStore = modelStore;
        _facade = facade;
    }

    public Task<Mermaid> Handle(MermaidRequest request, CancellationToken cancellationToken)
    {
        var file = _modelStore.Files.SingleOrDefault(f => _facade.GetFilePath(f) == request.Uri);
        var result = GenerateDiagramFile(file!);
        return Task.FromResult<Mermaid>(new Mermaid(result, file!.Name));
    }

    public string GenerateDiagramFile(TopModel.Core.FileModel.ModelFile file)
    {
        string diagram = string.Empty;
        var classes = file.Classes
            .Where(c => c.IsPersistent);

        diagram += "classDiagram\n";
        var notClasses = new List<Class>();
        foreach (var classe in classes)
        {
            diagram += @$"%% {classe.Comment}" + '\n';
            diagram += @$"class {classe.Name}{{" + '\n';
            if (classe.Reference && classe.ReferenceValues != null && classe.ReferenceValues.Count() > 0)
            {
                diagram += "&lt;&lt;Enum&gt;&gt;\n";
                foreach (var refValue in classe.ReferenceValues.OrderBy(x => x.Name, StringComparer.Ordinal))
                {
                    var code = classe.PrimaryKey == null || classe.PrimaryKey.Domain.Name != "DO_ID"
                        ? (string)refValue.Value[classe.PrimaryKey ?? classe.Properties.OfType<IFieldProperty>().First()]
                        : (string)refValue.Value[classe.UniqueKeys!.First().First()];

                    diagram += code + '\n';
                }
                diagram += "}\n";
                continue;
            }
            foreach (var property in classe.Properties.OfType<RegularProperty>())
            {
                diagram += $" {property.Domain.Name} {property.Name}\n";
            }
            diagram += "}\n";

            foreach (var property in classe.Properties.OfType<AssociationProperty>())
            {
                if (property.Association.ModelFile != file)
                {
                    notClasses.Add(property.Association);
                }
                string cardLeft;
                string cardRight;
                switch (property.Type)
                {
                    case AssociationType.OneToOne:
                        cardLeft = property.Required ? "1" : "0..1";
                        cardRight = "1";
                        break;
                    case AssociationType.OneToMany:
                        cardLeft = property.Required ? "1..*" : "0..*";
                        cardRight = property.Required ? "1" : "0..1";
                        break;
                    case AssociationType.ManyToOne:
                        cardLeft = property.Required ? "1" : "0..1";
                        cardRight = "0..*";
                        break;
                    case AssociationType.ManyToMany:
                    default:
                        cardLeft = property.Required ? "1..*" : "1..*";
                        cardRight = "0..*";
                        break;
                }

                diagram += @$"{property.Class.Name} ""{cardLeft}"" --> ""{cardRight}"" {property.Association.Name}{(property.Role != null ? " : " + property.Role : "")}" + '\n';
            }
            foreach (var property in classe.Properties.OfType<CompositionProperty>())
            {
                diagram += $"{property.Class.Name} --* {property.Composition.Name}\n";
            }
        }
        foreach (var classe in notClasses)
        {
            diagram += @$"%% {classe.Comment}" + '\n';
            diagram += @$"class {classe.Name}:::fileReference" + '\n';
        }


        foreach (var classe in classes.Where(c => c.Extends is not null))
        {
            diagram += @$"{classe.Extends!.Name} <|--  {classe.Name}" + '\n';
        }

        diagram += "\n";
        return diagram;
    }

}
