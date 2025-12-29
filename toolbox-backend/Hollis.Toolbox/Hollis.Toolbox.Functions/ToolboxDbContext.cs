using Hollis.Toolbox.Functions.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hollis.Toolbox.Functions;

public class ToolboxDbContext : DbContext
{
    public DbSet<PastebinItem> PastebinItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var schemaName = nameof(ToolboxDbContext).Replace(nameof(DbContext), string.Empty);
        modelBuilder.HasDefaultSchema(schemaName);

        base.OnModelCreating(modelBuilder);
    }
}
