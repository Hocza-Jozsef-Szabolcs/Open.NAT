using Microsoft.AspNetCore.Mvc;

namespace Open.Nat.TestServer.Controllers;

[ApiController]
[Route("TestServer")]
public class TTestServerController : ControllerBase
{
  private readonly ILogger<TTestServerController> _logger;

  public TTestServerController(ILogger<TTestServerController> logger)
  {
    _logger = logger;
  }

  [HttpGet]
  public string Version()
  {
    return "TestServer 1.0.0";
  }
}