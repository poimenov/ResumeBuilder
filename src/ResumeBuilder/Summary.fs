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

            MudCard'' {
                Outlined true

                MudCardContent'' {

                    adapt {
                        let! getSummary, setSummary = store.Summary.WithSetter()

                        MudHtmlEditor.create (getSummary, setSummary, string (localizer["HtmlPlaceholder"]))
                    }
                }
            }
        })
