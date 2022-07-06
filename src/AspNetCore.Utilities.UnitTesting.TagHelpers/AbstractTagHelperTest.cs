﻿#nullable enable
using AngleSharp.Dom;
using AngleSharp.Html;
using ICG.AspNetCore.Utilities.UnitTesting.TagHelpers.FromFramework;

namespace ICG.AspNetCore.Utilities.UnitTesting.TagHelpers;

public abstract class BaseTagHelperTest
{
    public static TagHelperContext MakeTagHelperContext(string tagName = "div", TagHelperAttributeList? attributes = null)
    {
        attributes ??= new TagHelperAttributeList();

        return new TagHelperContext(
            tagName,
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));
    }

    public static TagHelperOutput MakeTagHelperOutput(
        string tagName,
        TagHelperAttributeList? attributes = null,
        HtmlString? childContent = null)
    {
        attributes ??= new TagHelperAttributeList();

        return new TagHelperOutput(
            tagName,
            attributes,
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(childContent);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    public virtual SettingsTask VerifyTagHelper(TagHelperOutput output, Action<INodeList>? action) => Verify(output.Render())
        .UseExtension("html")
        .ScrubEmptyLines()
        .PrettyPrintHtml(action);

    public virtual SettingsTask VerifyTagHelper(TagHelperOutput output) => VerifyTagHelper(output, null);
}

public abstract class LoggingTagHelperTest : BaseTagHelperTest
{
    protected ITestOutputHelper Output { get; }

    protected LoggingTagHelperTest(ITestOutputHelper output)
    {
        Output = output;
    }

    public override SettingsTask VerifyTagHelper(TagHelperOutput output)
    {
        var hasWrittenOutput = false;
        return base.VerifyTagHelper(output, (document) =>
        {
            if (hasWrittenOutput) return;
            var builder = new StringBuilder();
            var formatter = new PrettyMarkupFormatter
            {
                Indentation = "  ",
                NewLine = "\n"
            };
            using var writer = new StringWriter(builder);
            document.ToHtml(writer, formatter);
            writer.Flush();

            Output.WriteLine($"Output:{builder}");
            hasWrittenOutput = true;
        });
    }
}

public abstract class ModelTagHelperTest<TTagHelper, TModel> : LoggingTagHelperTest where TModel : new()
{
    protected ModelTagHelperTest(ITestOutputHelper output) : base(output)
    {
    }

    internal abstract TTagHelper TagHelperFactory(IHtmlGenerator htmlGenerator, ModelExpression modelExpression, ViewContext viewContext);

    protected TTagHelper GetTagHelper(
        IHtmlGenerator htmlGenerator,
        object model,
        string propertyName,
        IModelMetadataProvider? metadataProvider = null)
    {
        return GetTagHelper(
            htmlGenerator,
            container: new TModel(),
            containerType: typeof(TModel),
            model: model,
            propertyName: propertyName,
            expressionName: propertyName,
            metadataProvider: metadataProvider);
    }

    protected TTagHelper GetTagHelper(
        IHtmlGenerator htmlGenerator,
        object container,
        Type containerType,
        object model,
        string propertyName,
        string expressionName,
        IModelMetadataProvider? metadataProvider = null)
    {
        metadataProvider ??= new TestModelMetadataProvider();

        var containerExplorer = metadataProvider.GetModelExplorerForType(containerType, container);

        var propertyMetadata = metadataProvider.GetMetadataForProperty(containerType, propertyName);
        var modelExplorer = containerExplorer.GetExplorerForExpression(propertyMetadata, model);

        var modelExpression = new ModelExpression(expressionName, modelExplorer);
        var viewContext = TestableHtmlGenerator.GetViewContext(container, htmlGenerator, metadataProvider);

        return TagHelperFactory(htmlGenerator, modelExpression, viewContext);
    }
}