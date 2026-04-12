using FluentValidation;
using Shopfinity.Application.Features.Wishlists.DTOs;

namespace Shopfinity.Application.Features.Wishlists.Validators;

public class AddWishlistItemDtoValidator : AbstractValidator<AddWishlistItemDto>
{
    public AddWishlistItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("A valid productId is required.");
    }
}
