﻿// <auto-generated />
using FlightBook.Persistence.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlightBook.Persistence.EFCore.Migrations
{
    [DbContext(typeof(FlightBookContext))]
    [Migration("20211031222754_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.11")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FlightBook.DomainModel.Flight", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("FlightSeatLimit")
                        .HasColumnType("int");

                    b.Property<decimal>("FlightTotalLuggageWeightLimit")
                        .HasPrecision(10, 3)
                        .HasColumnType("decimal(10,3)");

                    b.Property<int>("PerPassengerLuggageCountLimit")
                        .HasColumnType("int");

                    b.Property<decimal>("PerPassengerLuggageWeightLimit")
                        .HasPrecision(10, 3)
                        .HasColumnType("decimal(10,3)");

                    b.HasKey("ID");

                    b.ToTable("Flights");
                });

            modelBuilder.Entity("FlightBook.DomainModel.FlightRegistration", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("FlightID")
                        .HasColumnType("bigint");

                    b.Property<long>("PassengerID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("FlightID");

                    b.HasIndex("PassengerID");

                    b.ToTable("FlightRegistrations");
                });

            modelBuilder.Entity("FlightBook.DomainModel.LuggagePiece", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("FlightID")
                        .HasColumnType("bigint");

                    b.Property<long>("FlightRegistrationID")
                        .HasColumnType("bigint");

                    b.Property<decimal>("WeightInKg")
                        .HasPrecision(10, 3)
                        .HasColumnType("decimal(10,3)");

                    b.HasKey("ID");

                    b.HasIndex("FlightID");

                    b.HasIndex("FlightRegistrationID");

                    b.ToTable("LuggagePieces");
                });

            modelBuilder.Entity("FlightBook.DomainModel.Passenger", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("ID");

                    b.ToTable("Passengers");
                });

            modelBuilder.Entity("FlightBook.DomainModel.FlightRegistration", b =>
                {
                    b.HasOne("FlightBook.DomainModel.Flight", "Flight")
                        .WithMany("FlightRegistrations")
                        .HasForeignKey("FlightID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FlightBook.DomainModel.Passenger", "Passenger")
                        .WithMany("FlightRegistrations")
                        .HasForeignKey("PassengerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Flight");

                    b.Navigation("Passenger");
                });

            modelBuilder.Entity("FlightBook.DomainModel.LuggagePiece", b =>
                {
                    b.HasOne("FlightBook.DomainModel.Flight", "Flight")
                        .WithMany("LuggagePieces")
                        .HasForeignKey("FlightID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("FlightBook.DomainModel.FlightRegistration", "FlightRegistration")
                        .WithMany("LuggagePieces")
                        .HasForeignKey("FlightRegistrationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Flight");

                    b.Navigation("FlightRegistration");
                });

            modelBuilder.Entity("FlightBook.DomainModel.Flight", b =>
                {
                    b.Navigation("FlightRegistrations");

                    b.Navigation("LuggagePieces");
                });

            modelBuilder.Entity("FlightBook.DomainModel.FlightRegistration", b =>
                {
                    b.Navigation("LuggagePieces");
                });

            modelBuilder.Entity("FlightBook.DomainModel.Passenger", b =>
                {
                    b.Navigation("FlightRegistrations");
                });
#pragma warning restore 612, 618
        }
    }
}