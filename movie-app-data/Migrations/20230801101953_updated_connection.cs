using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_app_data.Migrations
{
    /// <inheritdoc />
    public partial class updated_connection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TheMovieDbId",
                table: "Movies",
                newName: "TheMovieDb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TheMovieDb",
                table: "Movies",
                newName: "TheMovieDbId");
        }
    }
}
