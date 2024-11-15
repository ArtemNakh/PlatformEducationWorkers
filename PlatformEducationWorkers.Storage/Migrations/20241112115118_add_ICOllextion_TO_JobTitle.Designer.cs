﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlatformEducationWorkers.Storage;

#nullable disable

namespace PlatformEducationWorkers.Storage.Migrations
{
    [DbContext(typeof(PlatformEducationContex))]
    [Migration("20241112115118_add_ICOllextion_TO_JobTitle")]
    partial class add_ICOllextion_TO_JobTitle
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CourcesJobTitle", b =>
                {
                    b.Property<int>("AccessRolesId")
                        .HasColumnType("int");

                    b.Property<int>("CoursesId")
                        .HasColumnType("int");

                    b.HasKey("AccessRolesId", "CoursesId");

                    b.HasIndex("CoursesId");

                    b.ToTable("CourcesJobTitle");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Cources", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ContentCourse")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateCreate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EnterpriseId")
                        .HasColumnType("int");

                    b.Property<string>("Questions")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TitleCource")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EnterpriseId");

                    b.ToTable("Cources");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Enterprice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateCreate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Enterprises");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.JobTitle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("EnterpriseId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EnterpriseId");

                    b.ToTable("JobTitles");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Birthday")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateCreate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EnterpriseId")
                        .HasColumnType("int");

                    b.Property<int>("JobTitleId")
                        .HasColumnType("int");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EnterpriseId");

                    b.HasIndex("JobTitleId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.UserResults", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CourceId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateCompilation")
                        .HasColumnType("datetime2");

                    b.Property<int>("MaxRating")
                        .HasColumnType("int");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CourceId");

                    b.HasIndex("UserId");

                    b.ToTable("UserResults");
                });

            modelBuilder.Entity("CourcesJobTitle", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.JobTitle", null)
                        .WithMany()
                        .HasForeignKey("AccessRolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PlatformEducationWorkers.Core.Models.Cources", null)
                        .WithMany()
                        .HasForeignKey("CoursesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Cources", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.Enterprice", "Enterprise")
                        .WithMany("Cources")
                        .HasForeignKey("EnterpriseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Enterprise");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Enterprice", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.JobTitle", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.Enterprice", "Enterprise")
                        .WithMany()
                        .HasForeignKey("EnterpriseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Enterprise");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.User", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.Enterprice", "Enterprise")
                        .WithMany("Users")
                        .HasForeignKey("EnterpriseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PlatformEducationWorkers.Core.Models.JobTitle", "JobTitle")
                        .WithMany()
                        .HasForeignKey("JobTitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Enterprise");

                    b.Navigation("JobTitle");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.UserResults", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.Cources", "Cource")
                        .WithMany()
                        .HasForeignKey("CourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PlatformEducationWorkers.Core.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cource");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Enterprice", b =>
                {
                    b.Navigation("Cources");

                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
