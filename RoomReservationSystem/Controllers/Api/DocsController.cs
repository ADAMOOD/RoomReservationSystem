using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace RoomReservationSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocsController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetDocsAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var apiControllers = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(typeof(ApiControllerAttribute),
                true)
                .Any())
                .ToList();
            var documentation = new List<object>();
            foreach (var apiController in apiControllers)
            {
                var routeAttr = apiController.GetCustomAttribute<RouteAttribute>();
                string basePath = apiController.Name.Replace("Controller", "");
                if (routeAttr != null)
                {
                    var methods = apiController.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (var method in methods)
                    {
                        var httpGet = method.GetCustomAttribute<HttpGetAttribute>();
                        var httpPost = method.GetCustomAttribute<HttpPostAttribute>();
                        if (httpGet == null && httpPost == null) continue;
                        string httpMethod = (httpGet == null ? "POST" : "GET");
                        
                        var parameters = method.GetParameters()
                        .Select(p => new { Name = p.Name, Type = p.ParameterType.Name })
                        .ToList();

                        documentation.Add(new
                        {
                            Controller = apiController.Name,
                            Endpoint = $"api/{basePath}",
                            Action = method.Name,
                            Method = httpMethod,
                            Parameters = parameters,
                            Returns = method.ReturnType.Name
                        });
                    }
                }
            }
            return Ok(documentation);

        }
    }
}
