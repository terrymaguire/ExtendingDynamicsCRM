using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace CustomWorkflowActivityBase
{
    public class ExampleWorkflow : CodeActivity
    {
        [Output("User")]
        [ReferenceTarget("systemuser")]
        public InOutArgument<EntityReference> User { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            var user = GetLastModifiedUser(service);
            User.Set(executionContext, user.ToEntityReference());

        }

        private Entity GetLastModifiedUser(IOrganizationService service)
        {
            var query = new QueryExpression("systemuser");
            query.ColumnSet = new ColumnSet("systemuserid");
            query.AddOrder("modifiedon", OrderType.Descending);

            var users = service.RetrieveMultiple(query);

            if (users.Entities.Count > 0)
                return users.Entities[0];

            return null;
        }
    }
}
