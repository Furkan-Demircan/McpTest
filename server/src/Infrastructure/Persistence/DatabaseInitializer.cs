using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

/// <summary>
/// Demo için şema hazırlığı. EnsureCreated sadece veritabanı hiç yoksa çalışır; var olan
/// veritabanına yeni eklenen entity'lerin tablolarını eklemez. Bu yüzden veritabanı zaten
/// varsa modelde olup veritabanında olmayan tablolar (ve indeksleri) oluşturulur.
/// Mevcut tablolara dokunulmaz (kolon ekleme/değiştirme yok); bu bir migration değildir.
/// Gerçek sistemde EF migration kullanılmalıdır.
/// </summary>
public static class DatabaseInitializer
{
    public static void EnsureSchema(ApplicationDbContext context, ILogger logger)
    {
        if (context.Database.EnsureCreated())
        {
            logger.LogInformation("Veritabanı ve tüm tablolar oluşturuldu.");
            return;
        }

        var model = context.GetService<IDesignTimeModel>().Model;
        var operations = context.GetService<IMigrationsModelDiffer>()
            .GetDifferences(null, model.GetRelationalModel());

        var missingTables = operations
            .OfType<CreateTableOperation>()
            .Select(operation => operation.Name)
            .Where(table => !TableExists(context, table))
            .ToHashSet();

        if (missingTables.Count == 0)
        {
            logger.LogInformation("Veritabanı şeması güncel.");
            return;
        }

        // Sadece eksik tabloların oluşturma, indeks ve yabancı anahtar işlemleri
        var toApply = operations.Where(operation => operation switch
        {
            CreateTableOperation table => missingTables.Contains(table.Name),
            CreateIndexOperation index => missingTables.Contains(index.Table),
            AddForeignKeyOperation foreignKey => missingTables.Contains(foreignKey.Table),
            _ => false
        }).ToList();

        var commands = context.GetService<IMigrationsSqlGenerator>().Generate(toApply, model);
        context.GetService<IMigrationCommandExecutor>()
            .ExecuteNonQuery(commands, context.GetService<IRelationalConnection>());

        logger.LogInformation("Eksik tablolar oluşturuldu: {Tables}", string.Join(", ", missingTables));
    }

    // PostgreSQL (Npgsql) için: tablo mevcut şemada var mı?
    private static bool TableExists(ApplicationDbContext context, string table) =>
        context.Database
            .SqlQuery<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = current_schema() AND table_name = {table}) AS \"Value\"")
            .Single();
}
