using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TopModel.Core;

public static class ModelConfigExtensions {

    public static DocumentSelector GetDocumentSelector(this ModelConfig config)
    {
        return DocumentSelector.ForPattern($"{config.ModelRoot}/**/*.tmd");

    }
}