using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_app_data.Migrations
{
    /// <inheritdoc />
    public partial class fixed_TheMovieDbId_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TheMovieDb",
                table: "Movies",
                newName: "TheMovieDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TheMovieDbId",
                table: "Movies",
                newName: "TheMovieDb");
        }
    }
}
