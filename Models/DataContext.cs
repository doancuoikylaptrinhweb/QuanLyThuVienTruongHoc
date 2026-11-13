using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<tblCategory> Categories { get; set; }
        public DbSet<tblAuthor> Authors { get; set; }
        public DbSet<tblPublisher> Publishers { get; set; }
        public DbSet<tblBook> Books { get; set; }
        public DbSet<tblUser> Users { get; set; }
        public DbSet<tblBorrowTicket> BorrowTickets { get; set; }
        public DbSet<tblBorrowDetail> BorrowDetails { get; set; }
        public DbSet<tblAdminMenu> AdminMenus { get; set; }
    }
}