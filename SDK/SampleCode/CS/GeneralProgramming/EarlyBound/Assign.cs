// =====================================================================
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
//
// =====================================================================

//<snippetAssign>
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

// These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
// found in the SDK\bin folder
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages; 
using Microsoft.Xrm.Sdk.Client;  

namespace Microsoft.Crm.Sdk.Samples
{
	/// <summary>
	/// This Sample shows how to assign an account to another user.
    /// </summary>
    /// <remarks>
    /// Note: This sample expects another user to be present in 
    /// the current business unit other than the current credentials.
    /// </remarks>
    
	
	public class Assign
	{
        #region Class Level Members
        

        // Define the IDs needed for this sample.
        private Guid _accountId;   
        private Guid _myUserId;
        private Guid _otherUserId;



        // Declare the service proxy referring the CRUD
        private OrganizationServiceProxy _serviceProxy;
        private IOrganizationService _service;

        #endregion Class Level Members
        
        #region How To Sample Code
        /// <summary>
        /// Create and configure the organization service proxy.
        /// Retrieves new owner's details and creates an account record.
        /// Assign the account to new owner.
        /// Optionally delete any entity records that were created for this sample.
        // </summary>
        /// <param name="serverConfig">Contains server connection information.</param>
        /// <param name="promptForDelete">When True, the user will be prompted to delete all
        /// created entities.</param>
        public void Run(ServerConnection.Configuration serverConfig, bool promptForDelete)
        {
            try
            {

                // Connect to the Organization service. 
                // The using statement assures that the service proxy will be properly disposed.
                using (_serviceProxy = new OrganizationServiceProxy(serverConfig.OrganizationUri, serverConfig.HomeRealmUri,
                                                                     serverConfig.Credentials, serverConfig.DeviceCredentials))
                {
                    // This statement is required to enable early-bound type support.
                    _serviceProxy.EnableProxyTypes();

                    _service = (IOrganizationService)_serviceProxy;

                    // Call the method to create any data that this sample requires.
                    CreateRequiredRecords();

                    //<snippetAssign1>  
                    // Create the Request Object and Set the Request Object's Properties
                    AssignRequest assign = new AssignRequest
                        {
                            Assignee = new EntityReference(SystemUser.EntityLogicalName,
                                _otherUserId),
                            Target = new EntityReference(Account.EntityLogicalName,
                                _accountId)
                        };


                    // Execute the Request
                    _service.Execute(assign);
                    //</snippetAssign1>  

                    Console.WriteLine("The account is owned by new owner.");

                    DeleteRequiredRecords(promptForDelete);

                }
            }
            // Catch any service fault exceptions that Microsoft Dynamics CRM throws.
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
                // You can handle an exception here or pass it back to the calling method.
                throw;
            }
        }
        

        /// <summary>
        /// This method creates any entity records that this sample requires.
        /// Retrieves the user details.
        /// Create an account record.
        /// </summary>
        public void CreateRequiredRecords()
        {
            WhoAmIRequest userRequest = new WhoAmIRequest();
            WhoAmIResponse user = (WhoAmIResponse)_service.Execute(userRequest);

            // Current user.
            _myUserId = user.UserId;

            // Query to retrieve other users.
            QueryExpression querySystemUser = new QueryExpression
            {
                EntityName = SystemUser.EntityLogicalName,
                ColumnSet = new ColumnSet(new String[] { "systemuserid", "fullname" }),
                Criteria = new FilterExpression()
            };

            querySystemUser.Criteria.AddCondition("businessunitid", 
                ConditionOperator.Equal, user.BusinessUnitId);
            querySystemUser.Criteria.AddCondition("systemuserid", 
                ConditionOperator.NotEqual, _myUserId);
            // Excluding SYSTEM user.
            querySystemUser.Criteria.AddCondition("lastname", 
                ConditionOperator.NotEqual, "SYSTEM");
            // Excluding INTEGRATION user.
            querySystemUser.Criteria.AddCondition("lastname", 
                ConditionOperator.NotEqual, "INTEGRATION");

            DataCollection<Entity> otherUsers = _service.RetrieveMultiple(
                querySystemUser).Entities;

            int count = _service.RetrieveMultiple(querySystemUser).Entities.Count;
            if ( count > 0)
            {
                _otherUserId = (Guid)otherUsers[count-1].Attributes["systemuserid"];

                Console.WriteLine("Retrieved new owner {0} for assignment.",
                    otherUsers[count - 1].Attributes["fullname"]);
            }
            else
            {
                throw new FaultException(
                    "No other user found in the current business unit for assignment.");
            }

            // Create an Account record 
            Account newAccount = new Account
            {
                Name = "Example Account"
            };

            _accountId = _service.Create(newAccount);
            Console.WriteLine("Created {0}", newAccount.Name);

            return;
        }
        
        /// <summary>
        /// Deletes any entity records that were created for this sample.
        /// <param name="prompt">Indicates whether to prompt the user to delete 
        /// the records created in this sample.</param>
        /// </summary>
        public void DeleteRequiredRecords(bool prompt)
        {
            bool deleteRecords = true;

            if (prompt)
            {
                Console.WriteLine("\nDo you want these entity records deleted? (y/n) [y]: ");
                String answer = Console.ReadLine();

                deleteRecords = (answer.StartsWith("y") || answer.StartsWith("Y") || answer == String.Empty);
            }

            if (deleteRecords)
            {
                _service.Delete(Account.EntityLogicalName, _accountId);
                Console.WriteLine("Entity records have been deleted.");
            }
        }
      
        #endregion How To Sample Code

        #region Main
        /// <summary>
        /// Standard Main() method used by most SDK samples.
        /// </summary>
        /// <param name="args"></param>
        static public void Main(string[] args)
        {
            try
            {
                // Obtain the target organization's Web address and client logon 
                // credentials from the user.
                ServerConnection serverConnect = new ServerConnection();
                ServerConnection.Configuration config = serverConnect.GetServerConfiguration();

                Assign app = new Assign();
                app.Run(config, true);
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp);
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode);
                Console.WriteLine("Message: {0}", ex.Detail.Message);
                Console.WriteLine("Plugin Trace: {0}", ex.Detail.TraceText);
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

                    FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe 
                        = ex.InnerException 
                        as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                    if (fe != null)
                    {
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp);
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode);
                        Console.WriteLine("Message: {0}", fe.Detail.Message);
                        Console.WriteLine("Plugin Trace: {0}", fe.Detail.TraceText);
                        Console.WriteLine("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }
            // Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            // SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

            finally
            {
                Console.WriteLine("Press <Enter> to exit.");
                Console.ReadLine();
            }

        }
        #endregion Main
	}
}
//</snippetAssign>
