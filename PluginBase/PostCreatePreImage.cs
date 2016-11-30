using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginBase
{
    public class PostCreatePreImage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") &&
               context.InputParameters["Target"] is Entity)
            {
                var account = (Entity)context.InputParameters["Target"];
                var preImage = (Entity)context.PreEntityImages["AccountPreImage"];

                var oldValue = (string)preImage["accountnumber"];
                var newValue = (string)account["accountnumber"];

                var updateAccount = new Entity("account");
                updateAccount.Id = account.Id;
                updateAccount["new_accountnumberlog"] = oldValue + " - " + newValue;

                service.Update(updateAccount);
            }
        }
    }
}
