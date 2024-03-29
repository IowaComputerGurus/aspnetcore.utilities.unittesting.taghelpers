﻿// <auto-generated>
// Copied from https://github.com/dotnet/aspnetcore/blob/v6.0.6/src/Mvc/Mvc.Core/src/ModelBinding/Validation/DefaultModelValidatorProvider.cs
// This source file is subject to the MIT license granted by the .NET Foundation as copied into the root of this repository.
// Standard Changes:
//  - Pulling out global usings
//  - Using file scoped namespaces
//  - Changing of the namespace
// File Specific:
//  - Disable warnings CS1574, CS1584, CS1581, CS1580 around the XML doc comment
// </auto-generated>

namespace ICG.AspNetCore.Utilities.UnitTesting.TagHelpers.FromFramework;
#nullable enable

#pragma warning disable CS1574, CS1584, CS1581, CS1580
/// <summary>
/// A default <see cref="IModelValidatorProvider"/>.
/// </summary>
/// <remarks>
/// The <see cref="DefaultModelValidatorProvider"/> provides validators from <see cref="IModelValidator"/>
/// instances in <see cref="ModelBinding.ModelMetadata.ValidatorMetadata"/>.
/// </remarks>
/// #pragma warning restore CS1574, CS1584, CS1581, CS1580
internal class DefaultModelValidatorProvider : IMetadataBasedModelValidatorProvider
{
    /// <inheritdoc />
    public void CreateValidators(ModelValidatorProviderContext context)
    {
        //Perf: Avoid allocations here
        for (var i = 0; i < context.Results.Count; i++)
        {
            var validatorItem = context.Results[i];

            // Don't overwrite anything that was done by a previous provider.
            if (validatorItem.Validator != null)
            {
                continue;
            }

            if (validatorItem.ValidatorMetadata is IModelValidator validator)
            {
                validatorItem.Validator = validator;
                validatorItem.IsReusable = true;
            }
        }
    }

    public bool HasValidators(Type modelType, IList<object> validatorMetadata)
    {
        for (var i = 0; i < validatorMetadata.Count; i++)
        {
            if (validatorMetadata[i] is IModelValidator)
            {
                return true;
            }
        }

        return false;
    }
}