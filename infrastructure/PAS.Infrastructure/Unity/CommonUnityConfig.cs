using PAS.Common;
using StackExchange.Redis;
using System.Configuration;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using PAS.Repositories;
using PAS.Services;
using PAS.Services.Story;
using PAS.Services.Comment;
using PAS.Repositories.Projects;
using PAS.Services.Projects;
using PAS.Repositories.Mapping;
using PAS.Services.Criterion;
using System;

namespace PAS.Infrastructure
{
    public class CommonUnityConfig
    {
        public static void RegisterTypes(IUnityContainer container, bool usePerrequestLifeTimeManager)
        {
            container.RegisterType<IDistributedCache, DistributedRedisCache>(GetLifetimeManager(usePerrequestLifeTimeManager), new InjectionConstructor(new object[] {
                ConfigurationOptions.Parse(ConfigurationManager.AppSettings["RedisConnectionString"]),
                "PAS"
            }));
            container.RegisterType<IAuthProvider, AuthProvider>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ISharepointContextProvider, SharepointContextProvider>(GetLifetimeManager(usePerrequestLifeTimeManager));

            // Core Mappers
            container.RegisterType<PAS.Repositories.Mapping.IUserMapper, PAS.Repositories.Mapping.UserMapper>();
            container.RegisterType<PAS.Repositories.Mapping.IStoryMapper, PAS.Repositories.Mapping.StoryMapper>();
            container.RegisterType<PAS.Repositories.Mapping.ITaskMapper, PAS.Repositories.Mapping.TaskMapper>();
            container.RegisterType<PAS.Repositories.Mapping.ICheckListItemMapper, PAS.Repositories.Mapping.CheckListItemMapper>();
            container.RegisterType<PAS.Repositories.Mapping.IStoryMapper, PAS.Repositories.Mapping.StoryMapper>();
            container.RegisterType<PAS.Repositories.Mapping.IPersonalNoteMapper, PAS.Repositories.Mapping.PersonalNoteMapper>();
            container.RegisterType<PAS.Repositories.Mapping.ICareerPathMapper, PAS.Repositories.Mapping.CareerPathMapper>();
            container.RegisterType<PAS.Repositories.Mapping.ICareerPathStepMapper, PAS.Repositories.Mapping.CareerPathStepMapper>();
            container.RegisterType<PAS.Repositories.Mapping.ICareerPathTemplateMapper, PAS.Repositories.Mapping.CareerPathTemplateMapper>();
            container.RegisterType<PAS.Repositories.Mapping.ICareerPathTemplateStepMapper, PAS.Repositories.Mapping.CareerPathTemplateStepMapper>();
            container.RegisterType<PAS.Repositories.Mapping.ICommentMapper, PAS.Repositories.Mapping.CommentMapper>();
            container.RegisterType<PAS.Repositories.Mapping.ITaskUnfollowerMapper, PAS.Repositories.Mapping.TaskUnfollowerMapper>();
            container.RegisterType<PAS.Repositories.Mapping.IProjectMapper, PAS.Repositories.Mapping.ProjectMapper>();
            container.RegisterType<PAS.Repositories.Mapping.IPerformanceReviewPeriodMapper, PAS.Repositories.Mapping.PerformanceReviewPeriodMapper>();
            container.RegisterType<PAS.Repositories.Mapping.IUserPerformanceReviewsMapper, PAS.Repositories.Mapping.UserPerformanceReviewsMapper>();
            container.RegisterType<PAS.Repositories.Mapping.IProjectsAndUsersMapper, PAS.Repositories.Mapping.ProjectsAndUsersMapper>();
            container.RegisterType<ICriteriaMapper, CriteriaMapper>();
            container.RegisterType<ICriteriaGroupMapper, CriteriaGroupMapper>();
            container.RegisterType<ICriteriaOptionsMapper, CriteriaOptionsMapper>();
            container.RegisterType<ICriteriaOptionsListMapper, CriteriaOptionsListMapper>();
            container.RegisterType<IPerformanceReviewAndUserResultMapper, PerformanceReviewAndUserResultMapper>();
            container.RegisterType<IEvaluationReviewOptionsMapper, EvaluationReviewOptionsMapper>();
            container.RegisterType<IEvaluationReviewOptionMapper, EvaluationReviewOptionMapper>();
            container.RegisterType<IProjectCriterionMapper, ProjectCriterionMapper>();
            container.RegisterType<IProfileCategoryMapper, ProfileCategoryMapper>();
            container.RegisterType<IProfileItemMapper, ProfileItemMapper>(); 
            container.RegisterType<IPrecioFishboneWorkExperienceMapper, PrecioFishboneWorkExperienceMapper>();
            container.RegisterType<ISkillMapper, SkillMapper>();
            container.RegisterType<IGenericItemMapper, GenericItemMapper>();
            container.RegisterType<IPortfolioFileMapper, PortfolioFileMapper>();
            container.RegisterType<IResumeMapper, ResumeMapper>();
            container.RegisterType<ITemplateMapper, TemplateMapper>();
            container.RegisterType<ITemplateProfileCategoryMapper, TemplateProfileCategoryMapper>();
            container.RegisterType<ISkillCategoryMapper, SkillCategoryMapper>();
            container.RegisterType<IResumeHighlightedSkillCategoryMapper, ResumeHighlightedSkillCategoryMapper>();
            container.RegisterType<IResumeHighlightedSkillMapper, ResumeHighlightedSkillMapper>();
            container.RegisterType<IPersonLookupMapper, PersonLookupMapper>();
            container.RegisterType<IResignationTypeMapper, ResignationTypeMapper>();
            container.RegisterType<IUserCoachingMapper, UserCoachingMapper>();
            container.RegisterType<IKudoMapper, KudoMapper>();
            container.RegisterType(typeof(IMetadataMapper<>), typeof(MetadataMapper<>));
            container.RegisterType<ITeamMapper, TeamMapper>();
            container.RegisterType<ITeamMemberMapper, TeamMemberMapper>();
            container.RegisterType<IDepartmentMapper, DepartmentMapper>();
            container.RegisterType<IConfigurationMapper, ConfigurationMapper>();

            container.RegisterType<PAS.Model.Mapping.ISharePointInfoDtoMapper, PAS.Model.Mapping.SharePointInfoDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IStoryDtoMapper, PAS.Model.Mapping.StoryDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ITaskDtoMapper, PAS.Model.Mapping.TaskDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ICheckListDtoMapper, PAS.Model.Mapping.CheckListDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IUserDtoMapper, PAS.Model.Mapping.UserDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IPersonalNoteDtoMapper, PAS.Model.Mapping.PersonalNoteDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ICommentDtoMapper, PAS.Model.Mapping.CommentDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IProjectDtoMapper, PAS.Model.Mapping.ProjectDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ICriteriaGroupDtoMapper, PAS.Model.Mapping.CriteriaGroupDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ICriteriaDtoMapper, PAS.Model.Mapping.CriteriaDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ICriteriaOptionMapper, PAS.Model.Mapping.CriteriaOptionMapper>();
            container.RegisterType<PAS.Model.Mapping.ICriteriaOptionListDtoMapper, PAS.Model.Mapping.CriteriaOptionListDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IPerformanceReviewPeriodDtoMapper, PAS.Model.Mapping.PerformanceReviewPeriodDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IPerformanceReviewResultDtoMapper, PAS.Model.Mapping.PerformanceReviewResultDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IEvaluationReviewOptionsDtoMapper, PAS.Model.Mapping.EvaluationReviewOptionsDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IProfileCategoryDtoMapper, PAS.Model.Mapping.ProfileCategoryDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IProfileItemDtoMapper, PAS.Model.Mapping.ProfileItemDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ISkillDtoMapper, PAS.Model.Mapping.SkillDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IGenericItemDtoMapper, PAS.Model.Mapping.GenericItemDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IPortfolioFileDtoMapper, PAS.Model.Mapping.PortfolioFileDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IResumeDtoMapper, PAS.Model.Mapping.ResumeDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ITemplateDtoMapper, PAS.Model.Mapping.TemplateDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ITemplateProfileCategoryDtoMapper, PAS.Model.Mapping.TemplateProfileCategoryDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ISharepointImageDtoMapper, PAS.Model.Mapping.SharepointImageDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ISkillCategoryDtoMapper, PAS.Model.Mapping.SkillCategoryDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IResumeHighlightedSkillCategoryDtoMapper, PAS.Model.Mapping.ResumeHighlightedSkillCategoryDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IResumeHighlightedSkillDtoMapper, PAS.Model.Mapping.ResumeHighlightedSkillDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IResumeHighlightedSkillCategoryRequestDtoMapper, PAS.Model.Mapping.ResumeHighlightedSkillCategoryRequestDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ICompanySkillDtoMapper, PAS.Model.Mapping.CompanySkillDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ITeamDtoMapper, PAS.Model.Mapping.TeamDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.ITeamMembersDtoMapper, PAS.Model.Mapping.TeamMembersDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IResignationTypeDtoMapper, PAS.Model.Mapping.ResignationTypeDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IGroupDtoMapper, PAS.Model.Mapping.GroupDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IGenderDtoMapper, PAS.Model.Mapping.GenderDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IUserCoachingDtoMapper, PAS.Model.Mapping.UserCoachingDtoMapper>();
            container.RegisterType<Model.Mapping.IKudoDtoMapper, Model.Mapping.KudoDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IMetadataDtoMapper, PAS.Model.Mapping.MetadataDtoMapper>();
            container.RegisterType<PAS.Model.Mapping.IDepartmentDtoMapper, PAS.Model.Mapping.DepartmentDtoMapper>();

            // Core Repositories
            container.RegisterType<IUserRepository, UserRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IStoryRepository, StoryRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITaskRepository, TaskRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICheckListItemRepository, CheckListItemRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPersonalNoteRepository, PersonalNoteRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICareerPathRepository, CareerPathRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICommentRepository, CommentRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITaskUnfollowerRepository, TaskUnfollowerRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IProjectRepository, ProjectRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPerformanceReviewPeriodRepository, PerformanceReviewPeriodRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICriteriaGroupRepository, CriteriaGroupRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICriteriaOptionsListRepository, CriteriaOptionListRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IEvaluationReviewOptionsRepository, EvaluationReviewOptionsRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICriteriaRepository, CriteriaRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IProfileRepository, ProfileRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IProfileItemRepository, ProfileItemRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ILookupProfileRepository, LookupProfileRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IResumeRepository, ResumeRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITemplateRepository, TemplateRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPortfolioFileRepository, PortfolioFileRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IResumeHighlightedSkillCategoryRepository, ResumeHighlightedSkillCategoryRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IResumeHighlightedSkillRepository, ResumeHighlightedSkillRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IUserCoachingRepository, UserCoachingRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IKudoRepository, KudoRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IStatisticsRepository, StatisticsRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType(typeof(IMetadataRepository<>), typeof(MetadataRepository<>), GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITeamRepository, TeamRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITeamMemberRepository, TeamMemberRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IConfigurationRepository, ConfigurationRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IDepartmentRepository, DepartmentRepository>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IRecordRepository<Model.WorkRecord>, WorkRecordRepository> (GetLifetimeManager(usePerrequestLifeTimeManager));

            // Core Services
            container.RegisterType<IMsGraphService, MsGraphService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IHomeService, HomeService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IStoryService, StoryService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IStorySharepointService, StorySharepointService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITaskService, TaskService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITasksSecurityService, TasksSecurityService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICheckListService, CheckListService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IUserService, UserService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IEnumService, EnumService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IStorySecurityService, StorySecurityService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPersonalNoteSecurityService, PersonalNoteSecurityService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICareerPathService, CareerPathService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICareerPathSecurityService, CareerPathSecurityService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPersonalNoteService, PersonalNoteService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPersonalNoteSharepointService, PersonalNoteSharepointService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IEmailService, EmailService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICommentService, CommentService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITaskFollowerService, TaskFollowerService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IProjectServices, ProjectServices>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ICriterionServices, CriterionServices>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IOptionListService, OptionListService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPerformanceReviewService, PerformanceReviewService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IEvaluationReviewOptionsService, EvaluationReviewOptionsService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IProfileService, ProfileService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IProfileItemService, ProfileItemService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ILookupProfileService, LookupProfileService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IResumeService, ResumeService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITemplateService, TemplateService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPortfolioFileService, PortfolioFileService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IResumeHighlightedSkillCategoryService, ResumeHighlightedSkillCategoryService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IResumeHighlightedSkillService, ResumeHighlightedSkillService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ISharepointFileService, SharepointFileService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IPermissionService, PermissionService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ISPUserService, SPUserService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IUserCoachingService, UserCoachingService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IKudoService, KudoService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IStatisticsService, StatisticsService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IMetadataService, MetadataService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITeamService, TeamService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<ITeamMemberService, TeamMemberService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IConfigurationService, ConfigurationService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IDepartmentService, DepartmentService>(GetLifetimeManager(usePerrequestLifeTimeManager));
            container.RegisterType<IWorkHistoryService, WorkHistoryService>(GetLifetimeManager(usePerrequestLifeTimeManager));
        }

        public static ITypeLifetimeManager GetLifetimeManager(bool usePerrequestLifeTimeManager)
        {
            if (usePerrequestLifeTimeManager)
                return new PerResolveLifetimeManager();
            else
                return new ContainerControlledLifetimeManager();
        }
    }
}
