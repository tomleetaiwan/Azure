#r "SendGrid"
#r "System.Data"
#r "Newtonsoft.Json"

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

public static Mail prepareMailMessage (string emailAddress,string mailSubject,string mailBody)
{
    Mail messageMail = new Mail
    {        
        Subject = mailSubject          
    };

    var personalization = new Personalization();
    // change to email of recipient
    personalization.AddTo(new Email(emailAddress));   

    Content content = new Content
    {
        Type = "text/plain",
        Value = mailBody
    };
    messageMail.AddContent(content);
    messageMail.AddPersonalization(personalization);
    return (messageMail);
}

public static void InsertOrdertoDatabase (string dbConnectionString,string userName,string userEmail,string orderItem)
{
   using (var connectionOrder = new SqlConnection(dbConnectionString))
   {
	var cmdInsert = new SqlCommand("insert into OrderInfo values (@name,@email,@item)", connectionOrder);
        cmdInsert.Parameters.AddWithValue("@name", userName);
        cmdInsert.Parameters.AddWithValue("@email", userEmail);
        cmdInsert.Parameters.AddWithValue("@item", orderItem);
        connectionOrder.Open();
        cmdInsert.ExecuteNonQuery();
   } 
}

 public class SlackHook
{
    public string text { get; set; }
    public string icon_emoji { get; set; }
}

public static void SlackMessage( string WebhookURI,string message, string messageType)
{

    string IconCode = "";
    switch (messageType)
    {
      case "I":
      IconCode = ":white_check_mark:";
      break;
      case "W":
      IconCode = ":warning:";
      break;
      case "E":
      IconCode = ":x:";
      break;
     }

     var statusMsg = IconCode + message;
     using (var client = new HttpClient())
     {
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "AzureFunctions");        
        var msg = new SlackHook { text = statusMsg };
        StringContent SlackMessageJSONContent = new StringContent(JsonConvert.SerializeObject(msg));
        HttpResponseMessage response = client.PostAsync( WebhookURI, SlackMessageJSONContent).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
      }
}