using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;

namespace ccode.Shared;

public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public static void Render(string markdown)
    {
        var document = Markdown.Parse(markdown, Pipeline);
        foreach (var block in document)
            RenderBlock(block);
    }

    private static void RenderBlock(Block block)
    {
        switch (block)
        {
            case HeadingBlock heading:
                var level = heading.Level;
                var headingText = InlinesToString(heading.Inline);
                var color = level == 1 ? "bold yellow" : level == 2 ? "bold cyan" : "bold";
                AnsiConsole.MarkupLine($"[{color}]{Markup.Escape(headingText)}[/]");
                break;

            case ParagraphBlock paragraph:
                var text = InlinesToMarkup(paragraph.Inline);
                AnsiConsole.MarkupLine(text);
                break;

            case FencedCodeBlock code:
                var panel = new Panel(Markup.Escape(code.Lines.ToString()))
                    .BorderColor(Color.Grey)
                    .Expand();
                AnsiConsole.Write(panel);
                break;

            case ListBlock list:
                var index = 1;
                foreach (var item in list)
                {
                    if (item is ListItemBlock listItem)
                    {
                        var prefix = list.IsOrdered ? $"{index++}." : "•";
                        var content = string.Join(" ", listItem
                            .OfType<ParagraphBlock>()
                            .Select(p => InlinesToString(p.Inline)));
                        AnsiConsole.MarkupLine($"  {prefix} {Markup.Escape(content)}");
                    }
                }
                break;

            case QuoteBlock quote:
                foreach (var inner in quote)
                    RenderBlock(inner);
                break;

            case ThematicBreakBlock:
                AnsiConsole.Write(new Rule());
                break;
        }

        AnsiConsole.WriteLine();
    }

    private static string InlinesToString(ContainerInline? inlines)
    {
        if (inlines is null) return string.Empty;
        var result = new System.Text.StringBuilder();
        foreach (var inline in inlines)
        {
            result.Append(inline switch
            {
                LiteralInline literal => literal.Content.ToString(),
                CodeInline code => code.Content,
                EmphasisInline emphasis => InlinesToString(emphasis),
                LineBreakInline => " ",
                _ => string.Empty
            });
        }
        return result.ToString();
    }

    private static string InlinesToMarkup(ContainerInline? inlines)
    {
        if (inlines is null) return string.Empty;
        var result = new System.Text.StringBuilder();
        foreach (var inline in inlines)
        {
            result.Append(inline switch
            {
                LiteralInline literal => Markup.Escape(literal.Content.ToString()),
                CodeInline code => $"[yellow]{Markup.Escape(code.Content)}[/]",
                EmphasisInline { DelimiterChar: '*' or '_', DelimiterCount: 2 } bold
                    => $"[bold]{Markup.Escape(InlinesToString(bold))}[/]",
                EmphasisInline emphasis => $"[italic]{Markup.Escape(InlinesToString(emphasis))}[/]",
                LineBreakInline => "\n",
                LinkInline link => $"[link]{Markup.Escape(link.Url ?? string.Empty)}[/]",
                _ => string.Empty
            });
        }
        return result.ToString();
    }
}
