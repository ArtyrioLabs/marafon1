using CSharpFunctionalExtensions;
using FluentValidation.Results;
using MediatR;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Commands
{
    /// <summary>
    /// Request for deleting a User from Room.
    /// </summary>
    /// <param name="UserId">User identifier to be deleted.</param>
    /// <param name="AdminUserCode">Admin user authorization code.</param>
    public record DeleteUserCommand(ulong UserId, string AdminUserCode)
        : IRequest<Result<bool, ValidationResult>>;
}
