﻿using Microsoft.Extensions.WebEncoders.Testing;

namespace ICG.AspNetCore.Utilities.UnitTesting.TagHelpers;
#nullable enable

public static class TagHelperExtensions
{
    public static async Task<TagHelperOutput> Render(this TagHelper helper, string tagName = "div", HtmlString childContent = null)
    {
        var context = BaseTagHelperTest.MakeTagHelperContext();
        var output = BaseTagHelperTest.MakeTagHelperOutput("", childContent: childContent);

        await helper.ProcessAsync(context, output);

        return output;
    }

    public static string Render(this TagHelperOutput tagOutput)
    {
        var writer = new StringWriter();
        var encoder = new HtmlTestEncoder();
        tagOutput.WriteTo(writer, encoder);

        return writer.ToString();
    }

    public static void AssertContainsClass(this TagHelperOutput output, string className)
    {

        var tagClasses = output.Attributes["class"].Value.ToString()?.Split(' ');
        Assert.Contains(tagClasses, s => s == className);
    }
}