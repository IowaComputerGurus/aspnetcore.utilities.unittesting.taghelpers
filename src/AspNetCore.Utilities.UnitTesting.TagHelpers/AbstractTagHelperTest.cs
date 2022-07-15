#nullable enable
using AngleSharp.Dom;
using AngleSharp.Html;
using ICG.AspNetCore.Utilities.UnitTesting.TagHelpers.FromFramework;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ICG.AspNetCore.Utilities.UnitTesting.TagHelpers;

/// <summary>
///     Base class for tag helper tests. Contains <see cref="VerifyTagHelper(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput,System.Action{AngleSharp.Dom.INodeList}?)"/>
/// </summary>
public abstract class BaseTagHelperTest
{
    /// <summary>
    ///     Verifies the output of a tag helper
    /// </summary>
    /// <param name="output">The output of a tag helper</param>
    /// <param name="action">A callback containing the <see cref="INodeList"/> from AngleSharp's parsing of the generated Html. Can be null</param>
    /// <returns>A <see cref="SettingsTask"/> to await</returns>
    public virtual SettingsTask VerifyTagHelper(TagHelperOutput output, Action<INodeList>? action) => Verify(output.Render())
        .UseExtension("html")
        .ScrubEmptyLines()
        .PrettyPrintHtml(action);

    /// <summary>
    ///     Verifies the output of a tag helper
    /// </summary>
    /// <param name="output">The output of a tag helper</param>
    /// <returns>A <see cref="SettingsTask"/> to await</returns>
    public virtual SettingsTask VerifyTagHelper(TagHelperOutput output) => VerifyTagHelper(output, null);
}

/// <summary>
///     Base class for helper tests that will write the tag helper output to the xUnit logger.
/// </summary>
public abstract class LoggingTagHelperTest : BaseTagHelperTest
{
    /// <summary>
    ///     The test output helper provided by xUnit
    /// </summary>
    protected ITestOutputHelper Output { get; }

    /// <summary>
    ///     Creates an instance of the <see cref="LoggingTagHelperTest"/>
    /// </summary>
    /// <param name="output">The <see cref="ITestOutputHelper"/> injected by xUnit</param>
    protected LoggingTagHelperTest(ITestOutputHelper output)
    {
        Output = output;
    }

    /// <summary>
    ///     Verifies the output of a tag helper. Writes the prettified Html of the tag output
    ///     to the xUnit logger.
    /// </summary>
    /// <param name="output">The output of a tag helper</param>
    /// <returns>A <see cref="SettingsTask"/> to await</returns>
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

/// <summary>
///     Base class for tag helper tests involving the model binder, including validation and display attributes.
/// </summary>
/// <typeparam name="TTagHelper">The tag helper to test</typeparam>
/// <typeparam name="TModel">The model to test with</typeparam>
public abstract class ModelTagHelperTest<TTagHelper, TModel> : LoggingTagHelperTest where TModel : new()
{
    /// <summary>
    ///     Creates an instance of the <see cref="ModelTagHelperTest{TTagHelper,TModel}"/>
    /// </summary>
    /// <param name="output">The <see cref="ITestOutputHelper"/> injected by xUnit</param>
    protected ModelTagHelperTest(ITestOutputHelper output) : base(output)
    {
    }

    /// <summary>
    /// Factory method to generate the tag helper.
    /// </summary>
    /// <param name="htmlGenerator">The <see cref="IHtmlGenerator"/> instance to provide the tag helper</param>
    /// <param name="modelExpression">The <see cref="ModelExpression"/> to provide the tag helper</param>
    /// <param name="viewContext">The <see cref="ViewContext"/> to provide the </param>
    /// <returns></returns>
    protected abstract TTagHelper TagHelperFactory(IHtmlGenerator htmlGenerator, ModelExpression modelExpression, ViewContext viewContext);

    protected TTagHelper GetTagHelper<TProperty>(TModel model, Expression<Func<TModel, TProperty>> property)
    {
        var name = ((MemberExpression)property.Body).Member.Name;
        object propValue = property.Compile().Invoke(model);
        var metadataProvider = new TestModelMetadataProvider();
        var htmlGenerator = new TestableHtmlGenerator(metadataProvider);

        return GetTagHelper(htmlGenerator, model, propValue, name);
    }

    /// <summary>
    /// Gets a tag helper instance configured as if using the asp-for attribute
    /// </summary>
    /// <param name="htmlGenerator">The <see cref="IHtmlGenerator"/> to use</param>
    /// <param name="model">An instance of the <typeparamref name="TModel"/> to use</param>
    /// <param name="propertyName">The property name from the model</param>
    /// <param name="metadataProvider">A metadata provider to use, optional</param>
    /// <returns>A tag helper created by <see cref="TagHelperFactory"/></returns>
    protected TTagHelper GetTagHelper(
        IHtmlGenerator htmlGenerator,
        TModel container,
        object model,
        string propertyName,
        IModelMetadataProvider? metadataProvider = null)
    {
        metadataProvider ??= new TestModelMetadataProvider();

        var containerExplorer = metadataProvider.GetModelExplorerForType(typeof(TModel), model);

        var propertyMetadata = metadataProvider.GetMetadataForProperty(typeof(TModel), propertyName);
        var modelExplorer = containerExplorer.GetExplorerForExpression(propertyMetadata, model);

        var modelExpression = new ModelExpression(propertyName, modelExplorer);
        var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);

        return TagHelperFactory(htmlGenerator, modelExpression, viewContext);

    }
}