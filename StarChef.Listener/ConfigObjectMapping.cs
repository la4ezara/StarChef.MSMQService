using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                            .ForMember(dest => dest.Id, o =>
                            {
                                o.Condition(src => src.HasInternalId);
                                o.MapFrom(src => src.InternalId);
                            })
                            .ForMember(dest => dest.FirstName, o =>
                            {
                                o.Condition(src => src.HasFirstName);
                                o.MapFrom(src => src.FirstName);
                            })
                            .ForMember(dest => dest.LastName, o =>
                            {
                                o.Condition(src => src.HasLastName);
                                o.MapFrom(src => src.LastName);
                            })
                            .ForMember(dest => dest.EmailAddress, o =>
                            {
                                o.Condition(src => src.HasEmailAddress);
                                o.MapFrom(src => src.EmailAddress);
                            })
                            .ForMember(dest => dest.OrganizationId, o =>
                            {
                                o.Condition(src => src.HasExternalId);
                                o.MapFrom(src => src.ExternalId);
                            })
                            .ForAllOtherMembers(m => m.Ignore());
                #endregion

                #region AccountUpdated => UserTransferObject
                c.CreateMap<AccountUpdated, UserTransferObject>()
                            .ForMember(dest => dest.FirstName, o =>
                            {
                                o.Condition(src => src.HasFirstName);
                                o.MapFrom(src => src.FirstName);
                            })
                            .ForMember(dest => dest.LastName, o =>
                            {
                                o.Condition(src => src.HasLastName);
                                o.MapFrom(src => src.LastName);
                            })
                            .ForMember(dest => dest.EmailAddress, o =>
                            {
                                o.Condition(src => src.HasEmailAddress);
                                o.MapFrom(src => src.EmailAddress);
                            })
                            .ForMember(dest => dest.OrganizationId, o =>
                            {
                                o.Condition(src => src.HasExternalId);
                                o.MapFrom(src => src.ExternalId);
                            })
                            .ForAllOtherMembers(m => m.Ignore());
                #endregion
            });
        }
    }
}
