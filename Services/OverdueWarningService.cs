using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    public static class OverdueWarningService
    {
        public const string SeverityReminder = "Nhắc nhở";      // 1 - 3 ngày
        public const string SeverityWarning = "Cảnh báo";       // 4 - 7 ngày
        public const string SeverityCritical = "Nghiêm trọng";  // > 7 ngày

        public const string StatusPending = "Chưa nhắc nhở";
        public const string StatusReminded = "Đã nhắc nhở";
        public const string StatusReturned = "Đã trả sách";

        /// <summary>
        /// Tự động phân loại mức độ quá hạn theo số ngày.
        /// </summary>
        public static string ClassifySeverity(int overdueDays)
        {
            if (overdueDays <= 3)
                return SeverityReminder;
            if (overdueDays <= 7)
                return SeverityWarning;
            return SeverityCritical;
        }

        /// <summary>
        /// Đồng bộ cảnh báo quá hạn từ tblBorrowTicket & tblBorrowDetail vào tblOverdueWarning.
        /// Mỗi phiếu mượn quá hạn chỉ tạo/cập nhật 1 bản ghi gộp duy nhất (tránh trùng lặp).
        /// </summary>
        public static void SyncOverdueWarnings(DataContext context)
        {
            DateTime today = DateTime.Today;
            DateTime now = DateTime.Now;

            var activeStatuses = new[] { "Đang mượn", "Trả một phần", "Quá hạn", "Chờ duyệt trả" };

            var candidateTickets = context.BorrowTickets
                .Where(t => t.Status != null && activeStatuses.Contains(t.Status) && t.DueDate < today)
                .ToList();

            var existingWarnings = context.OverdueWarnings.ToList();
            var warningByTicketId = existingWarnings.ToDictionary(w => w.TicketID, w => w);

            var allRelevantTicketIds = candidateTickets.Select(t => t.TicketID)
                .Union(existingWarnings.Where(w => w.WarningStatus != StatusReturned).Select(w => w.TicketID))
                .Distinct()
                .ToList();

            var detailsByTicketId = context.BorrowDetails
                .Where(d => allRelevantTicketIds.Contains(d.TicketID))
                .ToList()
                .GroupBy(d => d.TicketID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var bookIds = detailsByTicketId.Values
                .SelectMany(list => list)
                .Select(d => d.BookID)
                .Distinct()
                .ToList();

            var bookTitleDict = context.Books
                .Where(b => bookIds.Contains(b.BookID))
                .ToDictionary(b => b.BookID, b => b.Title ?? b.Code);

            var activeOverdueTicketIds = new HashSet<int>();
            bool hasChanges = false;

            foreach (var ticket in candidateTickets)
            {
                var details = detailsByTicketId.ContainsKey(ticket.TicketID)
                    ? detailsByTicketId[ticket.TicketID]
                    : new List<tblBorrowDetail>();

                var unreturnedDetails = details.Where(d => !d.IsReturned).ToList();

                // Nếu phiếu có chi tiết sách và tất cả đã trả thì không còn quá hạn
                if (details.Count > 0 && unreturnedDetails.Count == 0)
                {
                    continue;
                }

                activeOverdueTicketIds.Add(ticket.TicketID);

                int overdueDays = Math.Max(1, (today - ticket.DueDate.Date).Days);
                string severity = ClassifySeverity(overdueDays);
                int overdueBookCount = unreturnedDetails.Count > 0 ? unreturnedDetails.Count : 1;

                var titles = unreturnedDetails
                    .Select(d => bookTitleDict.ContainsKey(d.BookID) ? bookTitleDict[d.BookID] : $"Sách #{d.BookID}")
                    .Distinct()
                    .ToList();

                string consolidatedTitles = titles.Count > 0
                    ? string.Join(", ", titles)
                    : "Tài liệu thư viện";

                if (consolidatedTitles.Length > 980)
                {
                    consolidatedTitles = consolidatedTitles.Substring(0, 977) + "...";
                }

                if (warningByTicketId.TryGetValue(ticket.TicketID, out var existing))
                {
                    bool changed = false;

                    if (existing.UserID != ticket.UserID) { existing.UserID = ticket.UserID; changed = true; }
                    if (existing.OverdueDays != overdueDays) { existing.OverdueDays = overdueDays; changed = true; }
                    if (existing.SeverityLevel != severity) { existing.SeverityLevel = severity; changed = true; }
                    if (existing.OverdueBookCount != overdueBookCount) { existing.OverdueBookCount = overdueBookCount; changed = true; }
                    if (existing.BookTitles != consolidatedTitles) { existing.BookTitles = consolidatedTitles; changed = true; }

                    if (existing.WarningStatus == StatusReturned)
                    {
                        existing.WarningStatus = StatusPending;
                        existing.ResolvedDate = null;
                        changed = true;
                    }

                    if (changed)
                    {
                        existing.UpdatedDate = now;
                        hasChanges = true;
                    }
                }
                else
                {
                    var newWarning = new tblOverdueWarning
                    {
                        TicketID = ticket.TicketID,
                        UserID = ticket.UserID,
                        OverdueDays = overdueDays,
                        SeverityLevel = severity,
                        OverdueBookCount = overdueBookCount,
                        BookTitles = consolidatedTitles,
                        WarningStatus = StatusPending,
                        ReminderCount = 0,
                        CreatedDate = now,
                        UpdatedDate = now
                    };
                    context.OverdueWarnings.Add(newWarning);
                    hasChanges = true;
                }
            }

            // Cập nhật trạng thái "Đã trả sách" cho các cảnh báo không còn quá hạn
            var nonReturnedWarnings = existingWarnings
                .Where(w => w.WarningStatus != StatusReturned && !activeOverdueTicketIds.Contains(w.TicketID))
                .ToList();

            if (nonReturnedWarnings.Count > 0)
            {
                var ticketIdsToCheck = nonReturnedWarnings.Select(w => w.TicketID).ToList();
                var ticketDict = context.BorrowTickets
                    .Where(t => ticketIdsToCheck.Contains(t.TicketID))
                    .ToDictionary(t => t.TicketID, t => t);

                foreach (var warning in nonReturnedWarnings)
                {
                    ticketDict.TryGetValue(warning.TicketID, out var ticket);
                    warning.WarningStatus = StatusReturned;
                    warning.ResolvedDate = ticket?.ReturnDate ?? now;
                    warning.UpdatedDate = now;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                context.SaveChanges();
            }
        }
    }
}
