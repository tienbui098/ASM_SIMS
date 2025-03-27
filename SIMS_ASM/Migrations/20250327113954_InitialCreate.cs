using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMS_ASM.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Majors",
                columns: table => new
                {
                    MajorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MajorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CourseStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseEndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majors", x => x.MajorID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone_number = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.CheckConstraint("CK_User_Role", "[Role] IN ('Student', 'Lecturer', 'Admin')");
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MajorID = table.Column<int>(type: "int", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.ClassID);
                    table.ForeignKey(
                        name: "FK_Classes_Majors_MajorID",
                        column: x => x.MajorID,
                        principalTable: "Majors",
                        principalColumn: "MajorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MajorID = table.Column<int>(type: "int", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CourseStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseEndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseID);
                    table.ForeignKey(
                        name: "FK_Courses_Majors_MajorID",
                        column: x => x.MajorID,
                        principalTable: "Majors",
                        principalColumn: "MajorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentClasses",
                columns: table => new
                {
                    StudentClassID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ClassID1 = table.Column<int>(type: "int", nullable: true),
                    UserID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClasses", x => x.StudentClassID);
                    table.ForeignKey(
                        name: "FK_StudentClasses_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentClasses_Classes_ClassID1",
                        column: x => x.ClassID1,
                        principalTable: "Classes",
                        principalColumn: "ClassID");
                    table.ForeignKey(
                        name: "FK_StudentClasses_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentClasses_Users_UserID1",
                        column: x => x.UserID1,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "ClassCourseFaculties",
                columns: table => new
                {
                    ClassCourseFacultyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CourseID = table.Column<int>(type: "int", nullable: false),
                    ClassID1 = table.Column<int>(type: "int", nullable: true),
                    CourseID1 = table.Column<int>(type: "int", nullable: true),
                    UserID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassCourseFaculties", x => x.ClassCourseFacultyID);
                    table.ForeignKey(
                        name: "FK_ClassCourseFaculties_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassCourseFaculties_Classes_ClassID1",
                        column: x => x.ClassID1,
                        principalTable: "Classes",
                        principalColumn: "ClassID");
                    table.ForeignKey(
                        name: "FK_ClassCourseFaculties_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassCourseFaculties_Courses_CourseID1",
                        column: x => x.CourseID1,
                        principalTable: "Courses",
                        principalColumn: "CourseID");
                    table.ForeignKey(
                        name: "FK_ClassCourseFaculties_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassCourseFaculties_Users_UserID1",
                        column: x => x.UserID1,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    EnrollmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ClassCourseFacultyID = table.Column<int>(type: "int", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClassCourseFacultyID1 = table.Column<int>(type: "int", nullable: true),
                    UserID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.EnrollmentID);
                    table.ForeignKey(
                        name: "FK_Enrollments_ClassCourseFaculties_ClassCourseFacultyID",
                        column: x => x.ClassCourseFacultyID,
                        principalTable: "ClassCourseFaculties",
                        principalColumn: "ClassCourseFacultyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_ClassCourseFaculties_ClassCourseFacultyID1",
                        column: x => x.ClassCourseFacultyID1,
                        principalTable: "ClassCourseFaculties",
                        principalColumn: "ClassCourseFacultyID");
                    table.ForeignKey(
                        name: "FK_Enrollments_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Users_UserID1",
                        column: x => x.UserID1,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    GradeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentID = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EnrollmentID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.GradeID);
                    table.ForeignKey(
                        name: "FK_Grades_Enrollments_EnrollmentID",
                        column: x => x.EnrollmentID,
                        principalTable: "Enrollments",
                        principalColumn: "EnrollmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Grades_Enrollments_EnrollmentID1",
                        column: x => x.EnrollmentID1,
                        principalTable: "Enrollments",
                        principalColumn: "EnrollmentID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassCourseFaculties_ClassID",
                table: "ClassCourseFaculties",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCourseFaculties_ClassID1",
                table: "ClassCourseFaculties",
                column: "ClassID1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCourseFaculties_CourseID",
                table: "ClassCourseFaculties",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCourseFaculties_CourseID1",
                table: "ClassCourseFaculties",
                column: "CourseID1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCourseFaculties_UserID",
                table: "ClassCourseFaculties",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCourseFaculties_UserID1",
                table: "ClassCourseFaculties",
                column: "UserID1");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_MajorID",
                table: "Classes",
                column: "MajorID");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_MajorID",
                table: "Courses",
                column: "MajorID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ClassCourseFacultyID",
                table: "Enrollments",
                column: "ClassCourseFacultyID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ClassCourseFacultyID1",
                table: "Enrollments",
                column: "ClassCourseFacultyID1");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_UserID",
                table: "Enrollments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_UserID1",
                table: "Enrollments",
                column: "UserID1");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_EnrollmentID",
                table: "Grades",
                column: "EnrollmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_EnrollmentID1",
                table: "Grades",
                column: "EnrollmentID1");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_ClassID",
                table: "StudentClasses",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_ClassID1",
                table: "StudentClasses",
                column: "ClassID1");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_UserID",
                table: "StudentClasses",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_UserID1",
                table: "StudentClasses",
                column: "UserID1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "StudentClasses");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "ClassCourseFaculties");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Majors");
        }
    }
}
