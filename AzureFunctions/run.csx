#r "Newtonsoft.Json"
#r "SendGrid"
#r "System.Configuration"

#load "Library.csx"

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;
using System;
using System.Configuration;

// The Order message JSON format as below 
//
//  {
//   "Name":"Tom Lee",
//   "Email":"tomlee@contoso.com",
//   "Item":"A1 Business Cloud"
//  }
//
//
//


public static void Run(string myQueueItem, TraceWriter log,out Mail message)
{
    // Basic information of Order
    string userName = null;
    string userEmail = null;
    string orderItem = null;
    // JSON Object 
    JObject restoredObject = null;
    // SendGird mail object
    message = new Mail ();    
    // Get Azure SQL Database Connection String from App Servive Application Setting
    string dbConnectionString = ConfigurationManager.ConnectionStrings["OrderDBConnectionString"].ConnectionString;
    // Get Slack Webhooks URI 
    string SlackWebhooksURI = ConfigurationManager.AppSettings["SlackWebhook"];
    //Process the new order message code here
    log.Info($"Start Process");

    try
    {
      //Parse new order message format
      restoredObject = JsonConvert.DeserializeObject<JObject>(myQueueItem);
      userName = restoredObject["Name"].ToString();
      userEmail = restoredObject["Email"].ToString();
      orderItem = restoredObject["Item"].ToString();
      
      //Insert new customer database and update tenant information
      InsertOrdertoDatabase (dbConnectionString,userName,userEmail,orderItem);

      //Invoke SendGrid API send out comfirmation email to the customer
      message = prepareMailMessage (userEmail,"Confirmation","Thanks for your order!");

      //Invoke Slack Webhooks API to inform internal operator
      SlackMessage(SlackWebhooksURI, "Receive a new order and process complete","I");

      //Process new order complete  
      log.Info($"Order Process Complete");
     }
     catch (Exception ex)
     {
       log.Info($"Exception message is"+ex.Message);        
       //Invoke Slack Webhooks API to inform internal operator
       SlackMessage(SlackWebhooksURI, "Receive a new order but something go wrong","E");  
     }      

}