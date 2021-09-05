using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    public class FirebaseNotficationService
    {
        private readonly FirebaseMessaging firebaseMessaging;
        public FirebaseNotficationService()
        {
            FirebaseApp app = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("fcm.json")
            });
            firebaseMessaging = FirebaseMessaging.GetMessaging(app);
        }
        public async Task<bool> SendMessageAsync(string Title, string Description, string Data, string Token)
        {
            Message message = new()
            {
                Notification = new Notification
                {
                    Title = Title,
                    Body = Description
                },
                Data = new Dictionary<string, string>()
                {
                    { "json", Data }
                },
                Token = Token
            };

            string msg = await firebaseMessaging.SendAsync(message);
            if (msg is not null && msg.Length > 0)
            {
                return true;
            }
            return false;
        }
    }
}
