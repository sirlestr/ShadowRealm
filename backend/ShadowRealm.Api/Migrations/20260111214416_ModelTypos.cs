using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShadowRealm.Api.Migrations
{
    /// <inheritdoc />
    public partial class ModelTypos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RevardXP",
                table: "Quests",
                newName: "RewardXP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RewardXP",
                table: "Quests",
                newName: "RevardXP");
        }
    }
}
