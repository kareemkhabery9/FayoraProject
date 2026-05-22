using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Domain.Common.Interfaces.IdentityModule;
using Fayora.Domain.Entities.AccommodationModule;
using Fayora.Domain.Entities.Booking;
using Fayora.Domain.Entities.ChatModule;
using Fayora.Domain.Entities.GuideModule;
using Fayora.Domain.Entities.IdentityModule;
using Fayora.Domain.Entities.SharedModule;
using Fayora.Domain.Entities.TouristModule;
using Fayora.Domain.Entities.ChatbotModule;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fayora.Infrastructure.Persistence.Repositories;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor, IPublisher publisher) : DbContext(options), IUnitOfWork
{
    // Identity Module
    public DbSet<User> Users { get; set; }
    public DbSet<UserIdentity> UserIdentities { get; set; }
    public DbSet<UserTokens> UserTokens { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }

    // Tourist Module
    public DbSet<TouristProfile> Tourists { get; set; }
    public DbSet<MasterInterest> MasterInterests { get; set; }
    public DbSet<UserInteraction> UserInteractions { get; set; }


    // Accommodation Module
    public DbSet<HousingUnit> HousingUnits { get; set; }
    public DbSet<UnitOwner> UnitOwners { get; set; }
    public DbSet<HousingUnitImage> HousingUnitImages { get; set; }
    public DbSet<CalendarBlock> CalendarBlocks { get; set; }


    // Guide Module
    public DbSet<TourGuide> TourGuides { get; set; }
    public DbSet<GuidePackage> GuideTourPackages { get; set; }
    public DbSet<GuideCity> GuideCities { get; set; }
    public DbSet<GuideRequest> GuideRequests { get; set; }
    public DbSet<GuideOffer> GuideOffers { get; set; }
    public DbSet<TourCompany> TourCompanies { get; set; }
    public DbSet<PackageActivity> PackageActivities { get; set; }
    public DbSet<PackageImage> PackageImages { get; set; }
    public DbSet<PackageOccurrence> PackageOccurrences { get; set; }
    public DbSet<GuideWeeklySchedule> GuideWeeklySchedules { get; set; }


    // Booking Module
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }


    // Shared Module
    public DbSet<City> Cities { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<LocationImage> LocationImages { get; set; }

    // Chatbot Module
    public DbSet<ChatbotSession> ChatbotSessions { get; set; }
    public DbSet<ChatbotMessage> ChatbotMessages { get; set; }

    // Notification Module
    public DbSet<Fayora.Domain.Entities.NotificationModule.DeviceToken> DeviceTokens { get; set; }
    public DbSet<Fayora.Domain.Entities.NotificationModule.PushCampaign> PushCampaigns { get; set; }


    public async Task CommitChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<AggregateRoot>()
            .SelectMany(x => x.Entity.GetDomainEvents())
            .ToList();

        if (IsUserWaitingOnline())
        {
            AddDomainEventsToOfflineProcessingQueue(domainEvents);
        }
        else
        {
            await PublishDomainEvents(publisher, domainEvents);
        }

        await SaveChangesAsync(cancellationToken);
    }

    private static async Task PublishDomainEvents(IPublisher publisher, List<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }

    private bool IsUserWaitingOnline() => httpContextAccessor.HttpContext is not null;

    private void AddDomainEventsToOfflineProcessingQueue(List<IDomainEvent> domainEvents)
    {
        var domainEventsQueue = httpContextAccessor.HttpContext!.Items
            .TryGetValue("DomainEventsQueue", out var value) && value is Queue<IDomainEvent> existingDomainEvents
                ? existingDomainEvents
                : new Queue<IDomainEvent>();

        domainEvents.ForEach(domainEventsQueue.Enqueue);

        httpContextAccessor.HttpContext!.Items["DomainEventsQueue"] = domainEventsQueue;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
