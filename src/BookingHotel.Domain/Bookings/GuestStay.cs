using BookingHotel.Domain.Common;

namespace BookingHotel.Domain.Bookings;

public sealed class GuestStay : Entity<GuestStayId>
{
  public string FullName { get; private set; } = "";

  public string IdNumber { get; private set; } = "";

  public bool IsAdult { get; private set; } = true;

  public DateTime DateOfBirth { get; private set; }

  public string Nationality { get; private set; } = "";

  private GuestStay()
  {
    // Required by EF Core.
  }

  private GuestStay(GuestStayId id, string fullName, string idNumber, bool isAdult, DateTime dob, string nationality)
  {
    Id = id;
    FullName = fullName;
    IdNumber = idNumber;
    IsAdult = isAdult;
    DateOfBirth = dob;
    Nationality = nationality;
  }

  internal static GuestStay Create(string fullName, string idNumber, bool isAdult, DateTime dob, string nationality)
  {
    return new GuestStay(
        GuestStayId.New(),
        fullName,
        idNumber,
        isAdult,
        dob,
        nationality);
  }

  internal void Update(string fullName, string idNumber, bool isAdult, DateTime dob, string nationality)
  {
    FullName = fullName;
    IdNumber = idNumber;
    IsAdult = isAdult;
    DateOfBirth = dob;
    Nationality = nationality;
  }
}
