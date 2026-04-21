using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntegradorIdeas.Migrations
{
    /// <inheritdoc />
    public partial class AddIdeaSimilarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SimilarToIdeaId",
                table: "Ideas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SimilarityPercentage",
                table: "Ideas",
                type: "REAL",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_SimilarToIdeaId",
                table: "Ideas",
                column: "SimilarToIdeaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Ideas_SimilarToIdeaId",
                table: "Ideas",
                column: "SimilarToIdeaId",
                principalTable: "Ideas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Ideas_SimilarToIdeaId",
                table: "Ideas");

            migrationBuilder.DropIndex(
                name: "IX_Ideas_SimilarToIdeaId",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "SimilarToIdeaId",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "SimilarityPercentage",
                table: "Ideas");
        }
    }
}
