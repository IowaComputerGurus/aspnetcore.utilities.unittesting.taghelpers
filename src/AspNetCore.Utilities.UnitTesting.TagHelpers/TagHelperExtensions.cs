using Microsoft.Extensions.WebEncoders.Testing;

namespace ICG.AspNetCore.Utilities.UnitTesting.TagHelpers;
#nullable enable

/// <summary>
/// Extension methods for tag helpers
/// </summary>
public static class TagHelperExtensions
{
    /// <summary>
    ///     Renders a tag helper to a <see cref="TagHelperOutput"/>
    /// </summary>
    /// <param name="helper">The tag helper to render</param>
    /// <param name="tagName">The tag name that the tag helper is for</param>
    /// <param name="attributes">Attributes for the tag helper</param>
    /// <param name="childContent">Child content of the tag helper</param>
    /// <returns>A task returning a <see cref="TagHelperOutput"/></returns>
    public static async Task<TagHelperOutput> Render(this TagHelper helper, string tagName = "div", TagHelperAttributeList? attributes = null, HtmlString? childContent = null)
    {
        attributes ??= new TagHelperAttributeList();

        var context = new TagHelperContext(
            tagName,
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            tagName,
            attributes,
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent.SetHtmlContent(childContent));
            });

        await helper.ProcessAsync(context, output);

        return output;
    }

    /// <summary>
    ///     Renders tag helper output to a string.
    /// </summary>
    /// <param name="tagOutput">The output of a tag helper</param>
    /// <remarks>
    ///     Since this method uses the <see cref="HtmlTestEncoder"/> from the framework,
    ///     any strings that will be HTML encoded before being sent to the client will be
    ///     wrapped in <code>HtmlEncode[[]]</code>.
    /// </remarks>
    /// <returns>A string of the rendered tag helper.</returns>
    public static string Render(this TagHelperOutput tagOutput)
    {
        var writer = new StringWriter();
        var encoder = new HtmlTestEncoder();
        tagOutput.WriteTo(writer, encoder);

        return writer.ToString();
    }

    /// <summary>
    ///     Asserts that the output of a tag helper references a specified CSS class
    /// </summary>
    /// <param name="output">The tag helper output</param>
    /// <param name="className">The class name</param>
    public static void AssertContainsClass(this TagHelperOutput output, string className)
    {
        var tagClasses = output.Attributes["class"].Value.ToString()?.Split(' ');
        Assert.NotNull(tagClasses);
        Assert.NotEmpty(tagClasses);
        Assert.Contains(tagClasses, s => s == className);
    }
}