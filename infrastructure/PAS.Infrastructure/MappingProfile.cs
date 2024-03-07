using DocumentFormat.OpenXml.Wordprocessing;
using PAS.Model.HRM;
using System.Collections.Generic;

namespace PAS.Infrastructure
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<PAS.Repositories.DataModel.User, PAS.Model.User>()
                //.ForMember(x => x.Role, opt => opt.MapFrom(src => src.Role))
                .ReverseMap();
            CreateMap<PAS.Repositories.DataModel.User, PAS.Model.HRM.User>()
                .ForMember(x => x.DisplayName, opt => opt.MapFrom(src => src.Name))
                .ReverseMap();
                //.ForMember(x => x.Avatar, opt => opt.MapFrom())
                //.ForMember(x => x.JobTitle, opt => opt.Ignore())
                //.ForMember(x => x.OfficeLocation, opt => opt.Ignore())
                //.ForMember(x => x.UserLeaveTracks, opt => opt.Ignore())
                //.ForMember(x => x.GroupDetails, opt => opt.Ignore())
                //.ForMember(x => x.FilePassword, opt => opt.Ignore())
                //.ForAllMembers(opts => opts.Ignore());
            CreateMap<PAS.Repositories.DataModel.Holiday, Holiday>().ReverseMap();
            CreateMap<PAS.Repositories.DataModel.UserLeaveTrack, UserLeaveTrack>();
            CreateMap<PAS.Repositories.DataModel.Holiday, Holiday>().ReverseMap();
            CreateMap<PAS.Repositories.DataModel.LeaveType, LeaveType>().ReverseMap(); ;
            CreateMap<PAS.Repositories.DataModel.LeaveTypeLocalization, LeaveTypeLocalization>().ReverseMap();
            CreateMap<PAS.Repositories.DataModel.LeaveTypeParam, LeaveTypeParam>().ReverseMap(); ;
            CreateMap<PAS.Repositories.DataModel.LeaveRequestHistory, LeaveRequestHistory>().ReverseMap(); ;
            CreateMap<PAS.Repositories.DataModel.UserLeaveTrack, UserLeaveTrack>().ReverseMap(); ;
            CreateMap<PAS.Repositories.DataModel.UserLeaveTrackHistory, UserLeaveTrackHistory>().ReverseMap();
            CreateMap<PAS.Repositories.DataModel.UserLeaveYear, UserLeaveYear>().ReverseMap(); ;
            CreateMap<PAS.Repositories.DataModel.HolidayLocalization, HolidayLocalization>().ReverseMap(); ;
            CreateMap<PAS.Repositories.DataModel.LeaveRequest, LeaveRequest>().ReverseMap();
            CreateMap<PAS.Repositories.DataModel.LeaveRequestApprover, LeaveRequestApprover>().ReverseMap();
        }
    }
}
