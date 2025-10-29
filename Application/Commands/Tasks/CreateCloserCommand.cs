// RealEstate.Application/Commands/Users/CreateCloserCommand.cs
using MediatR;
using RealEstate.Application.DTOs;

namespace RealEstate.Application.Commands.Users;

public record CreateCloserCommand(
    string Name,
    string Email,
    string Password,
    string? PhoneNumber,
    string Agency) : IRequest<UserDto>;