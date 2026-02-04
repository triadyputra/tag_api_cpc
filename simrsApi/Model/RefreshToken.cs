namespace cpcApi.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }

        public DateTime ExpiredAt { get; set; }
        public bool IsUsed { get; set; }      // 🔥 sudah dipakai?
        public bool IsRevoked { get; set; }   // 🔥 dicabut paksa?
        public DateTime CreatedAt { get; set; }
    }
}
