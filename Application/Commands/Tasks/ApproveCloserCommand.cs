// RealEstate.Application/Commands/Users/ApproveCloserCommand.cs
using MediatR;
using RealEstate.Application.DTOs;

namespace RealEstate.Application.Commands.Users;

public record ApproveCloserCommand(string CloserId, string ApprovedBy) : IRequest<UserDto>;