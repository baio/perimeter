﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PRR.Data.Entities;

namespace PRR.Data.DataContext
{
    public class DbDataContext : DbContext
    {
        public DbDataContext()
        {
        }

        public DbDataContext(DbContextOptions<DbDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Tenant> Tenants { get; set; }

        public virtual DbSet<DomainPool> DomainPools { get; set; }

        public virtual DbSet<Domain> Domains { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Application> Applications { get; set; }

        public virtual DbSet<Api> Apis { get; set; }

        public virtual DbSet<Permission> Permissions { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<RolePermission> RolesPermissions { get; set; }

        public virtual DbSet<DomainUserRole> DomainUserRole { get; set; }

        public virtual DbSet<SocialConnection> SocialConnections { get; set; }

        public virtual DbSet<SocialIdentity> SocialIdentities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasIndex(x => new {x.Name}).IsUnique();
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<DomainPool>(entity =>
            {
                entity.HasIndex(x => new {x.TenantId, x.Name}).IsUnique();
                entity.HasIndex(x => new {x.TenantId, x.Identifier}).IsUnique();
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.HasOne(x => x.Tenant).WithMany(x => x.DomainPools).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Domain>(entity =>
            {
                entity.HasIndex(x => new {x.PoolId, x.EnvName}).IsUnique();
                entity.Property(x => x.IsMain).HasDefaultValue(false);
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.HasOne(x => x.Pool).WithMany(x => x.Domains).OnDelete(DeleteBehavior.Cascade);

                entity.Property(d => d.SigningAlgorithm).HasConversion<string>();
            });

            var enumArrayConverter = new ValueConverter<GrantType[], string>(
                v => string.Join(',', v.Select(x => Enum.GetName(typeof(GrantType), x))),
                v =>
                    v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => (GrantType) Enum.Parse(typeof(GrantType), x)).ToArray());

            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasIndex(x => new {x.DomainId, x.Name}).IsUnique();
                entity.HasIndex(x => x.ClientId).IsUnique();
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.Property(x => x.SSOEnabled).HasDefaultValue(false);
                entity.Property(x => x.IsDomainManagement).HasDefaultValue(false);
                entity.Property(x => x.GrantTypes)
                    .HasDefaultValue(new[] {GrantType.AuthorizationCodePKCE, GrantType.RefreshToken});
                entity.HasOne(x => x.Domain).WithMany(x => x.Applications).OnDelete(DeleteBehavior.Cascade);
                entity.Property(d => d.GrantTypes)
                    .HasConversion(enumArrayConverter);
            });

            modelBuilder.Entity<Api>(entity =>
            {
                entity.HasIndex(x => new {x.DomainId, x.Name}).IsUnique();
                entity.HasIndex(x => x.Identifier).IsUnique();
                entity.Property(x => x.IsDomainManagement).HasDefaultValue(false);
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.HasOne(x => x.Domain).WithMany(x => x.Apis).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(x =>
                    new
                    {
                        x.Name, x.DomainId
                    }).IsUnique();

                entity.Property(x => x.IsDomainManagement).HasDefaultValue(false);
                entity.Property(x => x.IsTenantManagement).HasDefaultValue(false);
                entity.Property(x => x.IsDefault).HasDefaultValue(false);
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");

                entity.HasData(Seed.Roles.GetAll());
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(x =>
                    new
                    {
                        x.Name, x.ApiId
                    }).IsUnique();

                entity.Property(x => x.IsDomainManagement).HasDefaultValue(false);
                entity.Property(x => x.IsTenantManagement).HasDefaultValue(false);
                entity.Property(x => x.IsCompanyFriendly).HasDefaultValue(false);
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.Property(x => x.IsDefault).HasDefaultValue(false);

                entity.HasData(Seed.Permissions.GetAll());
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(x => new {x.RoleId, x.PermissionId});
                entity.HasOne(x => x.Role).WithMany(x => x.RolesPermissions).HasForeignKey(x => x.RoleId);
                entity.HasOne(x => x.Permission).WithMany(x => x.RolesPermissions).HasForeignKey(x => x.PermissionId);

                entity.HasData(Seed.RolesPermissions.GetAll());
            });

            modelBuilder.Entity<DomainUserRole>(entity =>
            {
                entity.HasKey(x => new {x.RoleId, x.DomainId, x.UserEmail});
                entity.HasOne(x => x.Role).WithMany(x => x.DomainUsersRoles).HasForeignKey(x => x.RoleId);
                entity.HasOne(x => x.Domain).WithMany(x => x.DomainUsersRoles).HasForeignKey(x => x.DomainId);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(x => x.Email).IsUnique();
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<SocialConnection>(entity =>
            {
                entity.HasKey(x => new {x.DomainId, x.SocialName});
                entity.HasOne(x => x.Domain).WithMany(x => x.SocialConnections).HasForeignKey(x => x.DomainId);
            });

            modelBuilder.Entity<SocialIdentity>(entity =>
            {
                entity.HasKey(x => new {x.SocialId, x.SocialName});
                entity.HasIndex(x => new {x.Email, x.SocialName}).IsUnique();
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.HasOne(x => x.User).WithMany(x => x.SocialIdentities).HasForeignKey(x => x.UserId);
            });
        }
    }
}