using Fayora.Application.Common.Abstractions.Caching;
using Fayora.Application.Common.Factories;
using Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Application.Common.Interfaces.Persistences.BookingModule;
using Fayora.Application.Common.Interfaces.Persistences.ChatModule;
using Fayora.Application.Common.Interfaces.Persistences.GuideModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Persistences.SharedModule;
using Fayora.Application.Common.Interfaces.Persistences.TouristModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Application.Common.Interfaces.Services.BookingModule;
using Fayora.Application.Common.Interfaces.Services.SharedModule;
using Fayora.Application.Common.Interfaces.Services.ChatbotModule;
using Fayora.Application.Common.Strategies;
using Fayora.Domain.Common.Interfaces.IdentityModule;
using Fayora.Infrastructure.Persistence.Caching;
using Fayora.Infrastructure.Persistence.Repositories;
using Fayora.Infrastructure.Persistence.Repositories.AccommodationModule;
using Fayora.Infrastructure.Persistence.Repositories.AdminModule;
using Fayora.Infrastructure.Persistence.Repositories.BookingModule;
using Fayora.Infrastructure.Persistence.Repositories.ChatModule;
using Fayora.Infrastructure.Persistence.Repositories.GuideModule;
using Fayora.Infrastructure.Persistence.Repositories.IdentityModule;
using Fayora.Infrastructure.Persistence.Repositories.SharedModule;
using Fayora.Infrastructure.Persistence.Repositories.TouristModule;
using Fayora.Infrastructure.Services.AdminModule;
using Fayora.Infrastructure.Services.Authentication;
using Fayora.Infrastructure.Services.AuthModule;
using Fayora.Infrastructure.Services.BookingModule;
using Fayora.Infrastructure.Services.SharedModule;
using Fayora.Infrastructure.Services.Chatbot;
using Fayora.Infrastructure.Settings;
using Fayora.Infrastructure.Strategies;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;


using Fayora.Application.Common.Interfaces.Persistences.NotificationModule;
using Fayora.Infrastructure.Persistence.Repositories.NotificationModule;
using Fayora.Application.Common.Interfaces.Services.NotificationModule;
using Fayora.Infrastructure.Services.NotificationModule;


namespace Fayora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddAuthentication(configuration)
            .AddPersistence(configuration)
            .AddService(configuration)
            .AddBackgroundJobs(configuration);
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        var redisConnectionString = configuration.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString!)
        );


        // Admin Module
        services.AddScoped<IAdminRepository, AdminRepository>();

        // Notification Module
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Identity Module
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
        services.AddScoped<IUserIdentityRepository, UserIdentityRepository>();
        services.AddScoped<IMasterInterestRepository, MasterInterestRepository>();


        services.AddScoped<ITouristRepository, TouristRepository>();
        services.AddScoped<IUserInteractionRepository, UserInteractionRepository>();
        services.AddScoped<IMessageSenderStrategy, WhatsAppSenderStrategy>();
        services.AddScoped<IMessageSenderStrategy, SmsSenderStrategy>();


        // Accommodation Module
        services.AddScoped<IHousingUnitRepository, HousingUnitRepository>();
        services.AddScoped<IUnitOwnerRepository, UnitOwnerRepository>();
        services.AddScoped<IHousingUnitImageRepository, HousingUnitImageRepository>();
        services.AddScoped<IHousingUnitImageRepository, HousingUnitImageRepository>();
        services.AddScoped<ICalendarBlockRepository, CalendarBlockRepository>();

        // Tour Guide Module
        services.AddScoped<ITourGuideRepository, TourGuideRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<ITourCompanyRepository, TourCompanyRepository>();
        services.AddScoped<IPackageImageRepository, PackageImageRepository>();
        services.AddScoped<IPackageOccurrenceRepository, PackageOccurrenceRepository>();
        services.AddScoped<IGuideWeeklyScheduleRepository, GuideWeeklyScheduleRepository>();

        // Shared Module
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<ILocationImageRepository, LocationImageRepository>();

        // Chat Module
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        // Booking Module
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IQrTokenService, QrTokenService>();

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<ICacheService, CacheService>();


        return services;
    }

    public static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IUserTokenService, UserTokenService>();
        services.AddSingleton<IVerificationCodeService, VerificationCodeService>();
        services.AddSingleton<ICodeHasher, CodeHasher>();
        services.AddSingleton<ITokenHasher, TokenHasher>();
        services.AddSingleton<IMessageGenerator, MessageGenerator>();
        services.AddScoped<IUserDeviceManager, UserDeviceManager>();
        services.AddScoped<IAuthTokenGenerator, AuthTokenGenerator>();
        services.AddScoped<IStorageService, CloudinaryStorageService>();

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<TwilioSettings>(configuration.GetSection(TwilioSettings.SectionName));
        services.Configure<GoogleSettings>(configuration.GetSection(GoogleSettings.SectionName));
        services.Configure<FacebookSettings>(configuration.GetSection(FacebookSettings.SectionName));
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
        services.Configure<PaymobSettings>(configuration.GetSection(PaymobSettings.SectionName));


        services.AddMemoryCache();
        services.AddSingleton<IDailyUploadTracker, RadisDailyUploadTracker>();

        services.AddScoped<IUploadStrategy, ProfileImageUploadStrategy>();
        services.AddScoped<IUploadStrategy, HousingUnitUploadStrategy>();
        services.AddScoped<IUploadStrategy, PackageImageUploadStrategy>();
        services.AddScoped<IUploadStrategy, VerificationUploadStrategy>();

        services.AddScoped<IMessageService, MessageService>();
        services.AddSingleton<IEmailService, EmailService>();

        services.AddSingleton<ISocialAuthService, SocialAuthService>();
        services.AddHttpClient<ISocialAuthStrategy, FacebookAuthStrategy>();
        services.AddSingleton<ISocialAuthStrategy, GoogleAuthStrategy>();
        services.AddSingleton<ISocialAuthStrategy, MockAppleAuthService>();

        services.AddScoped<IVerificationStrategy, TourGuideVerificationStrategy>();
        services.AddScoped<IVerificationStrategy, TourCompanyVerificationStrategy>();
        services.AddScoped<IVerificationStrategy, GuidePackageVerificationStrategy>();
        services.AddScoped<IVerificationStrategy, HousingUnitVerificationStrategy>();

        services.AddScoped<IVerificationFactory, VerificationFactory>();

        services.AddScoped<IFileStorageService, LocalFileService>();
        services.AddSingleton<IFirebaseNotificationService, FirebaseNotificationService>();
        services.AddScoped<INotificationScheduler, NotificationScheduler>();

        services.AddScoped<IInventoryModerationService, InventoryModerationService>();

        services.AddHttpClient<IPaymentService, PaymobPaymentService>();

        // Chatbot Module
        services.Configure<GeminiSettings>(configuration.GetSection(GeminiSettings.SectionName));
        services.Configure<OpenAISettings>(configuration.GetSection(OpenAISettings.SectionName));
        services.AddHttpClient<GeminiChatbotService>();
        services.AddHttpClient<OpenAIChatbotService>();
        services.AddScoped<IChatbotServiceFactory, ChatbotServiceFactory>();
        services.AddScoped<IChatbotInteractionService, ChatbotInteractionService>();


        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.Section, jwtSettings);

        services.AddSingleton(Options.Create(jwtSettings));
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                RoleClaimType = "roles",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            });

        return services;
    }

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

        services.AddHangfireServer();

        return services;
    }
}