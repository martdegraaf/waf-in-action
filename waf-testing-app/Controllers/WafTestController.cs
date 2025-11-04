using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WafTestingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WafTestController : ControllerBase
{
    private readonly ILogger<WafTestController> _logger;

    public WafTestController(ILogger<WafTestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            Message = "WAF Testing Controller",
            AvailableEndpoints = new[]
            {
                "GET /api/waftest/safe - Safe endpoint",
                "GET /api/waftest/sql-injection - SQL injection test",
                "GET /api/waftest/xss - XSS test", 
                "GET /api/waftest/path-traversal - Path traversal test",
                "POST /api/waftest/malicious-payload - Malicious payload test",
                "GET /api/waftest/bot-simulation - Bot simulation test",
                "POST /api/waftest/person-registration - Person registration with 'Meneer Havinga' test",
                "POST /api/waftest/vehicle-registration - Vehicle registration with 'LAND ROVER' test",
                "POST /api/waftest/comment-submission - Comment submission with '---' pattern test"
            }
        });
    }

    [HttpGet("safe")]
    public IActionResult SafeEndpoint()
    {
        _logger.LogInformation("Safe endpoint accessed");
        return Ok(new { 
            Message = "This is a safe endpoint", 
            Status = "Success",
            Data = new { UserId = 1, UserName = "TestUser", Role = "User" }
        });
    }

    [HttpGet("sql-injection")]
    public IActionResult SqlInjectionTest([FromQuery] string userId = "")
    {
        _logger.LogWarning("SQL injection test endpoint accessed with userId: {UserId}", userId);
        
        // These parameters should trigger WAF
        var maliciousInputs = new[]
        {
            "1' OR '1'='1",
            "1; DROP TABLE users--",
            "1' UNION SELECT * FROM users--",
            "admin'--",
            "1' OR 1=1#"
        };

        if (maliciousInputs.Contains(userId))
        {
            return BadRequest(new { 
                Message = "Malicious SQL injection detected",
                Input = userId,
                Status = "Blocked by application logic"
            });
        }

        return Ok(new { 
            Message = "SQL query would be executed here",
            UserId = userId,
            Status = "This should be blocked by WAF"
        });
    }

    [HttpGet("xss")]
    public IActionResult XssTest([FromQuery] string comment = "")
    {
        _logger.LogWarning("XSS test endpoint accessed with comment: {Comment}", comment);
        
        // These should trigger WAF
        var maliciousScripts = new[]
        {
            "<script>alert('XSS')</script>",
            "<img src=x onerror=alert('XSS')>",
            "javascript:alert('XSS')",
            "<svg onload=alert('XSS')>",
            "'><script>alert('XSS')</script>"
        };

        if (maliciousScripts.Any(script => comment.Contains(script, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { 
                Message = "Malicious script detected",
                Input = comment,
                Status = "Blocked by application logic"
            });
        }

        return Ok(new { 
            Message = "Comment would be displayed here",
            Comment = comment,
            Status = "This should be blocked by WAF"
        });
    }

    [HttpGet("path-traversal")]
    public IActionResult PathTraversalTest([FromQuery] string filePath = "")
    {
        _logger.LogWarning("Path traversal test endpoint accessed with filePath: {FilePath}", filePath);
        
        // These should trigger WAF
        var maliciousPaths = new[]
        {
            "../../../etc/passwd",
            "..\\..\\..\\windows\\system32\\config\\sam",
            "../etc/shadow",
            "../../../../boot.ini",
            "../web.config"
        };

        if (maliciousPaths.Any(path => filePath.Contains(path, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { 
                Message = "Path traversal attempt detected",
                Input = filePath,
                Status = "Blocked by application logic"
            });
        }

        return Ok(new { 
            Message = "File would be accessed here",
            FilePath = filePath,
            Status = "This should be blocked by WAF"
        });
    }

    [HttpPost("malicious-payload")]
    public IActionResult MaliciousPayloadTest([FromBody] JsonElement payload)
    {
        var payloadString = payload.ToString();
        _logger.LogWarning("Malicious payload test endpoint accessed with payload: {Payload}", payloadString);
        
        // Check for various malicious patterns
        var maliciousPatterns = new[]
        {
            "eval(",
            "document.cookie",
            "window.location",
            "base64",
            "fromCharCode",
            "innerHTML",
            "outerHTML"
        };

        if (maliciousPatterns.Any(pattern => payloadString.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { 
                Message = "Malicious payload detected",
                Status = "Blocked by application logic"
            });
        }

        return Ok(new { 
            Message = "Payload would be processed here",
            ReceivedPayload = payload,
            Status = "This should be blocked by WAF"
        });
    }

    [HttpGet("bot-simulation")]
    public IActionResult BotSimulationTest()
    {
        _logger.LogWarning("Bot simulation test endpoint accessed");
        
        // Check User-Agent header for bot patterns
        var userAgent = Request.Headers.UserAgent.ToString();
        var clientIP = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        var botPatterns = new[]
        {
            "bot", "crawler", "spider", "scraper", "scanner"
        };

        bool botPatternDetected = botPatterns.Any(pattern => userAgent.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        string detectedPattern = botPatternDetected ? 
            botPatterns.First(p => userAgent.Contains(p, StringComparison.OrdinalIgnoreCase)) : "";

        var response = new
        {
            Message = "Bot simulation test endpoint",
            UserAgent = userAgent,
            ClientIP = clientIP,
            TestType = "Microsoft Bot Manager Detection",
            ImportantNote = "Bot Manager may return 200 OK - this is NORMAL behavior!",
            Explanation = new
            {
                BotManagerBehavior = "Uses behavioral analysis over time, not immediate content blocking",
                FirstRequests = "Often allowed to establish patterns",
                ProductionBlocking = "Effectively blocks real bots based on behavior",
                DemoRecommendation = "Use OWASP tests (SQL, XSS, Path Traversal) for guaranteed 403 responses"
            },
            ExpectedResult = "200 OK (Bot Manager learning) OR 403 Blocked (if pattern detected)",
            BotPatternDetected = botPatternDetected,
            DetectedPattern = detectedPattern,
            Timestamp = DateTime.UtcNow
        };

        if (botPatternDetected)
        {
            _logger.LogWarning("Bot pattern detected in User-Agent: {UserAgent}", userAgent);
        }

        return Ok(response);
    }

    [HttpGet("large-request")]
    public IActionResult LargeRequestTest([FromQuery] string data = "")
    {
        _logger.LogInformation("Large request test endpoint accessed with data length: {Length}", data.Length);
        
        if (data.Length > 8000)
        {
            return BadRequest(new { 
                Message = "Request too large",
                DataLength = data.Length,
                Status = "This should be blocked by WAF"
            });
        }

        return Ok(new { 
            Message = "Request processed",
            DataLength = data.Length,
            Status = "Success"
        });
    }

    [HttpGet("protocol-attack")]
    public IActionResult ProtocolAttackTest()
    {
        _logger.LogWarning("Protocol attack test endpoint accessed");
        
        // Check for HTTP protocol anomalies
        var headers = Request.Headers;
        var suspiciousHeaders = new[]
        {
            "X-Forwarded-For: 127.0.0.1",
            "Host: evil.com",
            "Content-Length: -1"
        };

        return Ok(new { 
            Message = "Protocol attack simulation",
            Headers = headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Status = "This might trigger WAF protocol rules"
        });
    }

    [HttpGet("command-injection")]
    public IActionResult CommandInjectionTest([FromQuery] string command = "")
    {
        _logger.LogWarning("Command injection test endpoint accessed with command: {Command}", command);
        
        // Check for command injection patterns
        var commandPatterns = new[]
        {
            ";", "&", "|", "`", "$", "ls", "cat", "rm", "del", "dir", "whoami", "id"
        };

        if (commandPatterns.Any(pattern => command.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { 
                Message = "Command injection attempt detected",
                Command = command,
                Status = "This should be blocked by OWASP rules"
            });
        }

        return Ok(new { 
            Message = "Safe command processed",
            Command = command,
            Status = "Success"
        });
    }

    [HttpPost("person-registration")]
    public IActionResult PersonRegistrationTest([FromBody] PersonRegistrationRequest request)
    {
        _logger.LogInformation("Person registration test endpoint accessed with name: {Name}", request?.Name ?? "");
        
        // Check for problematic names that might trigger WAF
        var problematicPatterns = new[]
        {
            "Meneer Havinga", "LAND ROVER", "---", "--", "Mr. Test", "SELECT", "UNION", "DROP"
        };

        if (!string.IsNullOrEmpty(request?.Name) && 
            problematicPatterns.Any(pattern => request.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Potentially problematic name pattern detected: {Name}", request.Name);
        }

        return Ok(new { 
            Message = "Person registration processed",
            Name = request?.Name ?? "",
            Email = request?.Email ?? "",
            Status = "Registration successful - WAF allows legitimate data",
            RegisteredAt = DateTime.UtcNow
        });
    }

    [HttpPost("vehicle-registration")]
    public IActionResult VehicleRegistrationTest([FromBody] VehicleRegistrationRequest request)
    {
        _logger.LogInformation("Vehicle registration test endpoint accessed with brand: {Brand}", request?.Brand ?? "");
        
        // Check for vehicle brands that might contain problematic patterns
        var knownBrands = new[]
        {
            "LAND ROVER", "MERCEDES-BENZ", "BMW", "AUDI", "VOLKSWAGEN", "FORD", "TOYOTA"
        };

        bool isKnownBrand = !string.IsNullOrEmpty(request?.Brand) && 
                           knownBrands.Any(brand => request.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase));

        return Ok(new { 
            Message = "Vehicle registration processed",
            Brand = request?.Brand ?? "",
            Model = request?.Model ?? "",
            Year = request?.Year ?? 0,
            IsKnownBrand = isKnownBrand,
            Status = "Vehicle registration successful - WAF handles legitimate vehicle data",
            RegisteredAt = DateTime.UtcNow
        });
    }

    [HttpPost("comment-submission")]
    public IActionResult CommentSubmissionTest([FromBody] CommentSubmissionRequest request)
    {
        _logger.LogInformation("Comment submission test endpoint accessed with comment length: {Length}", request?.Comment?.Length ?? 0);
        
        // Check for problematic patterns in comments
        var suspiciousPatterns = new[]
        {
            "---", "--", "/*", "*/", "<!--", "-->", "<script", "</script"
        };

        bool containsSuspiciousPattern = !string.IsNullOrEmpty(request?.Comment) && 
                                       suspiciousPatterns.Any(pattern => request.Comment.Contains(pattern, StringComparison.OrdinalIgnoreCase));

        if (containsSuspiciousPattern)
        {
            _logger.LogWarning("Suspicious pattern detected in comment: {Comment}", request.Comment);
        }

        return Ok(new { 
            Message = "Comment submitted successfully",
            Comment = request?.Comment ?? "",
            Author = request?.Author ?? "",
            ContainsSuspiciousPattern = containsSuspiciousPattern,
            Status = "Comment processed - WAF evaluates content patterns",
            SubmittedAt = DateTime.UtcNow
        });
    }
}

public class PersonRegistrationRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

public class VehicleRegistrationRequest
{
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
}

public class CommentSubmissionRequest
{
    public string Comment { get; set; } = "";
    public string Author { get; set; } = "";
}
