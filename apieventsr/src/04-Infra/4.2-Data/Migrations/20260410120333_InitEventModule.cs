using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apieventsr.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitEventModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DomainEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MyProperty = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BannerUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EnrollmentStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnrollmentEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResultDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AwardDetails = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "segments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BlobName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_documents_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_segments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_segments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_segments_segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_enrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResponsibleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ManagementRepresentative = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_enrollments_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_event_enrollments_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_enrollments_segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StorageName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BlobName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    EventEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_enrollment_files_event_enrollments_EventEnrollmentId",
                        column: x => x.EventEnrollmentId,
                        principalTable: "event_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_SegmentId",
                table: "categories",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_files_EventEnrollmentId_OriginalName",
                table: "enrollment_files",
                columns: new[] { "EventEnrollmentId", "OriginalName" },
                unique: true,
                filter: "\"DeleteDate\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_event_documents_EventId",
                table: "event_documents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_event_enrollments_CategoryId",
                table: "event_enrollments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_event_enrollments_EventId",
                table: "event_enrollments",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_event_enrollments_SchoolId_EventId_SegmentId_CategoryId",
                table: "event_enrollments",
                columns: new[] { "SchoolId", "EventId", "SegmentId", "CategoryId" },
                unique: true,
                filter: "\"DeleteDate\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_event_enrollments_SegmentId",
                table: "event_enrollments",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_user_segments_SegmentId",
                table: "user_segments",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_user_segments_UserId_SegmentId",
                table: "user_segments",
                columns: new[] { "UserId", "SegmentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainEntity");

            migrationBuilder.DropTable(
                name: "enrollment_files");

            migrationBuilder.DropTable(
                name: "event_documents");

            migrationBuilder.DropTable(
                name: "user_segments");

            migrationBuilder.DropTable(
                name: "event_enrollments");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "segments");
        }
    }
}
