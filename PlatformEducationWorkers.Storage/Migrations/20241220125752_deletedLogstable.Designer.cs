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
    [Migration("20241220125752_deletedLogstable")]
    partial class deletedLogstable
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

            modelBuilder.Entity("CoursesJobTitle", b =>
                {
                    b.Property<int>("AccessRolesId")
                        .HasColumnType("int");

                    b.Property<int>("CoursesId")
                        .HasColumnType("int");

                    b.HasKey("AccessRolesId", "CoursesId");

                    b.HasIndex("CoursesId");

                    b.ToTable("CoursesJobTitle");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Courses", b =>
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

                    b.Property<bool>("ShowContextPassage")
                        .HasColumnType("bit");

                    b.Property<bool>("ShowCorrectAnswers")
                        .HasColumnType("bit");

                    b.Property<bool>("ShowSelectedAnswers")
                        .HasColumnType("bit");

                    b.Property<string>("TitleCource")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EnterpriseId");

                    b.ToTable("Cources");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Enterprise", b =>
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

                    b.Property<string>("PasswordEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

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

                    b.Property<string>("ProfileAvatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

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

                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateCompilation")
                        .HasColumnType("datetime2");

                    b.Property<int>("MaxRating")
                        .HasColumnType("int");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("answerJson")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("UserId");

                    b.ToTable("UserResults");
                });

            modelBuilder.Entity("CoursesJobTitle", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.JobTitle", null)
                        .WithMany()
                        .HasForeignKey("AccessRolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PlatformEducationWorkers.Core.Models.Courses", null)
                        .WithMany()
                        .HasForeignKey("CoursesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Courses", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.Enterprise", "Enterprise")
                        .WithMany("Courses")
                        .HasForeignKey("EnterpriseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Enterprise");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Enterprise", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.JobTitle", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.Enterprise", "Enterprise")
                        .WithMany()
                        .HasForeignKey("EnterpriseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Enterprise");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.User", b =>
                {
                    b.HasOne("PlatformEducationWorkers.Core.Models.Enterprise", "Enterprise")
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
                    b.HasOne("PlatformEducationWorkers.Core.Models.Courses", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PlatformEducationWorkers.Core.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PlatformEducationWorkers.Core.Models.Enterprise", b =>
                {
                    b.Navigation("Courses");

                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
