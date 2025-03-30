using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMS_ASM.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentClasses_Classes_ClassID1",
                table: "StudentClasses");

            migrationBuilder.DropIndex(
                name: "IX_StudentClasses_ClassID1",
                table: "StudentClasses");

            migrationBuilder.DropColumn(
                name: "ClassID1",
                table: "StudentClasses");

            migrationBuilder.DropColumn(
                name: "CourseEndDate",
                table: "Majors");

            migrationBuilder.DropColumn(
                name: "CourseStartDate",
                table: "Majors");

            migrationBuilder.DropColumn(
                name: "CourseEndDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CourseStartDate",
                table: "Courses");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date_of_birth",
                table: "Users",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date_of_birth",
                table: "Users",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<int>(
                name: "ClassID1",
                table: "StudentClasses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CourseEndDate",
                table: "Majors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CourseStartDate",
                table: "Majors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CourseEndDate",
                table: "Courses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CourseStartDate",
                table: "Courses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_ClassID1",
                table: "StudentClasses",
                column: "ClassID1");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClasses_Classes_ClassID1",
                table: "StudentClasses",
                column: "ClassID1",
                principalTable: "Classes",
                principalColumn: "ClassID");
        }
    }
}
