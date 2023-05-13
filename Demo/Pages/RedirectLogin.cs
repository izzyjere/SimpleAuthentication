using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Demo.Pages
{
    public class RedirectLogin   : ComponentBase
    {
        [CascadingParameter]
        Task<AuthenticationState> AuthenticationState { get; set; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var user = (await AuthenticationState).User;
            if(!user.Identity.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/login",true);
            }
        }
    }
}
