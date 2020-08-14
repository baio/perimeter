﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PRR.Data.DataContext;

namespace PRR.Data.DataContextMigrations.Migrations
{
    [DbContext(typeof(DbDataContext))]
    partial class DbDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("PRR.Data.Entities.Api", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AccessTokenExpiresIn")
                        .HasColumnType("integer");

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<int>("DomainId")
                        .HasColumnType("integer");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsUserManagement")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Identifier")
                        .IsUnique();

                    b.HasIndex("DomainId", "Name")
                        .IsUnique();

                    b.ToTable("Apis");
                });

            modelBuilder.Entity("PRR.Data.Entities.Application", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AllowedCallbackUrls")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ClientSecret")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<int>("DomainId")
                        .HasColumnType("integer");

                    b.Property<string>("Flow")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("IdTokenExpiresIn")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("RefreshTokenExpiresIn")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ClientId")
                        .IsUnique();

                    b.HasIndex("DomainId", "Name")
                        .IsUnique();

                    b.ToTable("Applications");
                });

            modelBuilder.Entity("PRR.Data.Entities.Domain", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("EnvName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsMain")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<int?>("PoolId")
                        .HasColumnType("integer");

                    b.Property<int?>("TenantId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.HasIndex("PoolId", "EnvName")
                        .IsUnique();

                    b.ToTable("Domains");
                });

            modelBuilder.Entity("PRR.Data.Entities.DomainPool", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TenantId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.ToTable("DomainPools");
                });

            modelBuilder.Entity("PRR.Data.Entities.DomainUserRole", b =>
                {
                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.Property<int>("DomainId")
                        .HasColumnType("integer");

                    b.Property<string>("UserEmail")
                        .HasColumnType("text");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("RoleId", "DomainId", "UserEmail");

                    b.HasIndex("DomainId");

                    b.HasIndex("UserId");

                    b.ToTable("DomainUserRole");
                });

            modelBuilder.Entity("PRR.Data.Entities.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("ApiId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsCompanyFriendly")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsDomainManagement")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsTenantManagement")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ApiId");

                    b.HasIndex("Name", "ApiId")
                        .IsUnique();

                    b.ToTable("Permissions");

                    b.HasData(
                        new
                        {
                            Id = -50,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Allow archive tenant",
                            IsCompanyFriendly = false,
                            IsDomainManagement = false,
                            IsTenantManagement = true,
                            Name = "archive:tenant"
                        },
                        new
                        {
                            Id = -100,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Allow manage tenant admins",
                            IsCompanyFriendly = false,
                            IsDomainManagement = false,
                            IsTenantManagement = true,
                            Name = "manage:tenant-admins"
                        },
                        new
                        {
                            Id = -200,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Allow manage tenant domains",
                            IsCompanyFriendly = false,
                            IsDomainManagement = false,
                            IsTenantManagement = true,
                            Name = "manage:tenant-domains"
                        },
                        new
                        {
                            Id = -250,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Archive domain",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "archive:domain"
                        },
                        new
                        {
                            Id = -300,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Manage domain properties",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "manage:domain"
                        },
                        new
                        {
                            Id = -400,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Allow manage domain super admins",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "manage:domain-super-admins"
                        },
                        new
                        {
                            Id = -500,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Allow manage domain admins",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "manage:domain-admins"
                        },
                        new
                        {
                            Id = -600,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Manage users except admins, super-admins and owners",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "manage:users"
                        },
                        new
                        {
                            Id = -700,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Read users",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "read:users"
                        },
                        new
                        {
                            Id = -800,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Read roles",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "read:roles"
                        },
                        new
                        {
                            Id = -900,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Manage roles",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "manage:roles"
                        },
                        new
                        {
                            Id = -1000,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Read permissions",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "read:permissions"
                        },
                        new
                        {
                            Id = -1100,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Manage permissions",
                            IsCompanyFriendly = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "manage:permissions"
                        });
                });

            modelBuilder.Entity("PRR.Data.Entities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("DomainId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDefault")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsDomainManagement")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsTenantManagement")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DomainId");

                    b.HasIndex("Name", "DomainId")
                        .IsUnique();

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = -100,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Tenant owner",
                            IsDefault = false,
                            IsDomainManagement = false,
                            IsTenantManagement = true,
                            Name = "TenantOwner"
                        },
                        new
                        {
                            Id = -200,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Tenant Super admin",
                            IsDefault = false,
                            IsDomainManagement = false,
                            IsTenantManagement = true,
                            Name = "TenantSuperAdmin"
                        },
                        new
                        {
                            Id = -300,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Tenant Admin",
                            IsDefault = false,
                            IsDomainManagement = false,
                            IsTenantManagement = true,
                            Name = "TenantAdmin"
                        },
                        new
                        {
                            Id = -400,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Domain owner",
                            IsDefault = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "DomainOwner"
                        },
                        new
                        {
                            Id = -500,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Domain Super admin",
                            IsDefault = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "DomainSuperAdmin"
                        },
                        new
                        {
                            Id = -600,
                            DateCreated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Domain Admin",
                            IsDefault = false,
                            IsDomainManagement = true,
                            IsTenantManagement = false,
                            Name = "DomainAdmin"
                        });
                });

            modelBuilder.Entity("PRR.Data.Entities.RolePermission", b =>
                {
                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.Property<int>("PermissionId")
                        .HasColumnType("integer");

                    b.HasKey("RoleId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("RolesPermissions");

                    b.HasData(
                        new
                        {
                            RoleId = -100,
                            PermissionId = -50
                        },
                        new
                        {
                            RoleId = -100,
                            PermissionId = -200
                        },
                        new
                        {
                            RoleId = -100,
                            PermissionId = -100
                        },
                        new
                        {
                            RoleId = -200,
                            PermissionId = -200
                        },
                        new
                        {
                            RoleId = -200,
                            PermissionId = -100
                        },
                        new
                        {
                            RoleId = -300,
                            PermissionId = -200
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -250
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -300
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -400
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -500
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -600
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -700
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -800
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -900
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -1000
                        },
                        new
                        {
                            RoleId = -400,
                            PermissionId = -1100
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -300
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -400
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -500
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -600
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -700
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -800
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -900
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -1000
                        },
                        new
                        {
                            RoleId = -500,
                            PermissionId = -1100
                        },
                        new
                        {
                            RoleId = -600,
                            PermissionId = -600
                        },
                        new
                        {
                            RoleId = -600,
                            PermissionId = -700
                        },
                        new
                        {
                            RoleId = -600,
                            PermissionId = -800
                        },
                        new
                        {
                            RoleId = -600,
                            PermissionId = -900
                        },
                        new
                        {
                            RoleId = -600,
                            PermissionId = -1000
                        },
                        new
                        {
                            RoleId = -600,
                            PermissionId = -1100
                        });
                });

            modelBuilder.Entity("PRR.Data.Entities.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("PRR.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PRR.Data.Entities.Api", b =>
                {
                    b.HasOne("PRR.Data.Entities.Domain", "Domain")
                        .WithMany("Apis")
                        .HasForeignKey("DomainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PRR.Data.Entities.Application", b =>
                {
                    b.HasOne("PRR.Data.Entities.Domain", "Domain")
                        .WithMany("Applications")
                        .HasForeignKey("DomainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PRR.Data.Entities.Domain", b =>
                {
                    b.HasOne("PRR.Data.Entities.DomainPool", "Pool")
                        .WithMany("Domains")
                        .HasForeignKey("PoolId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PRR.Data.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId");
                });

            modelBuilder.Entity("PRR.Data.Entities.DomainPool", b =>
                {
                    b.HasOne("PRR.Data.Entities.Tenant", "Tenant")
                        .WithMany("DomainPools")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PRR.Data.Entities.DomainUserRole", b =>
                {
                    b.HasOne("PRR.Data.Entities.Domain", "Domain")
                        .WithMany("DomainUsersRoles")
                        .HasForeignKey("DomainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PRR.Data.Entities.Role", "Role")
                        .WithMany("DomainUsersRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PRR.Data.Entities.User", "User")
                        .WithMany("DomainUsersRoles")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("PRR.Data.Entities.Permission", b =>
                {
                    b.HasOne("PRR.Data.Entities.Api", "Api")
                        .WithMany("Permissions")
                        .HasForeignKey("ApiId");
                });

            modelBuilder.Entity("PRR.Data.Entities.Role", b =>
                {
                    b.HasOne("PRR.Data.Entities.Domain", "Domain")
                        .WithMany("Roles")
                        .HasForeignKey("DomainId");
                });

            modelBuilder.Entity("PRR.Data.Entities.RolePermission", b =>
                {
                    b.HasOne("PRR.Data.Entities.Permission", "Permission")
                        .WithMany("RolesPermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PRR.Data.Entities.Role", "Role")
                        .WithMany("RolesPermissions")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PRR.Data.Entities.Tenant", b =>
                {
                    b.HasOne("PRR.Data.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
