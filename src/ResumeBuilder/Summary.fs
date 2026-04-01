[<AutoOpen>]
module ResumeBuilder.Summary

open System
open Microsoft.Extensions.Localization
open Fun.Blazor
open Tizzani.MudBlazor.HtmlEditor
open Microsoft.AspNetCore.Components

type MudHtmlToolbarOptions with
    static member create() =
        html.blazor (
            ComponentAttrBuilder<MudHtmlToolbarOptions>()
                .Add((fun c -> c.BackgroundColorPicker), false)
                .Add((fun c -> c.ForegroundColorPicker), false)
                .Add((fun c -> c.InsertImage), false)
                .Add((fun c -> c.TypographyPicker), false)
                .Add((fun c -> c.Align), false)
                .Add((fun c -> c.Indent), false)
                .Add((fun c -> c.Blockquote), false)
                .Add((fun c -> c.CodeBlock), false)
                .Add((fun c -> c.InsertLink), false)
        )

type MudHtmlEditor with
    static member create(value, onValueChanged) =
        html.blazor (
            ComponentAttrBuilder<MudHtmlEditor>()
                .Add((fun c -> c.Html), value)
                .Add((fun x -> x.HtmlChanged), EventCallback<string>(null, Action<string> onValueChanged))
                .Add((fun c -> c.ChildContent), MudHtmlToolbarOptions.create () |> html.renderFragment)
        )


let summaryPage =
    html.inject (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Summary"]
            }

            adapt {
                let! summary = store.Summary.WithSetter()

                MudHtmlEditor.create summary

            }
        })
