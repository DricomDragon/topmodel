﻿using Microsoft.Extensions.Logging;
using TopModel.Core;

namespace TopModel.Generator.Sql.Procedural;

/// <summary>
/// Générateur de PL-SQL.
/// </summary>
public class PostgreSchemaGenerator : AbstractSchemaGenerator
{
    public PostgreSchemaGenerator(SqlConfig config, ILogger<ProceduralSqlGenerator> logger, TranslationStore translationStore)
        : base(config, logger, translationStore)
    {
    }

    protected override string BatchSeparator => ";";

    protected override bool AllowTablespace => true;

    protected override string JsonType => "jsonb";

    protected override bool SupportsClusteredKey => false;

    protected override bool UseQuotes => false;

    protected override void WriteComments(SqlFileWriter writerComment, Class classe, string tableName, List<IProperty> properties)
    {
        writerComment.WriteLine();
        writerComment.WriteLine("/**");
        writerComment.WriteLine("  * Commentaires pour la table " + tableName);
        writerComment.WriteLine(" **/");
        writerComment.WriteLine($"COMMENT ON TABLE {tableName} IS '{classe.Comment.Replace("'", "''")}'{BatchSeparator}");
        foreach (var p in properties)
        {
            writerComment.WriteLine($"COMMENT ON COLUMN {tableName}.{p.SqlName} IS '{p.Comment.Replace("'", "''")}'{BatchSeparator}");
        }
    }

    /// <summary>
    /// Gère l'auto-incrémentation des clés primaires en ajoutant identity à la colonne.
    /// </summary>
    /// <param name="writerCrebas">Flux d'écriture création bases.</param>
    protected override void WriteIdentityColumn(SqlFileWriter writerCrebas)
    {
        writerCrebas.Write(" generated by default as identity");
        if (Config.Identity.Increment != null || Config.Identity.Start != null)
        {
            writerCrebas.Write(" (");
            if (Config.Identity.Start != null)
            {
                writerCrebas.Write($"{$"start with {Config.Identity.Start}"}");
            }

            if (Config.Identity.Increment != null)
            {
                writerCrebas.Write($"{$" increment {Config.Identity.Increment}"}");
            }

            writerCrebas.Write(")");
        }
    }

    protected override string GetSequenceName(Class classe)
    {
        return $"SEQ_{classe.SqlName}";
    }

    protected override void WriteSequenceDeclaration(Class classe, SqlFileWriter writerCrebas, string tableName)
    {
        writerCrebas.Write($"create sequence {GetSequenceName(classe)} as {MainConfig.GetType(classe.PrimaryKey.Single()).ToUpper()}");

        if (Config.Identity.Start != null)
        {
            writerCrebas.Write($"{$" start {Config.Identity.Start}"}");
        }

        if (Config.Identity.Increment != null)
        {
            writerCrebas.Write($"{$" increment {Config.Identity.Increment}"}");
        }

        writerCrebas.Write($" owned by {tableName}.{classe.PrimaryKey.Single().SqlName}");
    }
}