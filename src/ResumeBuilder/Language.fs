[<AutoOpen>]
module ResumeBuilder.Language

open System
open Microsoft.Extensions.Localization
open Fun.Blazor
open FSharp.Data.Adaptive
open Tizzani.MudBlazor.HtmlEditor
open MudBlazor

let languagePage =
    html.inject (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        let selectedLanguageIndex = cval -1
        let selectedLanguage = cval None
        let language = cval (Language("", "", 0))
        let langName = cval ""
        let fluency = cval ""
        let level = cval 0

        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Languages"]
            }

            MudCard'' {
                Outlined true

                adapt {
                    let! languages = store.Languages
                    let! getSelectedLanguageIndex, setSelectedLanguageIndex = selectedLanguageIndex.WithSetter()
                    let! getSelectedLanguage, setSelectedLanguage = selectedLanguage.WithSetter()
                    let! getLanguage, setLanguage = language.WithSetter()
                    let! getLangName, setLangName = langName.WithSetter()
                    let! getFluency, setFluency = fluency.WithSetter()
                    let! getLevel, setLevel = level.WithSetter()

                    MudCardContent'' {
                        MudList'' {
                            type' Language
                            SelectedValue'(getSelectedLanguage, setSelectedLanguage)
                            class' "list"

                            SelectedValueChanged(fun item ->
                                match item with
                                | Some lang ->
                                    let indexOption = languages |> List.tryFindIndex (fun x -> x = lang)

                                    match indexOption with
                                    | Some index ->
                                        setSelectedLanguageIndex index
                                        setSelectedLanguage item
                                        setLanguage lang
                                        setLangName lang.Name
                                        setFluency lang.Fluency
                                        setLevel lang.Level
                                    | None -> ()
                                | None -> ())

                            languages
                            |> List.map (fun x ->
                                MudListItem'' {
                                    class' "list-item"
                                    Value(Some x)
                                    title' x.Name
                                    Text x.Name
                                })
                        }

                        MudTextField''<string> {
                            label' (string (localizer["LanguageName"]))
                            Variant Variant.Text
                            Value'(getLangName, setLangName)
                        }

                        MudTextField''<string> {
                            label' (string (localizer["Fluency"]))
                            Variant Variant.Text
                            Value'(getFluency, setFluency)
                        }

                        MudInputLabel'' {
                            style' "margin-top: 16px;"
                            localizer["Level"]
                        }

                        MudRating'' {
                            MaxValue 5
                            SelectedValue'(getLevel, setLevel)
                        }
                    }

                    MudCardActions'' {
                        class' "card-actions"

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small

                            Disabled(String.IsNullOrWhiteSpace getLangName || getLevel < 1)

                            OnClick(fun _ ->
                                let newLanguage = Language(getLangName, getFluency, getLevel)

                                let exists =
                                    languages
                                    |> List.exists (fun x ->
                                        x.Name.Equals(getLangName, StringComparison.InvariantCultureIgnoreCase))

                                if not exists then
                                    store.Languages.Publish(languages @ [ newLanguage ])
                                    setLangName ""
                                    setFluency ""
                                    setLevel 1)

                            localizer["Add"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedLanguageIndex < 0)

                            OnClick(fun _ ->
                                match getSelectedLanguage with
                                | None -> ()
                                | Some lang ->
                                    store.Languages.Publish(languages |> List.filter (fun x -> x <> lang))
                                    setLangName ""
                                    setFluency ""
                                    setLevel 1)

                            localizer["Delete"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedLanguageIndex < 0)

                            OnClick(fun _ ->
                                if languages.Length > 0 && getSelectedLanguageIndex > -1 then
                                    let updatedLanguage = Language(getLangName, getFluency, getLevel)

                                    store.Languages.Publish(
                                        languages
                                        |> List.mapi (fun i x ->
                                            if i = getSelectedLanguageIndex then updatedLanguage else x)
                                    )

                                    setLangName ""
                                    setFluency ""
                                    setLevel 1)

                            localizer["Update"]
                        }
                    }

                }
            }
        })
