﻿using Microsoft.EntityFrameworkCore;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity => { entity.Property(x => x.DateCreated).HasDefaultValueSql("now()"); });

            modelBuilder.Entity<DomainPool>(entity =>
            {
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.HasOne(x => x.Tenant).WithMany(x => x.DomainPools).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Domain>(entity =>
            {
                entity.Property(x => x.IsMain).HasDefaultValue(false);
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.HasOne(x => x.Pool).WithMany(x => x.Domains).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasIndex(x => x.ClientId).IsUnique();
                entity.Property(x => x.DateCreated).HasDefaultValueSql("now()");
                entity.HasOne(x => x.Domain).WithMany(x => x.Applications).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Api>(entity =>
            {
                entity.HasIndex(x => x.Identifier).IsUnique();
                entity.Property(x => x.IsUserManagement).HasDefaultValue(false);
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
        }
    }
}