using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NotificationService.Data;
using NotificationService.Models;
using System;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    public class NotifierService : Notifier.NotifierBase
    {
        private readonly ILogger<NotifierService> _logger;
        private readonly NotificationsDbContext _notificationsDbContext;
        private readonly IMemoryCache _cache;
        private readonly FirebaseNotficationService _firebaseNotficationService;
        public NotifierService(ILogger<NotifierService> logger, NotificationsDbContext postsDbContext, IMemoryCache cache, FirebaseNotficationService firebaseNotficationService)
        {
            _logger = logger;
            _notificationsDbContext = postsDbContext;
            _cache = cache;
            _firebaseNotficationService = firebaseNotficationService;
        }
        public override async Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.FcmToken))
                {
                    return new RegisterUserResponse { Success = false };
                }
                User user = await _notificationsDbContext.Users.FindAsync(request.UserId);
                if (user is null)
                {
                    _notificationsDbContext.Users.Add(new User
                    {
                        UserId = request.UserId,
                        Token = request.FcmToken
                    });
                }
                else
                {
                    user.Token = request.FcmToken;
                }
                int rowCount = await _notificationsDbContext.SaveChangesAsync();
                if (rowCount > 0)
                {
                    _cache.Set(request.UserId, request.FcmToken, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
                    return new RegisterUserResponse { Success = true };
                }
                else
                {
                    return new RegisterUserResponse { Success = false };
                }
            }
            catch (Exception)
            {
                return new RegisterUserResponse { Success = false };
            }
        }
        public override async Task<NotifyUserReponse> NotifyUser(NotifyUserRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Data) || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            {
                return new NotifyUserReponse { Success = false };
            }
            string token = _cache.Get<string>(request.UserId);
            if (token is null)
            {
                User user = await _notificationsDbContext.Users.FindAsync(request.UserId);
                if (user is null)
                {
                    return new NotifyUserReponse { Success = false };
                }
                token = user.Token;
                _cache.Set(user.UserId, user.Token, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            }
            try
            {
                await _firebaseNotficationService.SendMessageAsync(
                    request.Title,
                    request.Description,
                    request.Data,
                    token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not send the msg: {ex.Message}");
                return new NotifyUserReponse { Success = false };
            }


            return new NotifyUserReponse { Success = true };

        }
    }
}

