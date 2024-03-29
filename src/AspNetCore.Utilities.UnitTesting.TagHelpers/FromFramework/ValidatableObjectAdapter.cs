﻿// <auto-generated>
// Copied from https://github.com/dotnet/aspnetcore/blob/v6.0.6/src/Mvc/Mvc.DataAnnotations/src/ValidatableObjectAdapter.cs
// This source file is subject to the MIT license granted by the .NET Foundation as copied into the root of this repository.
// Standard Changes:
//  - Pulling out global usings
//  - Using file scoped namespaces
//  - Changing of the namespace
// File Specific:
//  - Replace resource string with formatted string
// </auto-generated>

namespace ICG.AspNetCore.Utilities.UnitTesting.TagHelpers.FromFramework;

internal class ValidatableObjectAdapter : IModelValidator
{
    public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
    {
        var model = context.Model;
        if (model == null)
        {
            return Enumerable.Empty<ModelValidationResult>();
        }

        if (!(model is IValidatableObject validatable))
        {
            // ICG-MS: Replace resource string with formatted string
            throw new InvalidOperationException($"Type {model.GetType()} incompatible with {nameof(IValidatableObject)}");
        }

        // The constructed ValidationContext is intentionally slightly different from what
        // DataAnnotationsModelValidator creates. The instance parameter would be context.Container
        // (if non-null) in that class. But, DataAnnotationsModelValidator _also_ passes context.Model
        // separately to any ValidationAttribute.
        var validationContext = new ValidationContext(
            instance: validatable,
            serviceProvider: context.ActionContext?.HttpContext?.RequestServices,
            items: null)
        {
            DisplayName = context.ModelMetadata.GetDisplayName(),
            MemberName = context.ModelMetadata.Name,
        };

        return ConvertResults(validatable.Validate(validationContext));
    }

    private IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results)
    {
        foreach (var result in results)
        {
            if (result != ValidationResult.Success)
            {
                if (result.MemberNames == null || !result.MemberNames.Any())
                {
                    yield return new ModelValidationResult(memberName: null, message: result.ErrorMessage);
                }
                else
                {
                    foreach (var memberName in result.MemberNames)
                    {
                        yield return new ModelValidationResult(memberName, result.ErrorMessage);
                    }
                }
            }
        }
    }
}