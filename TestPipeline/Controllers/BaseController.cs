using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TestPipeline.Controllers;

[Authorize]
public class BaseController : ControllerBase
{
}