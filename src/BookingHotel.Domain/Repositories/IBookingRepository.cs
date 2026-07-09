using BookingHotel.Domain.Bookings;

namespace BookingHotel.Domain.Repositories;

public interface IBookingRepository
{
  #region 1. FLOW KHỞI TẠO & ĐẶT PHÒNG (Booking Creation Flow)

  /// <summary>
  /// Lưu mới một đơn đặt phòng vào cơ sở dữ liệu khi khách hàng vừa bấm "Đặt phòng".
  /// Trạng thái ban đầu thường là Pending.
  /// </summary>
  /// <param name="booking">Aggregate Root Booking chứa thông tin phòng và khách hàng.</param>
  /// <param name="cancellationToken">Mã hủy tiến trình nếu request bị hủy sớm.</param>
  Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

  #endregion

  #region 2. FLOW THAY ĐỔI & CẬP NHẬT (Modification Flow - State: Pending/Confirmed)

  /// <summary>
  /// Lấy thông tin một đơn đặt phòng theo ID để chuẩn bị cho việc chỉnh sửa thông tin 
  /// (như thêm/xóa phòng, thay đổi thông tin khách, cập nhật ghi chú, thay đổi thời gian).
  /// </summary>
  /// <remarks>
  /// Hàm này bắt buộc phải lấy kèm (Eager Loading) danh sách BookedRooms và GuestStays 
  /// để Aggregate Root có thể kiểm tra tính toàn vẹn dữ liệu (ví dụ: kiểm tra sức chứa tối đa).
  /// </remarks>
  Task<Booking?> GetByIdAsync(BookingId id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Cập nhật lại toàn bộ trạng thái của Aggregate Booking sau khi thực hiện các logic thay đổi bên trong Domain.
  /// Dùng cho cả luồng: Đổi phòng, thêm khách, cập nhật ghi chú, v.v.
  /// </summary>
  void Update(Booking booking);

  #endregion

  #region 3. FLOW VẬN HÀNH & QUẢN LÝ (Hotel Operations Flow - State: Confirmed -> CheckedIn -> Completed)

  /// <summary>
  /// Tìm kiếm đơn đặt phòng dựa trên Mã khách hàng. 
  /// Phục vụ cho luồng: Khách hàng muốn xem lại "Lịch sử đặt phòng" hoặc "Danh sách phòng sắp ở".
  /// </summary>
  Task<IReadOnlyCollection<Booking>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Tìm kiếm các đơn đặt phòng dựa trên danh sách trạng thái cụ thể.
  /// </summary>
  /// <remarks>
  /// Phục vụ đắc lực cho các luồng vận hành của Lễ tân/Quản lý:
  /// - Tìm các phòng có trạng thái 'Confirmed' và có CheckInDate là hôm nay để làm thủ tục Check-in.
  /// - Tìm các phòng 'CheckedIn' có CheckOutDate là hôm nay để làm thủ tục Check-out.
  /// </remarks>
  Task<IReadOnlyCollection<Booking>> GetByStatusesAsync(IEnumerable<BookingStatus> statuses, CancellationToken cancellationToken = default);

  /// <summary>
  /// Kiểm tra xem một phòng cụ thể (RoomId) đã bị trùng lịch đặt (bị trùng khoảng thời gian ở) trong hệ thống chưa.
  /// </summary>
  /// <remarks>
  /// Phục vụ luồng: Chặn không cho phép hai đơn đặt phòng khác nhau đặt trùng một phòng vật lý 
  /// trong cùng một khoảng thời gian (Overbooking/Double-booking).
  /// </remarks>
  Task<bool> HasOverlappingBookingAsync(
      string roomId,
      DateTime checkInDate,
      DateTime checkOutDate,
      CancellationToken cancellationToken = default);

  #endregion

  #region 4. FLOW XỬ LÝ ĐƠN HOÀN/HỦY/HẾT HẠN (Background Jobs & Cancellation Flow)

  /// <summary>
  /// Lấy danh sách các đơn đặt phòng vẫn đang ở trạng thái 'Pending' nhưng đã quá thời hạn thanh toán/giữ chỗ.
  /// </summary>
  /// <remarks>
  /// Phục vụ cho luồng: Một Background Job chạy tự động mỗi 5-10 phút quét qua hệ thống, 
  /// tìm các đơn quá hạn này rồi gọi hàm `booking.Expire()` để giải phóng phòng cho người khác đặt.
  /// </remarks>
  Task<IReadOnlyCollection<Booking>> GetExpiredPendingBookingsAsync(DateTime thresholdTime, CancellationToken cancellationToken = default);

  #endregion
}
