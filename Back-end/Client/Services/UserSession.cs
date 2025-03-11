using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Back_end.Client.Services
{
    public class UserSession
    {
        public int UserId;
        private readonly ILocalStorageService _LocalStorage;
        private readonly NavigationManager _NavigationManager;
        public UserSession(ILocalStorageService localStorage, NavigationManager navigationManager)
        {
            _LocalStorage = localStorage;
            _NavigationManager = navigationManager;
        }

        public async Task SetUserId()
        {
            UserId = await _LocalStorage.GetItemAsync<int>("UserId");
        }

        public async Task Login(int userId)
        {
            UserId = userId;

            await _LocalStorage.SetItemAsync("UserId", userId);
            
            _NavigationManager.NavigateTo("/", forceLoad: true);
        }

        public async Task Logout()
        {
             UserId = 0;

            await _LocalStorage.ClearAsync();

             _NavigationManager.NavigateTo("/loginregister");
        }

        public bool IsLoggedIn()
        {
            if (UserId != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
