using System.Collections.Generic;

namespace PAS.Services
{
    public interface IProfileItemService
    {
        Model.Domain.ProfileItem AddProfileItem(Model.Domain.ProfileItem profileItem);
        void DeleteProfileItem(int id);
        void UpdateProfileItem(Model.Domain.ProfileItem profileItem);
        List<Model.Domain.ProfileItem> GetUserProfile(int userId);
        List<Model.Domain.ProfileItem> GetTopSkills(int userId, int topX);
        List<Model.Domain.CompanySkill> GetCompanySkills();
        List<Model.Domain.ProfileItem> GetProfileItemsBySkillId(int skillId);

    }
}
