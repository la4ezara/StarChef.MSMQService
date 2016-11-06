using System;
using System.Linq;
using AutoMapper;
using StarChef.Orchestrate.Models.TransferObjects;

using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;

namespace StarChef.Listener
{
    class ConfigObjectMapping
    {
        internal static void Init()
        {
            Mapper.Initialize(c =>
            {
                #region AccountCreated => UserTransferObject
                c.CreateMap<AccountCreated, UserTransferObject>()
                            .ForMember(dest => dest.Id, o => o.MapFrom(src => int.Parse(src.InternalId)))
                            .ForMember(dest => dest.FirstName, o => o.MapFrom(src => src.FirstName))
                            .ForMember(dest => dest.LastName, o => o.MapFrom(src => src.LastName))
                            .ForMember(dest => dest.EmailAddress, o => o.MapFrom(src => src.EmailAddress))
                            .ForMember(dest => dest.OrganizationId, o => o.MapFrom(src => Guid.Parse(src.ExternalId)))
                            .ForAllOtherMembers(m => m.Ignore());
                #endregion

                #region AccountCreateFailed => OperationFailedTransferObject
                c.CreateMap<AccountCreateFailed, OperationFailedTransferObject>()
                            .ForMember(dest => dest.UserId, o => o.MapFrom(src => src.InternalId))
                            .ForMember(dest => dest.ErrorCode, o => o.MapFrom(src => src.Reason))
                            .ForMember(dest => dest.Description, o => o.MapFrom(src => src.DetailsList.Aggregate((s, s1) => s + ",[" + s1 + "]")))
                            .ForAllOtherMembers(m => m.Ignore());
                #endregion

                #region AccountUpdated => UserTransferObject
                c.CreateMap<AccountUpdated, UserTransferObject>()
                            .ForMember(dest => dest.Username, o => o.MapFrom(src => src.Username))
                            .ForMember(dest => dest.FirstName, o => o.MapFrom(src => src.FirstName))
                            .ForMember(dest => dest.LastName, o => o.MapFrom(src => src.LastName))
                            .ForMember(dest => dest.EmailAddress, o => o.MapFrom(src => src.EmailAddress))
                            .ForMember(dest => dest.OrganizationId, o => o.MapFrom(src => Guid.Parse(src.ExternalId)))
                            .ForAllOtherMembers(m => m.Ignore());
                #endregion

                #region AccountUpdateFailed => OperationFailedTransferObject
                c.CreateMap<AccountUpdateFailed, OperationFailedTransferObject>()
                            .ForMember(dest => dest.UserId, o => o.MapFrom(src => src.ExternalId)) // todo is correct field?
                            .ForMember(dest => dest.ErrorCode, o => o.MapFrom(src => src.Reason))
                            .ForMember(dest => dest.Description, o => o.MapFrom(src => src.DetailsList.Aggregate((s, s1) => s + ",[" + s1 + "]")))
                            .ForAllOtherMembers(m => m.Ignore());
                #endregion
            });
        }
    }
}
