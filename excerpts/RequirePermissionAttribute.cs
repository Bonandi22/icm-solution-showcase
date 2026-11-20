// Excerpt from ICM Solution (private repository) — ApiInfrastructure/Security/Attributes/RequirePermissionAttribute.cs
// Usage on a controller action: [RequirePermission("VIEW_FINANCIAL")]

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ApiInfrastructure.Security.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;

        public RequirePermissionAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Obtem as permissões do usuário do JWT
            var userPermissions = user.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet();

            // Verifica se o usuário possui pelo menos uma das permissões necessárias
            bool hasPermission = _permissions.Any(p => userPermissions.Contains(p));

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}