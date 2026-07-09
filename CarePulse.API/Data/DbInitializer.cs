using CarePulse.API.Helpers;
using CarePulse.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace CarePulse.API.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (MySqlException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            // Existing database schema already contains the required tables.
            // Allow the application to continue using the existing database.
        }

        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];
        var adminFullName = configuration["SeedAdmin:FullName"] ?? "System Admin";
        var adminPhone = configuration["SeedAdmin:Phone"] ?? "+10000000001";

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRoles.Admin);
            if (adminUser is null)
            {
                adminUser = new User
                {
                    FullName = adminFullName,
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Phone = adminPhone,
                    Role = UserRoles.Admin,
                    IsActive = true
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
            else if (!BCrypt.Net.BCrypt.Verify(adminPassword, adminUser.PasswordHash))
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                adminUser.FullName = adminFullName;
                adminUser.Email = adminEmail;
                adminUser.Phone = adminPhone;
                adminUser.IsActive = true;
                context.Users.Update(adminUser);
                await context.SaveChangesAsync();
            }
        }
        else
        {
            // No admin credentials provided in configuration; skip seeding or updating the admin user.
            // This prevents hardcoded admin credentials from being used.
        }

        var patientUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRoles.Patient);
        if (patientUser is null)
        {
            patientUser = new User
            {
                FullName = "Sample Patient",
                Email = "patient@carepulse.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                Phone = "+10000000002",
                Role = UserRoles.Patient,
                IsActive = true
            };

            await context.Users.AddAsync(patientUser);
            await context.SaveChangesAsync();
        }
        else if (!BCrypt.Net.BCrypt.Verify("Patient@123", patientUser.PasswordHash))
        {
            patientUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123");
            patientUser.FullName = "Sample Patient";
            patientUser.Email = "patient@carepulse.local";
            patientUser.Phone = "+10000000002";
            patientUser.IsActive = true;
            context.Users.Update(patientUser);
            await context.SaveChangesAsync();
        }

        // Seed a test user requested by developer (nikita) if not present
        var testEmail = "nikita2402singh@gmail.com";
        var testUser = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == testEmail);
        if (testUser is null)
        {
            testUser = new User
            {
                FullName = "Nikita Singh",
                Email = testEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12341234"),
                Phone = "+91" + "6260816202",
                Role = UserRoles.Patient,
                IsActive = true
            };

            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
        }

        var doctorEmail = "doctor@carepulse.local";
        var doctorUser = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == doctorEmail);
        if (doctorUser is null)
        {
            doctorUser = new User
            {
                FullName = "Dr. Olivia Smith",
                Email = doctorEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                Phone = "+10000000003",
                Role = UserRoles.Doctor,
                DoctorCategory = "General Physician",
                DoctorQualification = "MBBS",
                DoctorSpecialty = "General Medicine",
                IsActive = true
            };

            await context.Users.AddAsync(doctorUser);
            await context.SaveChangesAsync();
        }
        else
        {
            var doctorPassword = "Doctor@123";
            if (!BCrypt.Net.BCrypt.Verify(doctorPassword, doctorUser.PasswordHash))
            {
                doctorUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(doctorPassword);
            }

            doctorUser.FullName = "Dr. Olivia Smith";
            doctorUser.Email = doctorEmail;
            doctorUser.Phone = "+10000000003";
            doctorUser.Role = UserRoles.Doctor;
            doctorUser.DoctorCategory = "General Physician";
            doctorUser.DoctorQualification = "MBBS";
            doctorUser.DoctorSpecialty = "General Medicine";
            doctorUser.IsActive = true;
            context.Users.Update(doctorUser);
            await context.SaveChangesAsync();
        }

        // Seed menu items (canteen)
        if (!await context.MenuItems.AnyAsync())
        {
            var menuItems = new List<MenuItem>
            {
                new MenuItem { Name = "Grilled Chicken Salad", Description = "Mixed greens with grilled chicken, cherry tomatoes, and vinaigrette.", Price = 199.00M, FoodCategory = "Non-Vegetarian", IsAvailable = true },
                new MenuItem { Name = "Vegetable Thali", Description = "Seasonal vegetables, dal, rice and chapati.", Price = 149.00M, FoodCategory = "Vegetarian", IsAvailable = true },
                new MenuItem { Name = "Oats Porridge", Description = "Warm oats with milk, honey and seasonal fruits.", Price = 99.00M, FoodCategory = "Diabetic", IsAvailable = true },
                new MenuItem { Name = "Paneer Wrap", Description = "Spiced paneer with salad wrapped in a tortilla.", Price = 129.00M, FoodCategory = "Vegetarian", IsAvailable = true },
                new MenuItem { Name = "Fruit Plate", Description = "Assorted fresh fruits — perfect for light diets.", Price = 89.00M, FoodCategory = "Low-Calorie", IsAvailable = true },
                new MenuItem { Name = "Baked Fish", Description = "Soft baked fish with herbs and steamed vegetables.", Price = 219.00M, FoodCategory = "Non-Vegetarian", IsAvailable = true }
            };

            await context.MenuItems.AddRangeAsync(menuItems);
            await context.SaveChangesAsync();
        }

        // Seed sample appointments (if none exist and providers are available)
        if (!await context.Appointments.AnyAsync())
        {
            var appointmentPatient = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRoles.Patient);
            var appointmentDoctor = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRoles.Doctor);
            if (appointmentPatient is not null && appointmentDoctor is not null)
            {
                var appointments = new List<Appointment>
                {
                    new Appointment { PatientId = appointmentPatient.UserId, DoctorId = appointmentDoctor.UserId, AppointmentDate = DateTime.UtcNow.AddDays(1).AddHours(10), Status = "Requested", Notes = "Initial consultation" },
                    new Appointment { PatientId = appointmentPatient.UserId, DoctorId = appointmentDoctor.UserId, AppointmentDate = DateTime.UtcNow.AddDays(3).AddHours(14), Status = "Requested", Notes = "Follow-up checkup" }
                };
                await context.Appointments.AddRangeAsync(appointments);
                await context.SaveChangesAsync();
            }
        }

        if (!await context.DietPlans.AnyAsync())
        {
            var dietPlanPatient = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRoles.Patient);
            var dieticianUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRoles.Dietician) ?? await context.Users.FirstOrDefaultAsync();

            if (dietPlanPatient is not null && dieticianUser is not null)
            {
                var plans = new List<DietPlan>
                {
                    new DietPlan { PatientId = dietPlanPatient.UserId, CreatedById = dieticianUser.UserId, DietSummary = "Balanced meals: 3 main meals + 2 snacks, 2000 kcal/day.", Goal = "Maintain healthy weight", Notes = "Include high-fiber foods and lean proteins." },
                    new DietPlan { PatientId = dietPlanPatient.UserId, CreatedById = dieticianUser.UserId, DietSummary = "Low-sodium plan for cardiac support: emphasize fresh vegetables and lean proteins.", Goal = "Reduce blood pressure", Notes = "Limit processed foods and salt." },
                    new DietPlan { PatientId = dietPlanPatient.UserId, CreatedById = dieticianUser.UserId, DietSummary = "High-protein recovery plan: add protein-rich shakes and nutrient-dense snacks.", Goal = "Support recovery and muscle maintenance", Notes = "Monitor kidney function if prolonged." }
                };
                await context.DietPlans.AddRangeAsync(plans);
                await context.SaveChangesAsync();
            }
        }
    }
}
