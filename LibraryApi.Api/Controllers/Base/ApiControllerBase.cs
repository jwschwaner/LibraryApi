using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Api.Controllers.Base;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase;
