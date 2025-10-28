// RealEstate.Application/Commands/Auth/LoginCommand.cs
using MediatR;
using RealEstate.Application.DTOs;

namespace RealEstate.Application.Commands.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;