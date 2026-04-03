[<AutoOpen>]
module ResumeBuilder.Links

open System
open MudBlazor
open Microsoft.Extensions.Localization
open FSharp.Data.Adaptive
open Fun.Blazor

let linksPage =
    html.inject (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        let selectedLinkIndex = cval -1
        let selectedLink = cval None
        let link = cval ""

        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Links"]
            }

            MudForm'' {
                adapt {
                    let! links = store.Links
                    let! getSelectedLinkIndex, setSelectedLinkIndex = selectedLinkIndex.WithSetter()
                    let! getSelectedLink, setSelectedLink = selectedLink.WithSetter()
                    let! getLink, setLink = link.WithSetter()

                    MudList'' {
                        type' (string option)
                        SelectedValue'(getSelectedLink, setSelectedLink)

                        SelectedValueChanged(fun item ->
                            match item with
                            | Some str ->
                                let indexOption = links |> List.tryFindIndex (fun x -> x = str)

                                match indexOption with
                                | Some index ->
                                    setSelectedLinkIndex index
                                    setSelectedLink item
                                    setLink str
                                | None -> ()
                            | None -> ())

                        links
                        |> List.map (fun x ->
                            MudListItem'' {
                                Value(Some x)
                                Text x
                            })

                    }

                    MudTextField''<string> {
                        label' (string (localizer["Link"]))
                        Variant Variant.Text
                        Value'(getLink, setLink)
                        Immediate true

                        Validation(
                            Func<string, string>(fun v ->
                                if String.IsNullOrEmpty v || isValidUrl v then
                                    null
                                else
                                    string (localizer["InvalidUrlMessage"]))
                        )
                    }

                    MudDivider'' { style' "margin: 5px 0px 5px 0px;" }

                    MudStack'' {
                        Row true
                        Justify Justify.FlexEnd

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(String.IsNullOrWhiteSpace getLink)

                            OnClick(fun _ ->
                                let exists =
                                    links
                                    |> List.exists (fun x ->
                                        x.Equals(getLink, StringComparison.InvariantCultureIgnoreCase))

                                if not exists && isValidUrl getLink then
                                    store.Links.Publish(links @ [ getLink ])
                                    setLink ""

                            )

                            localizer["Add"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedLinkIndex < 0)

                            OnClick(fun _ ->
                                match getSelectedLink with
                                | None -> ()
                                | Some _link ->
                                    store.Links.Publish(links |> List.filter (fun x -> x <> _link))
                                    setLink ""
                                    setSelectedLink None
                                    setSelectedLinkIndex -1)

                            localizer["Delete"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedLinkIndex < 0)

                            OnClick(fun _ ->
                                let indexOption = links |> List.tryFindIndex (fun x -> x = getLink)

                                if links.Length > 0 && getSelectedLinkIndex > -1 then
                                    match indexOption with
                                    | Some _ -> ()
                                    | None ->
                                        store.Links.Publish(
                                            links
                                            |> List.mapi (fun i x -> if i = getSelectedLinkIndex then getLink else x)
                                        )

                                    setLink ""
                                    setSelectedLink None
                                    setSelectedLinkIndex -1)

                            localizer["Update"]
                        }
                    }
                }
            }
        })
