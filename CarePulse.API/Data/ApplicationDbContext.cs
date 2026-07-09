using CarePulse.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.API.Data;

public class ApplicationDbContext
    : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext>
        options)
        : base(options)
    {
    }

    public DbSet<User> Users
    {
        get;
        set;
    }

    public DbSet<RefreshToken>
        RefreshTokens
    {
        get;
        set;
    }

    public DbSet<PasswordResetToken>
        PasswordResetTokens
    {
        get;
        set;
    }

    public DbSet<PasswordResetOtp>
        PasswordResetOtps
    {
        get;
        set;
    }

    public DbSet<Appointment>
        Appointments
    {
        get;
        set;
    }

    public DbSet<DoctorAvailabilitySlot>
        DoctorAvailabilitySlots
    {
        get;
        set;
    }

    public DbSet<Prescription>
        Prescriptions
    {
        get;
        set;
    }

    public DbSet<DietPlan>
        DietPlans
    {
        get;
        set;
    }

    public DbSet<MenuItem>
        MenuItems
    {
        get;
        set;
    }

    public DbSet<MealOrder>
        MealOrders
    {
        get;
        set;
    }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override
        void OnModelCreating(
            ModelBuilder modelBuilder)
    {
        base.OnModelCreating(
            modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId)
                .HasColumnName("UserId")
                .ValueGeneratedOnAdd();
            entity.Property(x => x.FullName)
                .IsRequired();
            entity.Property(x => x.Email)
                .IsRequired();
            entity.Property(x => x.PasswordHash)
                .IsRequired();
            entity.Property(x => x.Phone)
                .IsRequired();
            entity.Property(x => x.Role)
                .IsRequired();
            entity.Property(x => x.IsActive)
                .IsRequired();
            entity.Property(x => x.CreatedAt)
                .IsRequired();
            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        modelBuilder.Entity<RefreshToken>()
            .HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<PasswordResetOtp>()
            .HasOne(x => x.User)
            .WithMany(x => x.PasswordResetOtps)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DoctorAvailabilitySlot>()
            .HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DietPlan>()
            .HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DietPlan>()
            .HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MealOrder>()
            .HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MealOrder>()
            .HasOne(x => x.MenuItem)
            .WithMany()
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}