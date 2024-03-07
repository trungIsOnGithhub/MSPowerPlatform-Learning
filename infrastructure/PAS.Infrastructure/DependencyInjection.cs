using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PAS.Common;
using PAS.Common.Configurations;
using PAS.Model.Mapping;
using PAS.Repositories;
using PAS.Repositories.HRM;
using PAS.Repositories.Mapping;
using PAS.Repositories.Projects;
using PAS.Services;
using PAS.Services.BackgroundTask;
using PAS.Services.Comment;
using PAS.Services.Criterion;
using PAS.Services.HRM;
using PAS.Services.Projects;
using PAS.Services.Story;
using StackExchange.Redis;
using Unity.Injection;

namespace PAS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection Register(this IServiceCollection services, IConfiguration configuration)
        {
            // Auto Mapper Configurations
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            services.AddSingleton<AzureConfigurations>(configuration.GetSection("AzureConfigurations").Get<AzureConfigurations>());
            services.AddSingleton<SharePointConfigurations>(configuration.GetSection("SharePointConfigurations").Get<SharePointConfigurations>());

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DbContext, CoreContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<CoreContext>();
            //Registered services for Scheduling functionalities
            services.AddHostedService<PAS.Services.BackgroundTask.BackgroundService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            // Core Services
            services.AddTransient<IMsGraphService, MsGraphService>();
            services.AddTransient<IHomeService, HomeService>();
            services.AddTransient<IStoryService, StoryService>();
            services.AddTransient<IStorySharepointService, StorySharepointService>();
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<ITasksSecurityService, TasksSecurityService>();
            services.AddTransient<ICheckListService, CheckListService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IEnumService, EnumService>();
            services.AddTransient<IStorySecurityService, StorySecurityService>();
            services.AddTransient<IPersonalNoteSecurityService, PersonalNoteSecurityService>();
            services.AddTransient<ICareerPathService, CareerPathService>();
            services.AddTransient<ICareerPathSecurityService, CareerPathSecurityService>();
            services.AddTransient<IPersonalNoteService, PersonalNoteService>();
            services.AddTransient<IPersonalNoteSharepointService, PersonalNoteSharepointService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<ITaskFollowerService, TaskFollowerService>();
            services.AddTransient<IProjectServices, ProjectServices>();
            services.AddTransient<ICriterionServices, CriterionServices>();
            services.AddTransient<IOptionListService, OptionListService>();
            services.AddTransient<IPerformanceReviewService, PerformanceReviewService>();
            services.AddTransient<IEvaluationReviewOptionsService, EvaluationReviewOptionsService>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<IProfileItemService, ProfileItemService>();
            services.AddTransient<ILookupProfileService, LookupProfileService>();
            services.AddTransient<IResumeService, ResumeService>();
            services.AddTransient<ITemplateService, TemplateService>();
            services.AddTransient<IPortfolioFileService, PortfolioFileService>();
            services.AddTransient<IResumeHighlightedSkillCategoryService, ResumeHighlightedSkillCategoryService>();
            services.AddTransient<IResumeHighlightedSkillService, ResumeHighlightedSkillService>();
            services.AddTransient<ISharepointFileService, SharepointFileService>();
            services.AddTransient<IPermissionService, PermissionService>();
            services.AddTransient<ISPUserService, SPUserService>();
            services.AddTransient<IUserCoachingService, UserCoachingService>();
            services.AddTransient<IKudoService, KudoService>();
            services.AddTransient<IStatisticsService, StatisticsService>();
            services.AddTransient<IMetadataService, MetadataService>();
            services.AddTransient<ITeamService, TeamService>();
            services.AddTransient<ITeamMemberService, TeamMemberService>();
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IDepartmentService, DepartmentService>();
            services.AddTransient<IWorkHistoryService, WorkHistoryService>();

            //services.AddScoped<IApplicationContext, ApplicationContext>();
            //services.AddScoped<IAuthProvider, AuthProvider>();
            //services.AddScoped<IDistributedCache, DistributedRedisCache>();
            //services.AddScoped<ISharepointContextProvider, SharepointContextProvider>();

            // Core Mappers
            services.AddTransient<PAS.Repositories.Mapping.IUserMapper, PAS.Repositories.Mapping.UserMapper>();
            services.AddTransient<PAS.Repositories.Mapping.IStoryMapper, PAS.Repositories.Mapping.StoryMapper>();
            services.AddTransient<PAS.Repositories.Mapping.ITaskMapper, PAS.Repositories.Mapping.TaskMapper>();
            services.AddTransient<PAS.Repositories.Mapping.ICheckListItemMapper, PAS.Repositories.Mapping.CheckListItemMapper>();
            services.AddTransient<PAS.Repositories.Mapping.IStoryMapper, PAS.Repositories.Mapping.StoryMapper>();
            services.AddTransient<PAS.Repositories.Mapping.IPersonalNoteMapper, PAS.Repositories.Mapping.PersonalNoteMapper>();
            services.AddTransient<PAS.Repositories.Mapping.ICareerPathMapper, PAS.Repositories.Mapping.CareerPathMapper>();
            services.AddTransient<PAS.Repositories.Mapping.ICareerPathStepMapper, PAS.Repositories.Mapping.CareerPathStepMapper>();
            services.AddTransient<PAS.Repositories.Mapping.ICareerPathTemplateMapper, PAS.Repositories.Mapping.CareerPathTemplateMapper>();
            services.AddTransient<PAS.Repositories.Mapping.ICareerPathTemplateStepMapper, PAS.Repositories.Mapping.CareerPathTemplateStepMapper>();
            services.AddTransient<PAS.Repositories.Mapping.ICommentMapper, PAS.Repositories.Mapping.CommentMapper>();
            services.AddTransient<PAS.Repositories.Mapping.ITaskUnfollowerMapper, PAS.Repositories.Mapping.TaskUnfollowerMapper>();
            services.AddTransient<PAS.Repositories.Mapping.IProjectMapper, PAS.Repositories.Mapping.ProjectMapper>();
            services.AddTransient<PAS.Repositories.Mapping.IPerformanceReviewPeriodMapper, PAS.Repositories.Mapping.PerformanceReviewPeriodMapper>();
            services.AddTransient<PAS.Repositories.Mapping.IUserPerformanceReviewsMapper, PAS.Repositories.Mapping.UserPerformanceReviewsMapper>();
            services.AddTransient<PAS.Repositories.Mapping.IProjectsAndUsersMapper, PAS.Repositories.Mapping.ProjectsAndUsersMapper>();
            services.AddTransient<ICriteriaMapper, CriteriaMapper>();
            services.AddTransient<ICriteriaGroupMapper, CriteriaGroupMapper>();
            services.AddTransient<ICriteriaOptionsMapper, CriteriaOptionsMapper>();
            services.AddTransient<ICriteriaOptionsListMapper, CriteriaOptionsListMapper>();
            services.AddTransient<IPerformanceReviewAndUserResultMapper, PerformanceReviewAndUserResultMapper>();
            services.AddTransient<IEvaluationReviewOptionsMapper, EvaluationReviewOptionsMapper>();
            services.AddTransient<IEvaluationReviewOptionMapper, EvaluationReviewOptionMapper>();
            services.AddTransient<IProjectCriterionMapper, ProjectCriterionMapper>();
            services.AddTransient<IProfileCategoryMapper, ProfileCategoryMapper>();
            services.AddTransient<IProfileItemMapper, ProfileItemMapper>();
            services.AddTransient<IPrecioFishboneWorkExperienceMapper, PrecioFishboneWorkExperienceMapper>();
            services.AddTransient<ISkillMapper, SkillMapper>();
            services.AddTransient<IGenericItemMapper, GenericItemMapper>();
            services.AddTransient<IPortfolioFileMapper, PortfolioFileMapper>();
            services.AddTransient<IResumeMapper, ResumeMapper>();
            services.AddTransient<ITemplateMapper, TemplateMapper>();
            services.AddTransient<ITemplateProfileCategoryMapper, TemplateProfileCategoryMapper>();
            services.AddTransient<ISkillCategoryMapper, SkillCategoryMapper>();
            services.AddTransient<IResumeHighlightedSkillCategoryMapper, ResumeHighlightedSkillCategoryMapper>();
            services.AddTransient<IResumeHighlightedSkillMapper, ResumeHighlightedSkillMapper>();
            services.AddTransient<IPersonLookupMapper, PersonLookupMapper>();
            services.AddTransient<IResignationTypeMapper, ResignationTypeMapper>();
            services.AddTransient<IUserCoachingMapper, UserCoachingMapper>();
            services.AddTransient<IKudoMapper, KudoMapper>();
            services.AddTransient(typeof(IMetadataMapper<>), typeof(MetadataMapper<>));
            services.AddTransient<ITeamMapper, TeamMapper>();
            services.AddTransient<ITeamMemberMapper, TeamMemberMapper>();
            services.AddTransient<IDepartmentMapper, DepartmentMapper>();
            services.AddTransient<IConfigurationMapper, ConfigurationMapper>();

            services.AddTransient<PAS.Model.Mapping.ISharePointInfoDtoMapper, PAS.Model.Mapping.SharePointInfoDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IStoryDtoMapper, PAS.Model.Mapping.StoryDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ITaskDtoMapper, PAS.Model.Mapping.TaskDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ICheckListDtoMapper, PAS.Model.Mapping.CheckListDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IUserDtoMapper, PAS.Model.Mapping.UserDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IPersonalNoteDtoMapper, PAS.Model.Mapping.PersonalNoteDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ICommentDtoMapper, PAS.Model.Mapping.CommentDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IProjectDtoMapper, PAS.Model.Mapping.ProjectDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ICriteriaGroupDtoMapper, PAS.Model.Mapping.CriteriaGroupDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ICriteriaDtoMapper, PAS.Model.Mapping.CriteriaDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ICriteriaOptionMapper, PAS.Model.Mapping.CriteriaOptionMapper>();
            services.AddTransient<PAS.Model.Mapping.ICriteriaOptionListDtoMapper, PAS.Model.Mapping.CriteriaOptionListDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IPerformanceReviewPeriodDtoMapper, PAS.Model.Mapping.PerformanceReviewPeriodDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IPerformanceReviewResultDtoMapper, PAS.Model.Mapping.PerformanceReviewResultDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IEvaluationReviewOptionsDtoMapper, PAS.Model.Mapping.EvaluationReviewOptionsDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IProfileCategoryDtoMapper, PAS.Model.Mapping.ProfileCategoryDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IProfileItemDtoMapper, PAS.Model.Mapping.ProfileItemDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ISkillDtoMapper, PAS.Model.Mapping.SkillDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IGenericItemDtoMapper, PAS.Model.Mapping.GenericItemDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IPortfolioFileDtoMapper, PAS.Model.Mapping.PortfolioFileDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IResumeDtoMapper, PAS.Model.Mapping.ResumeDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ITemplateDtoMapper, PAS.Model.Mapping.TemplateDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ITemplateProfileCategoryDtoMapper, PAS.Model.Mapping.TemplateProfileCategoryDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ISharepointImageDtoMapper, PAS.Model.Mapping.SharepointImageDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ISkillCategoryDtoMapper, PAS.Model.Mapping.SkillCategoryDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IResumeHighlightedSkillCategoryDtoMapper, PAS.Model.Mapping.ResumeHighlightedSkillCategoryDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IResumeHighlightedSkillDtoMapper, PAS.Model.Mapping.ResumeHighlightedSkillDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IResumeHighlightedSkillCategoryRequestDtoMapper, PAS.Model.Mapping.ResumeHighlightedSkillCategoryRequestDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ICompanySkillDtoMapper, PAS.Model.Mapping.CompanySkillDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ITeamDtoMapper, PAS.Model.Mapping.TeamDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.ITeamMembersDtoMapper, PAS.Model.Mapping.TeamMembersDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IResignationTypeDtoMapper, PAS.Model.Mapping.ResignationTypeDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IGroupDtoMapper, PAS.Model.Mapping.GroupDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IGenderDtoMapper, PAS.Model.Mapping.GenderDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IUserCoachingDtoMapper, PAS.Model.Mapping.UserCoachingDtoMapper>();
            services.AddTransient<Model.Mapping.IKudoDtoMapper, Model.Mapping.KudoDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IMetadataDtoMapper, PAS.Model.Mapping.MetadataDtoMapper>();
            services.AddTransient<PAS.Model.Mapping.IDepartmentDtoMapper, PAS.Model.Mapping.DepartmentDtoMapper>();

            // Core Repositories
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IStoryRepository, StoryRepository>();
            services.AddTransient<ITaskRepository, TaskRepository>();
            services.AddTransient<ICheckListItemRepository, CheckListItemRepository>();
            services.AddTransient<IPersonalNoteRepository, PersonalNoteRepository>();
            services.AddTransient<ICareerPathRepository, CareerPathRepository>();
            services.AddTransient<ICommentRepository, CommentRepository>();
            services.AddTransient<ITaskUnfollowerRepository, TaskUnfollowerRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IPerformanceReviewPeriodRepository, PerformanceReviewPeriodRepository>();
            services.AddTransient<ICriteriaGroupRepository, CriteriaGroupRepository>();
            services.AddTransient<ICriteriaOptionsListRepository, CriteriaOptionListRepository>();
            services.AddTransient<IEvaluationReviewOptionsRepository, EvaluationReviewOptionsRepository>();
            services.AddTransient<ICriteriaRepository, CriteriaRepository>();
            services.AddTransient<IProfileRepository, ProfileRepository>();
            services.AddTransient<IProfileItemRepository, ProfileItemRepository>();
            services.AddTransient<ILookupProfileRepository, LookupProfileRepository>();
            services.AddTransient<IResumeRepository, ResumeRepository>();
            services.AddTransient<ITemplateRepository, TemplateRepository>();
            services.AddTransient<IPortfolioFileRepository, PortfolioFileRepository>();
            services.AddTransient<IResumeHighlightedSkillCategoryRepository, ResumeHighlightedSkillCategoryRepository>();
            services.AddTransient<IResumeHighlightedSkillRepository, ResumeHighlightedSkillRepository>();
            services.AddTransient<IUserCoachingRepository, UserCoachingRepository>();
            services.AddTransient<IKudoRepository, KudoRepository>();
            services.AddTransient<IStatisticsRepository, StatisticsRepository>();
            services.AddTransient(typeof(IMetadataRepository<>), typeof(MetadataRepository<>));
            services.AddTransient<ITeamRepository, TeamRepository>();
            services.AddTransient<ITeamMemberRepository, TeamMemberRepository>();
            services.AddTransient<IConfigurationRepository, ConfigurationRepository>();
            services.AddTransient<IDepartmentRepository, DepartmentRepository>();
            services.AddTransient<IRecordRepository<Model.WorkRecord>, WorkRecordRepository>();

            services.AddTransient<IUserLeaveTrackService, UserLeaveTrackService>();
            services.AddTransient<IUserLeaveYearService, UserLeaveYearService>();
            services.AddTransient<ILeaveTypeService, LeaveTypeService>();
            services.AddTransient<ILeaveService, LeaveService>();


            services.AddScoped<IApplicationContext, ApplicationContext>();

            services.AddScoped<IAuthProvider, AuthProvider>();
            services.AddScoped<IDistributedCache>(x => new DistributedRedisCache(ConfigurationOptions.Parse(configuration.GetSection("RedisConnectionString").Value), "PAS"));
            services.AddScoped<ISharepointContextProvider, SharepointContextProvider>();

            services.AddScoped<IHRMUnitOfWork, HRMUnitOfWork>();

            // HRM
            services.AddTransient<IHolidayService, HolidayService>();

            return services;
        }
    }
}
