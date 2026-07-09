using BookingHotel.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHotel.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
  public void Configure(EntityTypeBuilder<Room> builder)
  {
    builder.ToTable("rooms");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasColumnName("id")
        .HasConversion(
            id => id.Value,
            value => new RoomId(value))
        .ValueGeneratedNever();

    builder.Property(x => x.HotelId)
        .HasColumnName("hotel_id")
        .HasConversion(
            id => id.Value,
            value => new HotelId(value));

    builder.Property(x => x.RoomTypeId)
        .HasColumnName("room_type_id")
        .HasConversion(
            id => id.Value,
            value => new RoomTypeId(value));

    builder.Property(x => x.RoomNumber)
        .HasColumnName("room_number")
        .HasMaxLength(30)
        .IsRequired();

    builder.Property(x => x.Floor)
        .HasColumnName("floor");

    builder.Property(x => x.Status)
        .HasColumnName("status")
        .HasConversion<string>()
        .HasMaxLength(30);

    builder.Property(x => x.CreatedAt)
        .HasColumnName("created_at");

    builder.Property(x => x.UpdatedAt)
        .HasColumnName("updated_at");

    builder.HasOne<RoomType>()
        .WithMany()
        .HasForeignKey(x => x.RoomTypeId)
        .OnDelete(DeleteBehavior.Restrict);
  }
}
