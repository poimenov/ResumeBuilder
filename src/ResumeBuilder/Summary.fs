[<AutoOpen>]
module ResumeBuilder.Summary

open Microsoft.Extensions.Localization
open Fun.Blazor
open Tizzani.MudBlazor.HtmlEditor
open MudBlazor
open System.Collections.Generic

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

                        let attributes: IDictionary<string, obj> =
                            dict [ ("style", "max-height: calc(100dvh - 200px);") ]

                        MudHtmlEditor.create (
                            getSummary,
                            setSummary,
                            string (localizer["HtmlPlaceholder"]),
                            attributes
                        )
                    }
                }
            }
        })
