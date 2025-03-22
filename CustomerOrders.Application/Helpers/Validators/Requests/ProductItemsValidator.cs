using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Requests
{
    public class ProductItemsValidator : AbstractValidator<List<AddOrderProductItemRequest>>
    {
        public ProductItemsValidator(IValidator<AddOrderProductItemRequest> itemValidator)
        {
            RuleForEach(x => x).SetValidator(itemValidator);
        }
    }
}
