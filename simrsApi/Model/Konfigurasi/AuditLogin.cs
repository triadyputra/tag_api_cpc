namespace cpcApi.Model.Konfigurasi
{
    public class AuditLogin
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }

        public string IpAddress { get; set; }
        public string Device { get; set; }
        public string UserAgent { get; set; }

        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }

        public bool IsSuccess { get; set; }
    }
}
