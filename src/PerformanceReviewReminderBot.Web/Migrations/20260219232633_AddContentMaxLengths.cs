using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformanceReviewReminderBot.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddContentMaxLengths : Migration
    {
        // NOTE: SQLite TEXT columns do not support enforced maximum lengths,
        // so this migration intentionally contains no schema-altering operations.
        // The HasMaxLength(4000) and HasMaxLength(2000) annotations are recorded
        // in the EF Core model snapshot for documentation purposes and would
        // generate real column constraints when targeting SQL Server or PostgreSQL.
        // Runtime length limits are enforced at the service layer instead.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
