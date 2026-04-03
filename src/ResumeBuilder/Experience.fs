[<AutoOpen>]
module ResumeBuilder.Experience

open System
open Microsoft.Extensions.Localization
open Fun.Blazor
open FSharp.Data.Adaptive
open Tizzani.MudBlazor.HtmlEditor
open MudBlazor

let experiencePage =
    html.inject (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        let selectedExperienceIndex = cval -1
        let selectedExperience = cval None
        let experience = cval (Experience("", Uri "about:blank", "", "", "", ""))
        let company = cval ""
        let website = cval ""
        let position = cval ""
        let location = cval ""
        let period = cval ""
        let description = cval ""

        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Experiences"]
            }

            MudForm'' {
                adapt {
                    let! experiences = store.Experiences
                    let! getSelectedExperienceIndex, setSelectedExperienceIndex = selectedExperienceIndex.WithSetter()
                    let! getSelectedExperience, setSelectedExperience = selectedExperience.WithSetter()
                    let! getExperience, setExperience = experience.WithSetter()
                    let! getCompany, setCompany = company.WithSetter()
                    let! getWebsite, setWebsite = website.WithSetter()
                    let! getPosition, setPosition = position.WithSetter()
                    let! getLocation, setLocation = location.WithSetter()
                    let! getPeriod, setPeriod = period.WithSetter()
                    let! getDescription, setDescription = description.WithSetter()

                    MudList'' {
                        type' Experience
                        SelectedValue'(getSelectedExperience, setSelectedExperience)

                        SelectedValueChanged(fun item ->
                            match item with
                            | Some exp ->
                                let indexOption = experiences |> List.tryFindIndex (fun x -> x = exp)

                                match indexOption with
                                | Some index ->
                                    setSelectedExperienceIndex index
                                    setSelectedExperience item
                                    setExperience exp
                                    setCompany exp.Company
                                    setWebsite exp.Website.AbsoluteUri
                                    setPosition exp.Position
                                    setLocation exp.Location
                                    setPeriod exp.Period
                                    setDescription exp.Description
                                | None -> ()
                            | None -> ())

                        experiences
                        |> List.map (fun x ->
                            MudListItem'' {
                                Value(Some x)
                                Text $"{x.Company} ({x.Period})"
                            })

                    }

                    MudTextField''<string> {
                        label' (string (localizer["Company"]))
                        Variant Variant.Text
                        Value'(getCompany, setCompany)
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Website"]))
                        Variant Variant.Text
                        Value'(getWebsite, setWebsite)

                        Validation(
                            Func<string, string>(fun v ->
                                if String.IsNullOrEmpty v || isValidUrl v then
                                    null
                                else
                                    string (localizer["InvalidUrlMessage"]))
                        )
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Position"]))
                        Variant Variant.Text
                        Value'(getPosition, setPosition)
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Location"]))
                        Variant Variant.Text
                        Value'(getLocation, setLocation)
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Period"]))
                        Variant Variant.Text
                        Value'(getPeriod, setPeriod)
                    }

                    MudInputLabel'' {
                        style' "margin-top: 16px;"
                        localizer["Description"]
                    }

                    MudHtmlEditor.create (getDescription, setDescription, string (localizer["HtmlPlaceholder"]))

                    MudDivider'' { style' "margin: 5px 0px 5px 0px;" }

                    MudStack'' {
                        Row true
                        Justify Justify.FlexEnd

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small

                            Disabled(
                                String.IsNullOrWhiteSpace getCompany
                                || String.IsNullOrWhiteSpace getPosition
                                || String.IsNullOrWhiteSpace getPeriod
                            )

                            OnClick(fun _ ->
                                let newExperience =
                                    Experience(
                                        getCompany,
                                        (if String.IsNullOrWhiteSpace getWebsite then
                                             Uri "about:blank"
                                         else
                                             Uri getWebsite),
                                        getPosition,
                                        getLocation,
                                        getPeriod,
                                        getDescription
                                    )

                                let exists =
                                    experiences
                                    |> List.exists (fun x ->
                                        x.Company.Equals(getCompany, StringComparison.InvariantCultureIgnoreCase)
                                        && x.Period.Equals(getPeriod, StringComparison.InvariantCultureIgnoreCase))

                                if not exists then
                                    store.Experiences.Publish(experiences @ [ newExperience ])
                                    setCompany ""
                                    setWebsite ""
                                    setPosition ""
                                    setLocation ""
                                    setPeriod ""
                                    setDescription "")

                            localizer["Add"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedExperienceIndex < 0)

                            OnClick(fun _ ->
                                match getSelectedExperience with
                                | None -> ()
                                | Some exp ->
                                    store.Experiences.Publish(experiences |> List.filter (fun x -> x <> exp))
                                    setCompany ""
                                    setWebsite ""
                                    setPosition ""
                                    setLocation ""
                                    setPeriod ""
                                    setDescription ""
                                    setSelectedExperience None
                                    setSelectedExperienceIndex -1)

                            localizer["Delete"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedExperienceIndex < 0)

                            OnClick(fun _ ->
                                if experiences.Length > 0 && getSelectedExperienceIndex > -1 then
                                    let updatedExperience =
                                        Experience(
                                            getCompany,
                                            (if String.IsNullOrWhiteSpace getWebsite then
                                                 Uri "about:blank"
                                             else
                                                 Uri getWebsite),
                                            getPosition,
                                            getLocation,
                                            getPeriod,
                                            getDescription
                                        )

                                    store.Experiences.Publish(
                                        experiences
                                        |> List.mapi (fun i x ->
                                            if i = getSelectedExperienceIndex then
                                                updatedExperience
                                            else
                                                x)
                                    )

                                    setCompany ""
                                    setWebsite ""
                                    setPosition ""
                                    setLocation ""
                                    setPeriod ""
                                    setDescription ""
                                    setSelectedExperience None
                                    setSelectedExperienceIndex -1)

                            localizer["Update"]
                        }
                    }
                }
            }

        })
