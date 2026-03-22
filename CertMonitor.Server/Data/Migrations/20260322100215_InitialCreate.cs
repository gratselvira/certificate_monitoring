using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CertMonitor.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workstations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceUid = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Hostname = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workstations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkstationId = table.Column<int>(type: "integer", nullable: false),
                    AgentVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastScanAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TokenDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SerialNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TokenType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Model = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Pkcs11SlotId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WorkstationId = table.Column<int>(type: "integer", nullable: false),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenDevices_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScanSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionUid = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WorkstationId = table.Column<int>(type: "integer", nullable: false),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokensFoundCount = table.Column<int>(type: "integer", nullable: false),
                    CertificatesFoundCount = table.Column<int>(type: "integer", nullable: false),
                    RawPayloadJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanSessions_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScanSessions_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Thumbprint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Issuer = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Subject = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Algorithm = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenDeviceId = table.Column<int>(type: "integer", nullable: true),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_TokenDevices_TokenDeviceId",
                        column: x => x.TokenDeviceId,
                        principalTable: "TokenDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CertificateDetections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScanSessionId = table.Column<int>(type: "integer", nullable: false),
                    CertificateId = table.Column<int>(type: "integer", nullable: false),
                    TokenDeviceId = table.Column<int>(type: "integer", nullable: true),
                    DetectedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateDetections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateDetections_Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CertificateDetections_ScanSessions_ScanSessionId",
                        column: x => x.ScanSessionId,
                        principalTable: "ScanSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificateDetections_TokenDevices_TokenDeviceId",
                        column: x => x.TokenDeviceId,
                        principalTable: "TokenDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_WorkstationId",
                table: "Agents",
                column: "WorkstationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificateDetections_CertificateId",
                table: "CertificateDetections",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateDetections_ScanSessionId_CertificateId_TokenDevi~",
                table: "CertificateDetections",
                columns: new[] { "ScanSessionId", "CertificateId", "TokenDeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificateDetections_TokenDeviceId",
                table: "CertificateDetections",
                column: "TokenDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_Issuer_SerialNumber",
                table: "Certificates",
                columns: new[] { "Issuer", "SerialNumber" },
                unique: true,
                filter: "\"Thumbprint\" IS NULL OR \"Thumbprint\" = ''");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_Thumbprint",
                table: "Certificates",
                column: "Thumbprint",
                unique: true,
                filter: "\"Thumbprint\" IS NOT NULL AND \"Thumbprint\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_TokenDeviceId",
                table: "Certificates",
                column: "TokenDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanSessions_AgentId",
                table: "ScanSessions",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanSessions_SessionUid",
                table: "ScanSessions",
                column: "SessionUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanSessions_WorkstationId",
                table: "ScanSessions",
                column: "WorkstationId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenDevices_SerialNumber",
                table: "TokenDevices",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenDevices_WorkstationId",
                table: "TokenDevices",
                column: "WorkstationId");

            migrationBuilder.CreateIndex(
                name: "IX_Workstations_DeviceUid",
                table: "Workstations",
                column: "DeviceUid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateDetections");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "ScanSessions");

            migrationBuilder.DropTable(
                name: "TokenDevices");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Workstations");
        }
    }
}
