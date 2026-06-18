using Fayora.Domain.Entities.AccommodationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Fayora.Infrastructure.Persistence.Configurations.AccommodationModule;

public class HousingUnitConfiguration : IEntityTypeConfiguration<HousingUnit>
{
       public void Configure(EntityTypeBuilder<HousingUnit> builder)
       {
              builder.ToTable("HousingUnits");
              builder.HasKey(h => h.Id);

              builder.HasIndex(h => h.LocationId);
              builder.HasIndex(h => h.OwnerId);
              builder.HasIndex(h => h.Status);

              builder.Property(h => h.Title)
                     .IsRequired()
                     .HasMaxLength(200);

              builder.Property(h => h.Description)
                     .HasMaxLength(1000);

              builder.Property(h => h.AddressDetails)
                     .IsRequired()
                     .HasMaxLength(500);

              builder.OwnsOne(h => h.Coordinates, coord =>
              {
                     coord.Property(c => c.Latitude)
                    .HasColumnName("Latitude")
                    .IsRequired();

                     coord.Property(c => c.Longitude)
                    .HasColumnName("Longitude")
                    .IsRequired();
              });

              builder.OwnsOne(h => h.MainImageUrl, url =>
              {
                     url.Property(u => u.Value)
                  .HasColumnName("MainImageUrl")
                  .HasMaxLength(2048)
                  .IsRequired(false);
              });

              builder.OwnsOne(h => h.VerificationDocumentUrl, url =>
              {
                     url.Property(u => u.Value)
                  .HasColumnName("VerificationDocumentUrl")
                  .HasMaxLength(2048)
                  .IsRequired(false);
              });
              
              builder.Property(h => h.CheckInTime).HasColumnType("time");
              builder.Property(h => h.CheckOutTime).HasColumnType("time");

              builder.Property(h => h.PricePerNight).HasColumnType("decimal(18,2)");
              builder.Property(h => h.CommissionRate).HasColumnType("decimal(18,2)");
              builder.Property(h => h.Rating).HasColumnType("decimal(3,2)");

              builder.Property(h => h.Type)
                     .HasConversion<string>()
                     .HasMaxLength(50);

              builder.Property(x => x.AdminNotes)
                     .HasColumnType("nvarchar(500)");

              builder.Property(x => x.Status)
                     .HasConversion<int>()
                     .IsRequired();

              builder.Property<List<Guid>>("_imageIds")
                     .HasColumnName("ImageIds")
                     .HasColumnType("nvarchar(max)")
                     .HasConversion(
                         ids => JsonSerializer.Serialize(ids, JsonSerializerOptions.Default),
                         json => JsonSerializer.Deserialize<List<Guid>>(json, JsonSerializerOptions.Default)!);

              builder.HasMany(h => h.Amenities)
                     .WithMany()
                     .UsingEntity<Dictionary<string, object>>(
                         "HousingUnitMasterAmenities",
                         j => j.HasOne<MasterAmenity>().WithMany().HasForeignKey("MasterAmenityId").OnDelete(DeleteBehavior.Cascade),
                         j => j.HasOne<HousingUnit>().WithMany().HasForeignKey("HousingUnitId").OnDelete(DeleteBehavior.Cascade));

              builder.Navigation(h => h.Amenities)
                     .HasField("_amenities")
                     .UsePropertyAccessMode(PropertyAccessMode.Field);


       }
}