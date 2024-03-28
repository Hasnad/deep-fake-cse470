using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeepfakeWeb.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ImageData_ImageDataId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ImageDataId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ImageDataId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "ImageData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageData_ApplicationUserId",
                table: "ImageData",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageData_AspNetUsers_ApplicationUserId",
                table: "ImageData",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageData_AspNetUsers_ApplicationUserId",
                table: "ImageData");

            migrationBuilder.DropIndex(
                name: "IX_ImageData_ApplicationUserId",
                table: "ImageData");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ImageData");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageDataId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ImageDataId",
                table: "AspNetUsers",
                column: "ImageDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ImageData_ImageDataId",
                table: "AspNetUsers",
                column: "ImageDataId",
                principalTable: "ImageData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
