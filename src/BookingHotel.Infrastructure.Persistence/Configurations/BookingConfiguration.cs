using BookingHotel.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHotel.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
  public void Configure(EntityTypeBuilder<Booking> builder)
  {
    // 1. Booking (Aggregate Root)
    builder.ToTable("bookings");

    builder.HasKey(b => b.Id);

    builder.Property(b => b.Id)
        .HasColumnName("id")
        .HasConversion(
            id => id.Value,
            value => new BookingId(value))
        .ValueGeneratedNever();

    builder.Property(b => b.CustomerId)
        .HasColumnName("customer_id")
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(b => b.CheckInDate)
        .HasColumnName("check_in_date")
        .IsRequired();

    builder.Property(b => b.CheckOutDate)
        .HasColumnName("check_out_date")
        .IsRequired();

    builder.Property(b => b.Status)
        .HasColumnName("status")
        .HasConversion<string>()
        .HasMaxLength(50)
        .IsRequired();

    builder.Property(b => b.TotalNight)
        .HasColumnName("total_night")
        .IsRequired();

    builder.Property(b => b.Note)
        .HasColumnName("note")
        .HasMaxLength(1000);

    builder.Property(b => b.CreatedAt)
        .HasColumnName("created_at")
        .IsRequired();

    builder.Property(b => b.UpdatedAt)
        .HasColumnName("updated_at")
        .IsRequired();

    // 2. BookedRoom (OwnsMany)
    builder.OwnsMany(b => b.BookedRooms, roomBuilder =>
    {
      roomBuilder.ToTable("booked_rooms");

      roomBuilder.WithOwner()
              .HasForeignKey("booking_id");

      roomBuilder.HasKey(r => r.Id);

      roomBuilder.Property(r => r.Id)
              .HasColumnName("id")
              .HasConversion(
                  id => id.Value,
                  value => new BookedRoomId(value))
              .ValueGeneratedNever();

      roomBuilder.Property(r => r.RoomId)
              .HasColumnName("room_id")
              .HasMaxLength(100)
              .IsRequired();

      roomBuilder.Property(r => r.RoomNumberSnapshot)
              .HasColumnName("room_number_snapshot")
              .HasMaxLength(50)
              .IsRequired();

      roomBuilder.Property(r => r.RoomTypeSnapshot)
              .HasColumnName("room_type_snapshot")
              .HasMaxLength(100)
              .IsRequired();

      roomBuilder.Property(r => r.PricePerNightSnapshot)
              .HasColumnName("price_per_night_snapshot")
              .HasColumnType("decimal(18,2)")
              .IsRequired();

      roomBuilder.Property(r => r.MaxCapacitySnapshot)
              .HasColumnName("max_capacity_snapshot")
              .IsRequired();

      roomBuilder.Property(r => r.CheckInDate)
              .HasColumnName("check_in_date")
              .IsRequired();

      roomBuilder.Property(r => r.CheckOutDate)
              .HasColumnName("check_out_date")
              .IsRequired();

      roomBuilder.Property(r => r.CreatedAt)
              .HasColumnName("created_at")
              .IsRequired();

      roomBuilder.Property(r => r.UpdatedAt)
              .HasColumnName("updated_at")
              .IsRequired();

      // 3. BookedRoom own GuestStay
      roomBuilder.OwnsMany(r => r.GuestStays, guestBuilder =>
          {
            guestBuilder.ToTable("guest_stays");

            guestBuilder.WithOwner()
                    .HasForeignKey("booked_room_id");

            guestBuilder.HasKey(g => g.Id);

            guestBuilder.Property(g => g.Id)
                    .HasColumnName("id")
                    .HasConversion(
                        id => id.Value,
                        value => new GuestStayId(value))
                    .ValueGeneratedNever();

            guestBuilder.Property(g => g.FullName)
                    .HasColumnName("full_name")
                    .HasMaxLength(200)
                    .IsRequired();

            guestBuilder.Property(g => g.IdNumber)
                    .HasColumnName("id_number")
                    .HasMaxLength(50)
                    .IsRequired();

            guestBuilder.Property(g => g.IsAdult)
                    .HasColumnName("is_adult")
                    .IsRequired();

            guestBuilder.Property(g => g.DateOfBirth)
                    .HasColumnName("date_of_birth")
                    .IsRequired();

            guestBuilder.Property(g => g.Nationality)
                    .HasColumnName("nationality")
                    .HasMaxLength(100)
                    .IsRequired();
          });

      // Ép EF Core truy cập vào danh sách GuestStays thông qua backing field private _guestStays
      roomBuilder.Navigation(r => r.GuestStays)
              .UsePropertyAccessMode(PropertyAccessMode.Field);
    });

    // Ép EF Core truy cập vào danh sách BookedRooms thông qua backing field private _bookedRooms
    builder.Navigation(b => b.BookedRooms)
        .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
