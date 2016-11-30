using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginBase
{
    public class GenerateLeadAutoNumber : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                var lastNumber = GetLastNumber(service);

                var lead = (Entity)context.InputParameters["Target"];
                lead["new_autonumber"] = lastNumber + 1;
            }
        }


        private int GetLastNumber(IOrganizationService service)
        {
            var query = new QueryExpression("lead");
            query.ColumnSet = new ColumnSet("new_autonumber");
            query.Criteria.AddCondition("new_autonumber", ConditionOperator.NotNull);
            query.AddOrder("new_autonumber", OrderType.Descending);

            var leads = service.RetrieveMultiple(query);

            if (leads.Entities.Count > 0)
                return (int)leads.Entities[0]["new_autonumber"];

            return 0;
        }


    }
}
