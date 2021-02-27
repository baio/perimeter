using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PRR.Data.DataContextMigrations.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialIdentities",
                columns: table => new
                {
                    SocialName = table.Column<string>(nullable: false),
                    SocialId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialIdentities", x => new { x.SocialId, x.SocialName });
                    table.ForeignKey(
                        name: "FK_SocialIdentities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DomainPools",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    Identifier = table.Column<string>(nullable: false),
                    TenantId = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainPools_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnvName = table.Column<string>(nullable: false),
                    PoolId = table.Column<int>(nullable: true),
                    TenantId = table.Column<int>(nullable: true),
                    IsMain = table.Column<bool>(nullable: false, defaultValue: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    Issuer = table.Column<string>(nullable: false),
                    AccessTokenExpiresIn = table.Column<int>(nullable: false),
                    SigningAlgorithm = table.Column<string>(nullable: false),
                    HS256SigningSecret = table.Column<string>(nullable: true),
                    RS256Params = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Domains_DomainPools_PoolId",
                        column: x => x.PoolId,
                        principalTable: "DomainPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Domains_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Apis",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    DomainId = table.Column<int>(nullable: false),
                    Identifier = table.Column<string>(nullable: false),
                    IsDomainManagement = table.Column<bool>(nullable: false, defaultValue: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apis_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    DomainId = table.Column<int>(nullable: false),
                    ClientId = table.Column<string>(nullable: false),
                    ClientSecret = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    IdTokenExpiresIn = table.Column<int>(nullable: false),
                    RefreshTokenExpiresIn = table.Column<int>(nullable: false),
                    AllowedCallbackUrls = table.Column<string>(nullable: false),
                    AllowedLogoutCallbackUrls = table.Column<string>(nullable: false),
                    SSOEnabled = table.Column<bool>(nullable: false, defaultValue: false),
                    GrantTypes = table.Column<string>(nullable: false, defaultValue: "AuthorizationCodePKCE,RefreshToken"),
                    IsDomainManagement = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    IsTenantManagement = table.Column<bool>(nullable: false, defaultValue: false),
                    IsDomainManagement = table.Column<bool>(nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(nullable: false, defaultValue: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    DomainId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SocialConnections",
                columns: table => new
                {
                    DomainId = table.Column<int>(nullable: false),
                    SocialName = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: true),
                    ClientSecret = table.Column<string>(nullable: true),
                    Attributes = table.Column<string[]>(nullable: true),
                    Permissions = table.Column<string[]>(nullable: true),
                    Order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialConnections", x => new { x.DomainId, x.SocialName });
                    table.ForeignKey(
                        name: "FK_SocialConnections_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    IsTenantManagement = table.Column<bool>(nullable: false, defaultValue: false),
                    IsDomainManagement = table.Column<bool>(nullable: false, defaultValue: false),
                    IsCompanyFriendly = table.Column<bool>(nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(nullable: false, defaultValue: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    ApiId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Apis_ApiId",
                        column: x => x.ApiId,
                        principalTable: "Apis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DomainUserRole",
                columns: table => new
                {
                    DomainId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false),
                    UserEmail = table.Column<string>(nullable: false),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainUserRole", x => new { x.RoleId, x.DomainId, x.UserEmail });
                    table.ForeignKey(
                        name: "FK_DomainUserRole_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainUserRole_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainUserRole_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolesPermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(nullable: false),
                    PermissionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolesPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesPermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ApiId", "Description", "IsTenantManagement", "Name" },
                values: new object[] { -50, null, "Allow archive tenant", true, "archive:tenant" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ApiId", "Description", "IsDomainManagement", "Name" },
                values: new object[,]
                {
                    { -1100, null, "Manage permissions", true, "manage:permissions" },
                    { -1000, null, "Read permissions", true, "read:permissions" },
                    { -900, null, "Manage roles", true, "manage:roles" },
                    { -700, null, "Read users", true, "read:users" },
                    { -600, null, "Manage users except admins, super-admins and owners", true, "manage:users" },
                    { -800, null, "Read roles", true, "read:roles" },
                    { -400, null, "Allow manage domain super admins", true, "manage:domain-super-admins" },
                    { -300, null, "Manage domain properties", true, "manage:domain" },
                    { -250, null, "Archive domain", true, "archive:domain" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ApiId", "Description", "IsTenantManagement", "Name" },
                values: new object[,]
                {
                    { -200, null, "Allow manage tenant domains", true, "manage:tenant-domains" },
                    { -100, null, "Allow manage tenant admins", true, "manage:tenant-admins" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ApiId", "Description", "IsDomainManagement", "Name" },
                values: new object[] { -500, null, "Allow manage domain admins", true, "manage:domain-admins" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "DomainId", "IsDomainManagement", "Name" },
                values: new object[] { -500, "Domain Super admin", null, true, "DomainSuperAdmin" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "DomainId", "IsTenantManagement", "Name" },
                values: new object[,]
                {
                    { -100, "Tenant owner", null, true, "TenantOwner" },
                    { -200, "Tenant Super admin", null, true, "TenantSuperAdmin" },
                    { -300, "Tenant Admin", null, true, "TenantAdmin" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "DomainId", "IsDomainManagement", "Name" },
                values: new object[,]
                {
                    { -400, "Domain owner", null, true, "DomainOwner" },
                    { -600, "Domain Admin", null, true, "DomainAdmin" }
                });

            migrationBuilder.InsertData(
                table: "RolesPermissions",
                columns: new[] { "RoleId", "PermissionId" },
                values: new object[,]
                {
                    { -100, -50 },
                    { -600, -900 },
                    { -600, -800 },
                    { -600, -700 },
                    { -600, -600 },
                    { -600, -300 },
                    { -500, -1100 },
                    { -500, -1000 },
                    { -500, -900 },
                    { -500, -800 },
                    { -500, -700 },
                    { -500, -600 },
                    { -500, -500 },
                    { -500, -400 },
                    { -500, -300 },
                    { -400, -1100 },
                    { -400, -1000 },
                    { -400, -900 },
                    { -400, -800 },
                    { -400, -700 },
                    { -400, -600 },
                    { -400, -500 },
                    { -400, -400 },
                    { -400, -300 },
                    { -400, -250 },
                    { -300, -200 },
                    { -200, -100 },
                    { -200, -200 },
                    { -100, -100 },
                    { -100, -200 },
                    { -600, -1000 },
                    { -600, -1100 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apis_Identifier",
                table: "Apis",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Apis_DomainId_Name",
                table: "Apis",
                columns: new[] { "DomainId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ClientId",
                table: "Applications",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_DomainId_Name",
                table: "Applications",
                columns: new[] { "DomainId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainPools_TenantId_Identifier",
                table: "DomainPools",
                columns: new[] { "TenantId", "Identifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainPools_TenantId_Name",
                table: "DomainPools",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Domains_TenantId",
                table: "Domains",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_PoolId_EnvName",
                table: "Domains",
                columns: new[] { "PoolId", "EnvName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainUserRole_DomainId",
                table: "DomainUserRole",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainUserRole_UserId",
                table: "DomainUserRole",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ApiId",
                table: "Permissions",
                column: "ApiId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name_ApiId",
                table: "Permissions",
                columns: new[] { "Name", "ApiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_DomainId",
                table: "Roles",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name_DomainId",
                table: "Roles",
                columns: new[] { "Name", "DomainId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermissions_PermissionId",
                table: "RolesPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialIdentities_UserId",
                table: "SocialIdentities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialIdentities_Email_SocialName",
                table: "SocialIdentities",
                columns: new[] { "Email", "SocialName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_UserId",
                table: "Tenants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "DomainUserRole");

            migrationBuilder.DropTable(
                name: "RolesPermissions");

            migrationBuilder.DropTable(
                name: "SocialConnections");

            migrationBuilder.DropTable(
                name: "SocialIdentities");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Apis");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.DropTable(
                name: "DomainPools");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
