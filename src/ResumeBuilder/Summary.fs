[<AutoOpen>]
module ResumeBuilder.Summary

open Microsoft.Extensions.Localization
open Fun.Blazor
open Tizzani.MudBlazor.HtmlEditor
open MudBlazor

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
