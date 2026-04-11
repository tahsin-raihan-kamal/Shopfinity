using FluentValidation;

namespace Shopfinity.Application.Features.Products.DTOs;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.Price)
            .InclusiveBetween(0.01m, 999999.99m).WithMessage("Price must be between 0.01 and 999999.99.");

        RuleFor(x => x.StockQuantity)
            .InclusiveBetween(0, 100000).WithMessage("Stock quantity must be between 0 and 100000.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}
