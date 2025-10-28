// RealEstate.API/Controllers/AuthController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Commands.Auth;
using RealEstate.Application.DTOs;

namespace RealEstate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand(request);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // Implement refresh token logic
        return Ok();
    }
}

// RealEstate.API/Controllers/PropertiesController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Commands.Properties;
using RealEstate.Application.DTOs;
using RealEstate.Application.Queries.Properties;
using System.Security.Claims;

namespace RealEstate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PropertiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    [Authorize(Roles = "Closer,Admin")]
    public async Task<ActionResult<PropertyDto>> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        var command = new CreatePropertyCommand(request, GetUserId());
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProperty), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyDto>> GetProperty(string id)
    {
        var query = new GetPropertyByIdQuery(id, GetUserId());
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/down-payment/accept")]
    [Authorize(Roles = "Closer,Admin")]
    public async Task<ActionResult<PropertyDto>> AcceptDownPayment(
        string id, 
        [FromBody] AcceptDownPaymentRequest request)
    {
        var command = new AcceptDownPaymentCommand(id, request.Amount, GetUserId());
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

// RealEstate.API/Controllers/InvitationsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Commands.Invitations;
using RealEstate.Application.DTOs;
using System.Security.Claims;

namespace RealEstate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    [Authorize(Roles = "Closer,Admin")]
    public async Task<ActionResult<InvitationDto>> CreateInvitation([FromBody] CreateInvitationRequest request)
    {
        var command = new CreateInvitationCommand(
            request.Email,
            request.Role,
            request.PropertyId,
            GetUserId());
        
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateInvitation), new { id = result.Id }, result);
    }
}

// RealEstate.API/Controllers/DocumentsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Commands.Documents;
using RealEstate.Application.DTOs;
using RealEstate.Application.Queries.Documents;
using System.Security.Claims;

namespace RealEstate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("upload")]
    public async Task<ActionResult<DocumentDto>> UploadDocument([FromForm] IFormFile file, [FromForm] string propertyId, [FromForm] string documentType)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var command = new UploadDocumentCommand(
            propertyId,
            GetUserId(),
            file.FileName,
            memoryStream.ToArray(),
            file.ContentType,
            documentType);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(UploadDocument), new { id = result.Id }, result);
    }

    [HttpGet("property/{propertyId}")]
    public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByProperty(string propertyId)
    {
        var query = new GetDocumentsByPropertyQuery(propertyId, GetUserId());
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Closer,Admin")]
    public async Task<ActionResult<DocumentDto>> ApproveDocument(string id)
    {
        var command = new ApproveDocumentCommand(id, GetUserId());
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

// RealEstate.API/Controllers/TasksController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Commands.Tasks;
using RealEstate.Application.DTOs;
using RealEstate.Application.Queries.Tasks;
using System.Security.Claims;

namespace RealEstate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    [Authorize(Roles = "Closer,Admin")]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var command = new CreateTaskCommand(
            request.PropertyId,
            request.Title,
            request.Description,
            request.TaskType,
            request.AssignedTo,
            request.DueDate,
            GetUserId());

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateTask), new { id = result.Id }, result);
    }

    [HttpGet("my-tasks")]
    public async Task<ActionResult<List<TaskDto>>> GetMyTasks()
    {
        var query = new GetTasksByUserQuery(GetUserId());
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TaskDto>> UpdateTaskStatus(string id, [FromBody] UpdateTaskRequest request)
    {
        if (!request.Status.HasValue)
            return BadRequest("Status is required");

        var command = new UpdateTaskStatusCommand(id, request.Status.Value, GetUserId());
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

// RealEstate.API/Controllers/UsersController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Commands.Users;
using RealEstate.Application.DTOs;
using System.Security.Claims;

namespace RealEstate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("closers")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> RegisterCloser([FromBody] CreateCloserRequest request)
    {
        var command = new CreateCloserCommand(
            request.Name,
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.Agency);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(RegisterCloser), new { id = result.Id }, result);
    }

    [HttpPost("closers/{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> ApproveCloser(string id)
    {
        var command = new ApproveCloserCommand(id, GetUserId());
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}