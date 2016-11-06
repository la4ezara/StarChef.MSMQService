using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;

namespace StarChef.Listener.Handlers
{
    public class AccountCreatedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountCreated>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountCreatedEventHandler()
        {
        }

        public AccountCreatedEventHandler(IDatabaseCommands dbCommands) : base(dbCommands)
        {
        }

        public Task<MessageHandlerResult> HandleAsync(Events.AccountCreated payload, string trackingId)
        {
            if (payload.Source == Events.SourceSystem.STARCHEF)
            {
                var loginId = payload.InternalId;
            }
            

            throw new NotImplementedException();
            /*
             * for new user these checks are executed 
             * 
exec sc_admin_check_newpassword @db_id=224,@user_id=0,@password=N'1234'
go
exec sc_admin_is_duplicate_login_name @loginName=N'asd@dfg.net'
go
             * 
             * when existing user is saved
             * 
declare @p1 int
set @p1=439
exec sc_admin_save_preferences @user_id=@p1 output,@email=N'alexander.goida@fourth.com',@login_name=N'aprovU2',@forename=N'aprovU2',@lastname=N'aprovU2',@user_desc=N'',@user_notes=N'',@contact_number=N'',@ugroup_id=4,@language_id=1,@unit_family_id=-1,@user_config=default,@is_enabled=1,@is_deleted=0,@is_anonymous=0,@upicture_id=0,@upicture_location=NULL,@width=0,@height=0,@set_default=1,@nutrition_type_id=1
select @p1

declare @p1 int
set @p1=19933
exec sc_admin_update_login @login_id=@p1 output,@login_name=N'aprovU2',@db_application_id=1,@db_database_id=224,@user_id=439,@db_role_id=1,@login_password=NULL,@login_config=N'0',@is_enabled=1,@is_deleted=0
select @p1

exec sc_admin_save_user_ugroups_xml @user_id=439,@xml=N'<uugroups>
<uugroup id="4" name="Cafe Metro - Admins" isdefault="1" /><uugroup id="18" name="Cafe Metro - Leeds" isdefault="0"/>
<uugroup id="204" name="Copy of apr2" isdefault="0"/>
</uugroups>
'

exec sc_get_orchestration_lookup @entity_type_id=10


1) save user data
2) update login data for user
3) save user groups
4) ?? for new user update orchestration lookup
             */
        }
    }
}
