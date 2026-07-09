using BookingHotel.Domain.Hotels;

namespace BookingHotel.Domain.Repositories;

public interface IHotelRepository
{
  /// <summary>
  /// Lấy thông tin Hotel theo Id (Bao gồm cả danh sách RoomTypes và Rooms đi kèm)
  /// </summary>
  Task<Hotel?> GetById(HotelId id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Thêm mới một Hotel Aggregate vào cơ sở dữ liệu
  /// </summary>
  void Add(Hotel hotel, CancellationToken cancellationToken = default);

  /// <summary>
  /// Cập nhật thay đổi của Hotel Aggregate. 
  /// (Trong EF Core, phương thức này có thể để trống hoặc gọi Unit of Work, nhưng định nghĩa ở đây để tường minh)
  /// </summary>
  void Update(Hotel hotel);

  /// <summary>
  /// Kiểm tra xem số phòng (RoomNumber) đã tồn tại trong khách sạn này chưa.
  /// Hỗ trợ cho quy tắc business kiểm tra trùng lặp nhanh mà không cần load toàn bộ Rooms lên bộ nhớ.
  /// </summary>
  Task<bool> HasRoomNumberAsync(HotelId hotelId, string roomNumber, CancellationToken cancellationToken = default);
}
