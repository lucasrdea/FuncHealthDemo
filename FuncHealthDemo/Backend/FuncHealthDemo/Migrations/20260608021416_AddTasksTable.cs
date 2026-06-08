using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FuncHealthDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "DoctorWarnings");

            migrationBuilder.DropTable(
                name: "LabExams");

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId",
                table: "Tasks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.CreateTable(
                name: "DoctorWarnings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IssuedBy = table.Column<string>(type: "TEXT", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorWarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorWarnings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabExams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LabExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DoctorName = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_LabExams_LabExamId",
                        column: x => x.LabExamId,
                        principalTable: "LabExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DoctorWarnings",
                columns: new[] { "Id", "IssuedBy", "IssuedDate", "Message", "Severity", "UserId" },
                values: new object[,]
                {
                    { 1, "Dr. Michael Chen", new DateTime(2024, 12, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Your blood pressure readings have been slightly elevated. Please monitor daily and reduce sodium intake.", "Medium", 1 },
                    { 2, "Dr. Robert Martinez", new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cholesterol levels are above recommended range. Schedule a follow-up in 3 months and start prescribed medication.", "High", 2 }
                });

            migrationBuilder.InsertData(
                table: "LabExams",
                columns: new[] { "Id", "Category", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Hematology", "Measures different components of blood including red cells, white cells, and platelets", "Complete Blood Count (CBC)" },
                    { 2, "Cardiology", "Tests cholesterol levels including HDL, LDL, and triglycerides", "Lipid Panel" },
                    { 3, "General", "Evaluates kidney and liver function, electrolytes, and blood sugar", "Comprehensive Metabolic Panel" },
                    { 4, "Endocrinology", "Measures thyroid-stimulating hormone to assess thyroid health", "Thyroid Function Test (TSH)" },
                    { 5, "General", "Analyzes urine for various substances to detect infections and diseases", "Urinalysis" },
                    { 6, "Diabetes", "Measures average blood sugar levels over the past 2-3 months", "HbA1c (Glycated Hemoglobin)" },
                    { 7, "Hepatology", "Evaluates liver enzymes and proteins to assess liver health", "Liver Function Test (LFT)" },
                    { 8, "Nutrition", "Measures vitamin D levels in the blood", "Vitamin D Test" },
                    { 9, "Urology", "Screens for prostate cancer and monitors prostate health", "Prostate-Specific Antigen (PSA)" },
                    { 10, "Infectious Disease", "Detects active coronavirus infection", "COVID-19 PCR Test" }
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AppointmentDate", "DoctorName", "LabExamId", "Location", "Notes", "Status", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 4, 10, 0, 0, 0, DateTimeKind.Unspecified), "Dr. Michael Chen", 1, "City Medical Center, Room 205", "Bring previous lab results", 0, 1 },
                    { 2, new DateTime(2025, 1, 8, 14, 30, 0, 0, DateTimeKind.Unspecified), "Dr. Sarah Johnson", 2, "Bright Smile Dental Clinic", "Regular 6-month cleaning", 2, 1 },
                    { 3, new DateTime(2025, 1, 6, 9, 15, 0, 0, DateTimeKind.Unspecified), "Dr. Robert Martinez", 3, "Heart Health Institute, 3rd Floor", "Review EKG results from last visit", 0, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_LabExamId",
                table: "Appointments",
                column: "LabExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UserId",
                table: "Appointments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorWarnings_UserId",
                table: "DoctorWarnings",
                column: "UserId");
        }
    }
}
