using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_app_data.Migrations
{
    /// <inheritdoc />
    public partial class updated_foreign_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMovies_Movies_TheMovieDbId",
                table: "UserMovies");

            migrationBuilder.DropIndex(
                name: "IX_UserMovies_TheMovieDbId",
                table: "UserMovies");

            migrationBuilder.DropColumn(
                name: "TheMovieDbId",
                table: "UserMovies");

            migrationBuilder.CreateIndex(
                name: "IX_UserMovies_MovieId",
                table: "UserMovies",
                column: "MovieId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMovies_Movies_MovieId",
                table: "UserMovies",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "MovieId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMovies_Movies_MovieId",
                table: "UserMovies");

            migrationBuilder.DropIndex(
                name: "IX_UserMovies_MovieId",
                table: "UserMovies");

            migrationBuilder.AddColumn<int>(
                name: "TheMovieDbId",
                table: "UserMovies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserMovies_TheMovieDbId",
                table: "UserMovies",
                column: "TheMovieDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMovies_Movies_TheMovieDbId",
                table: "UserMovies",
                column: "TheMovieDbId",
                principalTable: "Movies",
                principalColumn: "MovieId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
