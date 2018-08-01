using System;
using System.Linq;
using AutoMapper;
using StarChef.Orchestrate.Models.TransferObjects;

using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;
using AccountStatus = Fourth.Orchestration.Model.People.Events.AccountStatus;

namespace StarChef.Listener
{
    class ConfigObjectMapping
    {
        private static readonly object _lock = new object();
        private static bool IsInitialized = false;

        internal static void Init()
        {
            lock (_lock)
            {
                if (!IsInitialized)
                {
                    Mapper.Initialize(c =>
                    {
                        #region AccountCreated => AccountCreatedTransferObject
                        c.CreateMap<AccountCreated, AccountCreatedTransferObject>()
                                        .ForMember(dest => dest.LoginId, o => o.MapFrom(src => int.Parse(src.InternalId)))
                                        .ForMember(dest => dest.FirstName, o => o.MapFrom(src => src.FirstName))
                                        .ForMember(dest => dest.LastName, o => o.MapFrom(src => src.LastName))
                                        .ForMember(dest => dest.EmailAddress, o => o.MapFrom(src => src.EmailAddress))
                                        .ForMember(dest => dest.ExternalLoginId, o => o.MapFrom(src => src.ExternalId))
                                        .ForMember(dest => dest.Username, o => o.MapFrom(src => src.Username))
                                        .ForMember(dest => dest.ExternalCustomerId, o => o.MapFrom(src => src.CustomerCanonicalId))
                                        .ForMember(dest => dest.PermissionSets, o => o.MapFrom(src => src.PermissionSetsList))
                                        .ForAllOtherMembers(m => m.Ignore());
                        #endregion

                        #region AccountCreateFailed => AccountCreateFailedTransferObject
                        c.CreateMap<AccountCreateFailed, AccountCreateFailedTransferObject>()
                                        .ForMember(dest => dest.LoginId, o => o.MapFrom(src => int.Parse(src.InternalId)))
                                        .ForMember(dest => dest.ErrorCode, o => o.MapFrom(src => src.Reason))
                                        .ForMember(dest => dest.Description, o => o.MapFrom(src => src.DetailsCount > 0 ? src.DetailsList.Aggregate((s, s1) => s + ",[" + s1 + "]") : null))
                                        .ForAllOtherMembers(m => m.Ignore());
                        #endregion

                        #region AccountUpdated => AccountUpdatedTransferObject
                        c.CreateMap<AccountUpdated, AccountUpdatedTransferObject>()
                                        .ForMember(dest => dest.Username, o => o.MapFrom(src => src.Username))
                                        .ForMember(dest => dest.FirstName, o => o.MapFrom(src => src.FirstName))
                                        .ForMember(dest => dest.LastName, o => o.MapFrom(src => src.LastName))
                                        .ForMember(dest => dest.EmailAddress, o => o.MapFrom(src => src.EmailAddress))
                                        .ForMember(dest => dest.ExternalLoginId, o => o.MapFrom(src => src.ExternalId))
                                        .ForMember(dest => dest.PermissionSets, o => o.MapFrom(src => src.PermissionSetsList))
                                        .ForAllOtherMembers(m => m.Ignore());
                        #endregion

                        #region AccountUpdateFailed => AccountUpdateFailedTransferObject
                        c.CreateMap<AccountUpdateFailed, AccountUpdateFailedTransferObject>()
                                        .ForMember(dest => dest.ExternalLoginId, o => o.MapFrom(src => src.ExternalId))
                                        .ForMember(dest => dest.ErrorCode, o => o.MapFrom(src => src.Reason))
                                        .ForMember(dest => dest.Description, o => o.MapFrom(src => src.DetailsCount > 0 ? src.DetailsList.Aggregate((s, s1) => s + ",[" + s1 + "]") : null))
                                        .ForAllOtherMembers(m => m.Ignore());
                        #endregion

                        #region AccountStatusChanged => AccountStatusChangedTransferObject
                        c.CreateMap<AccountStatusChanged, AccountStatusChangedTransferObject>()
                                        .ForMember(dest => dest.ExternalLoginId, o => o.MapFrom(src => src.ExternalId))
                                        .ForMember(dest => dest.IsActive, o => o.MapFrom(src => src.Status == AccountStatus.ACTIVE))
                                        .ForAllOtherMembers(m => m.Ignore());
                        #endregion

                        #region AccountStatusChangeFailed => AccountStatusChangeFailedTransferObject
                        c.CreateMap<AccountStatusChangeFailed, AccountStatusChangeFailedTransferObject>()
                                        .ForMember(dest => dest.ExternalLoginId, o => o.MapFrom(src => src.ExternalId))
                                        .ForMember(dest => dest.Description, o =>
                                        {
                                            o.Condition(src => src.DetailsCount > 0);
                                            o.MapFrom(src => src.DetailsList.Aggregate((s, s1) => s + ",[" + s1 + "]"));
                                        })
                                        .ForAllOtherMembers(m => m.Ignore());
                        #endregion
                    });

                    IsInitialized = true;
                }
            }
        }
    }
}
