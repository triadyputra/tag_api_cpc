using cpcApi.Model.DTO;

namespace cpcApi.Services.Combo
{
    public interface IRepoCombo
    {
        public Task<IEnumerable<ComboViewModel>> ComboCabang();
        public Task<IEnumerable<ComboViewModel>> ComboBank();
    }
}
