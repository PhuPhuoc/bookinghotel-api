using BookingHotel.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHotel.Infrastructure.Persistence.Configurations;

public sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
  public void Configure(EntityTypeBuilder<Hotel> builder)
  {
    builder.ToTable("hotels");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasColumnName("id")
        .HasConversion(
            id => id.Value,
            value => new HotelId(value))
        .ValueGeneratedNever();

    builder.Property(x => x.Name)
        .HasColumnName("name")
        .HasMaxLength(200)
        .IsRequired();

    builder.Property(x => x.Address)
        .HasColumnName("address")
        .HasMaxLength(500);

    builder.Property(x => x.Phone)
        .HasColumnName("phone")
        .HasMaxLength(30);

    builder.Property(x => x.Email)
        .HasColumnName("email")
        .HasMaxLength(255);

    builder.Property(x => x.Description)
        .HasColumnName("description")
        .HasMaxLength(2000);

    builder.Property(x => x.StarRating)
        .HasColumnName("star_rating")
        .IsRequired();

    builder.Property(x => x.CreatedAt)
        .HasColumnName("created_at")
        .IsRequired();

    builder.Property(x => x.UpdatedAt)
        .HasColumnName("updated_at")
        .IsRequired();

    builder.HasMany(x => x.RoomTypes)
        .WithOne()
        .HasForeignKey(x => x.HotelId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(x => x.Rooms)
        .WithOne()
        .HasForeignKey(x => x.HotelId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.RoomTypes)
        .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.Rooms)
        .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
