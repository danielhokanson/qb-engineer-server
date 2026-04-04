using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QBEngineer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a sequence starting after the highest existing job number.
            // The sequence is atomic — concurrent calls to nextval() never collide.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    max_num int;
                BEGIN
                    SELECT COALESCE(MAX(CAST(SUBSTRING(job_number FROM 3) AS int)), 1000)
                      INTO max_num
                      FROM jobs
                     WHERE job_number LIKE 'J-%'
                       AND SUBSTRING(job_number FROM 3) ~ '^\d+$';

                    EXECUTE format('CREATE SEQUENCE job_number_seq START WITH %s INCREMENT BY 1', max_num + 1);
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS job_number_seq;");
        }
    }
}
