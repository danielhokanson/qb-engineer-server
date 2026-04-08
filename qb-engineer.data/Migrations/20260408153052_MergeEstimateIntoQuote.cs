using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeEstimateIntoQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add new columns to quotes table
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "quotes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Quote");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "quotes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "quotes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "estimated_amount",
                table: "quotes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "assigned_to_id",
                table: "quotes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_estimate_id",
                table: "quotes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "converted_at",
                table: "quotes",
                type: "timestamp with time zone",
                nullable: true);

            // 2. Convert existing status column from integer to string
            migrationBuilder.Sql("""
                ALTER TABLE quotes ALTER COLUMN status TYPE character varying(30)
                    USING CASE status::integer
                        WHEN 0 THEN 'Draft'
                        WHEN 1 THEN 'Sent'
                        WHEN 2 THEN 'Accepted'
                        WHEN 3 THEN 'Declined'
                        WHEN 4 THEN 'Expired'
                        WHEN 5 THEN 'ConvertedToOrder'
                        ELSE 'Draft'
                    END;
                """);

            // 3. Make quote_number nullable (estimates don't have one)
            migrationBuilder.AlterColumn<string>(
                name: "quote_number",
                table: "quotes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            // 4. Set type='Quote' on all existing quote rows
            migrationBuilder.Sql("UPDATE quotes SET type = 'Quote' WHERE type = '' OR type IS NULL;");

            // 5. Migrate estimate data into quotes table
            migrationBuilder.Sql("""
                INSERT INTO quotes (
                    type, customer_id, title, description, estimated_amount,
                    expiration_date, status, notes, assigned_to_id,
                    converted_at, created_at, updated_at, deleted_at, deleted_by,
                    tax_rate, quote_number
                )
                SELECT
                    'Estimate',
                    e.customer_id,
                    e.title,
                    e.description,
                    e.estimated_amount,
                    e.valid_until,
                    CASE e.status
                        WHEN 'Draft' THEN 'Draft'
                        WHEN 'Sent' THEN 'Sent'
                        WHEN 'Accepted' THEN 'Accepted'
                        WHEN 'Declined' THEN 'Declined'
                        WHEN 'Expired' THEN 'Expired'
                        ELSE e.status
                    END,
                    e.notes,
                    e.assigned_to_id,
                    e.converted_at,
                    e.created_at,
                    e.updated_at,
                    e.deleted_at,
                    e.deleted_by,
                    0,
                    NULL
                FROM estimates e;
                """);

            // 6. Link converted estimates: set source_estimate_id on quote rows
            //    and update estimate rows to ConvertedToQuote status
            migrationBuilder.Sql("""
                UPDATE quotes q
                SET source_estimate_id = est_q.id
                FROM estimates e
                INNER JOIN quotes est_q ON est_q.type = 'Estimate'
                    AND est_q.customer_id = e.customer_id
                    AND est_q.title = e.title
                    AND est_q.created_at = e.created_at
                WHERE e.converted_to_quote_id = q.id
                    AND q.type = 'Quote';

                UPDATE quotes
                SET status = 'ConvertedToQuote'
                WHERE type = 'Estimate'
                    AND id IN (
                        SELECT source_estimate_id FROM quotes WHERE source_estimate_id IS NOT NULL
                    );
                """);

            // 7. Drop estimates table
            migrationBuilder.DropTable(name: "estimates");

            // 8. Drop old unique index on quote_number and recreate as filtered
            migrationBuilder.DropIndex(
                name: "ix_quotes_quote_number",
                table: "quotes");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_quote_number",
                table: "quotes",
                column: "quote_number",
                unique: true,
                filter: "quote_number IS NOT NULL");

            // 9. Add new indexes
            migrationBuilder.CreateIndex(
                name: "ix_quotes_assigned_to_id",
                table: "quotes",
                column: "assigned_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_source_estimate_id",
                table: "quotes",
                column: "source_estimate_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quotes_type",
                table: "quotes",
                column: "type");

            // 10. Add foreign keys
            migrationBuilder.AddForeignKey(
                name: "fk_quotes__asp_net_users_assigned_to_id",
                table: "quotes",
                column: "assigned_to_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_quotes_quotes_source_estimate_id",
                table: "quotes",
                column: "source_estimate_id",
                principalTable: "quotes",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down is destructive — estimate data cannot be fully recovered
            // This migration is not safely reversible
            throw new InvalidOperationException(
                "This migration merges estimates into quotes and cannot be reversed. " +
                "Restore from a database backup if needed.");
        }
    }
}
