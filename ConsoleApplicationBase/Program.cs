using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Module2.ClassLibrary.EarlyBound;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplicationBase
{
    class Program
    {
        static void Main(string[] args)
        {
            IOrganizationService service = GetOrgService();
            Console.WriteLine(service.ToString());
            Console.WriteLine("Connected...");

            //LabCreatingLead(service);
            //LabAccountQuery(service);
            //LabContactQuery(service);
            //LabUsingLinqCreateContacts(service);
            //LabUsingLinqUpdatingContacts(service);
            //LabUsingLinqDeleteContacts(service);
            LabUsingFetchXml(service);

            Console.WriteLine("Press Return to end...");
            Console.ReadLine();
        }

        private static void LabCreatingLead(IOrganizationService service)
        {
            var lead = new Entity("lead");
            lead["subject"] = "Terry's lead";
            lead["firstname"] = "Terry";
            lead["lastname"] = "Maguire";
            lead["companyname"] = "Avnet";
            lead["numberofemployees"] = (int)3500;
            lead["revenue"] = (decimal)1000000000;

            Guid leadId = service.Create(lead);

            Console.WriteLine("Lead created for {0} {1}",
                                lead["firstname"], lead["lastname"]);

        }

        private static void LabAccountQuery(IOrganizationService service)
        {
            var qe = new QueryExpression();
            var cs = new ColumnSet();
            var fe = new FilterExpression();
            var ce = new ConditionExpression();

            cs.AddColumns("name", "accountid");

            ce.AttributeName = "statecode";
            ce.Operator = ConditionOperator.Equal;
            ce.Values.Add("Active");

            fe.AddCondition(ce);

            qe.ColumnSet = cs;
            qe.Criteria = fe;
            qe.EntityName = "account";
            qe.AddOrder("name", OrderType.Ascending);

            var accountList = service.RetrieveMultiple(qe);

            foreach (var act in accountList.Entities)
            {
                Console.WriteLine("Name: {0}, Account Id: {1}", act.Attributes["name"], act.Attributes["accountid"]);
            }
        }

        private static void LabContactQuery(IOrganizationService service)
        {
            var qe = new QueryExpression("contact");
            var cs = new ColumnSet();
            var fe = new FilterExpression();
            var ce = new ConditionExpression();

            cs.AddColumns("fullname", "parentcustomerid");

            ce.AttributeName = "jobtitle";
            ce.Operator = ConditionOperator.Equal;
            ce.Values.Add("Purchasing Manager");

            fe.AddCondition(ce);

            qe.ColumnSet = cs;
            qe.Criteria.AddFilter(fe);

            var contacts = service.RetrieveMultiple(qe);

            foreach(var a in contacts.Entities)
            {
                Console.WriteLine(a.Attributes["fullname"]);
            }
        }

        private static void LabContactQueryByAttribute(IOrganizationService service)
        {
            var queryByExpression = new QueryByAttribute("account");
            queryByExpression.ColumnSet = new ColumnSet("name", "address1_city", "emailaddress1");

            queryByExpression.Attributes.AddRange("address1_city");

            queryByExpression.Values.AddRange("Dallas");

            var result = service.RetrieveMultiple(queryByExpression);

            foreach (var item in result.Entities)
            {
                Console.WriteLine("Name: {0}, Email Address: {1}", item.Attributes["name"], item.Attributes["emailaddress1"]);
            }
        }

        private static void LabUsingLinqCreateContacts(IOrganizationService service)
        {
            using (var orgContext = new ServiceContext(service))
            {
                var contact = new Contact()
                {
                    FirstName = "Alan",
                    LastName = "Smith"
                };

                orgContext.AddObject(contact);

                var contact2 = new Contact()
                {
                    FirstName = "Ben",
                    LastName = "Andrews"
                };
                orgContext.AddObject(contact2);

                var contact3 = new Contact()
                {
                    FirstName = "Colin",
                    LastName = "Wilcox"
                };
                orgContext.AddObject(contact3);

                orgContext.SaveChanges();
            }
        }

        private static void LabUsingLinqUpdatingContacts(IOrganizationService service)
        {
            using (var orgContext = new ServiceContext(service))
            {
                var qpm = from c in orgContext.ContactSet
                          where (c.JobTitle == null)
                          select c;

                foreach (var a in qpm)
                {
                    a.JobTitle = "Purchasing Supervisor";
                    orgContext.UpdateObject(a);

                    Console.WriteLine("{0}'s job title has been updated to Purchasin Supervisor", a["fullname"]);
                }

                orgContext.SaveChanges();
            }
        }

        private static void LabUsingLinqDeleteContacts(IOrganizationService service)
        {
            using (var orgContext = new ServiceContext(service))
            {
                var del = from c in orgContext.ContactSet
                          where (c.JobTitle == "Purchasing Supervisor")
                          select c;

                foreach (var a in del)
                {
                    orgContext.DeleteObject(a);
                }

                orgContext.SaveChanges();
            }
        }

        private static void LabUsingFetchXml(IOrganizationService service)
        {
            var fetchXml = @"<fetch mapping='logical'>
                             <entity name='contact'>
                             <attribute name='fullname'/>
                             <filter type='and'>
                             <condition attribute='jobtitle' operator='eq' value='Purchasing Assistant'/>
                             </filter>
                             </entity>
                             </fetch>";

            var contactList = (EntityCollection)service.RetrieveMultiple(
                                    new FetchExpression(fetchXml));

            foreach (var item in contactList.Entities)
            {
                Console.WriteLine(item["fullname"]);
                    
            }

        }

        private static IOrganizationService GetOrgService()
        {
            string connStr = ConfigurationManager.ConnectionStrings[1].ConnectionString;
            CrmServiceClient conn = new CrmServiceClient(connStr);

            return (IOrganizationService)conn.OrganizationServiceProxy;
        }
    }
}
