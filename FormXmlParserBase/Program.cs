using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Xml.Linq;

namespace FormXmlParserBase
{
    class Program
    {
        static void Main(string[] args)
        {
            IOrganizationService service = GetOrgService();

            GetConnectionDetails(service);

            var formxml = RetrieveFormXml(service, "account");
            var attributes = GetEntityAttributes(formxml);
            var sections = GetSections(formxml);
            var tabs = GetTabs(formxml);
            var navs = GetNavigtionItems(formxml);

            Console.WriteLine("Press Return to end...");
            Console.ReadLine();
        }

        private static IEnumerable<string> GetEntityAttributes(string xml)
        {
            XElement form = XElement.Parse(xml);
            return
               from elem in form.Descendants("control")
               select 
                elem.Attribute("id").Value;
        }

        private static IEnumerable<String> GetNavigtionItems(String xml)
        {
            XElement form = XElement.Parse(xml);
            return
               from elem in form.Descendants("Navigation")
                                .Descendants("NavBar")
                                .Descendants("NavBarRelationshipItem")
               select elem.Attribute("Id").Value;

        }
        private static IEnumerable<String> GetTabs(String xml)
        {
            XElement form = XElement.Parse(xml);
            return
               from elem in form.Descendants("tabs")
                                .Descendants("tab")
               select elem.Attribute("name").Value;

        }
        private static IEnumerable<String> GetSections(String xml)
        {
            XElement form = XElement.Parse(xml);
            return
               from elem in form.Descendants("sections")
                                 .Descendants("section")
               select elem.Attribute("name").Value;

        }

        private static string RetrieveFormXml(IOrganizationService service, string entityName)
        {
            var request = new RetrieveEntityRequest();
            request.EntityFilters = EntityFilters.Entity;
            request.LogicalName = entityName;

            var response = (RetrieveEntityResponse)service.Execute(request);
            var typecode = response.EntityMetadata.ObjectTypeCode.Value;

            var query = new QueryExpression("systemform");
            query.ColumnSet = new ColumnSet("formxml");
            query.Criteria.AddCondition(new ConditionExpression("type", ConditionOperator.Equal, 2));
            query.Criteria.AddCondition(new ConditionExpression("objecttypecode", ConditionOperator.Equal, typecode));

            var multiRequest = new RetrieveMultipleRequest();
            multiRequest.Query = query;

            var results = (RetrieveMultipleResponse)service.Execute(multiRequest);
            return results.EntityCollection.Entities.FirstOrDefault().Attributes["formxml"].ToString();
        }


        private static IOrganizationService GetOrgService()
        {
            try
            {

                string connStr = ConfigurationManager.ConnectionStrings[1].ConnectionString;
                CrmServiceClient conn = new CrmServiceClient(connStr);

                return (IOrganizationService)conn.OrganizationServiceProxy;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp);
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode);
                Console.WriteLine("Message: {0}", ex.Detail.Message);
                Console.WriteLine("Trace: {0}", ex.Detail.TraceText);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
            }
            catch (System.TimeoutException ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Message: {0}", ex.Message);
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.InnerException.Message ? "No Inner Fault" : ex.InnerException.Message);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine(ex.Message);

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);

                    FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe = ex.InnerException
                        as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                    if (fe != null)
                    {
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp);
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode);
                        Console.WriteLine("Message: {0}", fe.Detail.Message);
                        Console.WriteLine("Trace: {0}", fe.Detail.TraceText);
                        Console.WriteLine("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }

            // TODO: Other exception messages, need generic handler
            // Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            // SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

            return null;
        }

        private static void GetConnectionDetails(IOrganizationService service)
        {
            if (service != null)
            {
                
                // Obtain information about the logged on user from the web service.
                Guid userid = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;
                //SystemUser systemUser = (SystemUser)service.Retrieve("systemuser", userid,
                //    new ColumnSet(new string[] { "firstname", "lastname" }));
                var systemUser = (Entity)service.Retrieve("systemuser", userid,
                    new ColumnSet(new string[] { "firstname", "lastname" }));
                Console.WriteLine("Connected...");
                Console.WriteLine("Logged on user is {0} {1}.", systemUser.Attributes["firstname"], systemUser.Attributes["lastname"]);

                // Retrieve the version of Microsoft Dynamics CRM.
                RetrieveVersionRequest versionRequest = new RetrieveVersionRequest();
                RetrieveVersionResponse versionResponse =
                    (RetrieveVersionResponse)service.Execute(versionRequest);
                Console.WriteLine("Microsoft Dynamics CRM version {0}.", versionResponse.Version);
            }
            else
            {
                Console.WriteLine("Connection failed!");
            }
        }
    }
}
