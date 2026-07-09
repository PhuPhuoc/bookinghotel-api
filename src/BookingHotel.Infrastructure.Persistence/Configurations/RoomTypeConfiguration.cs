using BookingHotel.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHotel.Infrastructure.Persistence.Configurations;

public sealed class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
  public void Configure(EntityTypeBuilder<RoomType> builder)
  {
    builder.ToTable("room_types");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasColumnName("id")
        .HasConversion(
            id => id.Value,
            value => new RoomTypeId(value))
        .ValueGeneratedNever();

    builder.Property(x => x.HotelId)
        .HasColumnName("hotel_id")
        .HasConversion(
            id => id.Value,
            value => new HotelId(value));

    builder.Property(x => x.Name)
        .HasColumnName("name")
        .HasMaxLength(200)
        .IsRequired();

    builder.Property(x => x.Description)
        .HasColumnName("description")
        .HasMaxLength(1000);

    builder.Property(x => x.BasePrice)
        .HasColumnName("base_price")
        .HasColumnType("decimal(18,2)");

    builder.Property(x => x.MaxAdults)
        .HasColumnName("max_adults");

    builder.Property(x => x.MaxChildren)
        .HasColumnName("max_children");

    builder.Property(x => x.CreatedAt)
        .HasColumnName("created_at");

    builder.Property(x => x.UpdatedAt)
        .HasColumnName("updated_at");
  }
}
