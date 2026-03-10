using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations;

public partial class AddFullTextSearch : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add tsvector columns
        migrationBuilder.Sql("""
            ALTER TABLE jobs ADD COLUMN IF NOT EXISTS search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('english', coalesce(job_number, '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(title, '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(description, '')), 'C')
                ) STORED;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE customers ADD COLUMN IF NOT EXISTS search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('english', coalesce(name, '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(company_name, '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(email, '')), 'C')
                ) STORED;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE parts ADD COLUMN IF NOT EXISTS search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('english', coalesce(part_number, '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(description, '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(material, '')), 'C')
                ) STORED;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE leads ADD COLUMN IF NOT EXISTS search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('english', coalesce(company_name, '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(contact_name, '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(email, '')), 'C')
                ) STORED;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('english', coalesce(name, '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(serial_number, '')), 'B')
                ) STORED;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE expenses ADD COLUMN IF NOT EXISTS search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('english', coalesce(description, '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(category, '')), 'B')
                ) STORED;
            """);

        // Create GIN indexes
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_jobs_search ON jobs USING GIN (search_vector);");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_customers_search ON customers USING GIN (search_vector);");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_parts_search ON parts USING GIN (search_vector);");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_leads_search ON leads USING GIN (search_vector);");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_assets_search ON assets USING GIN (search_vector);");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_expenses_search ON expenses USING GIN (search_vector);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_jobs_search;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_customers_search;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_parts_search;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_leads_search;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_assets_search;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_expenses_search;");

        migrationBuilder.Sql("ALTER TABLE jobs DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("ALTER TABLE customers DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("ALTER TABLE parts DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("ALTER TABLE leads DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("ALTER TABLE assets DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("ALTER TABLE expenses DROP COLUMN IF EXISTS search_vector;");
    }
}
