using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.MasterData
{
    public class MasterDenom
    {
        [Key]
        public int Id { get; set; }
        public int Nominal { get; set; }

        public int Urutan { get; set; }

        public bool Active { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
