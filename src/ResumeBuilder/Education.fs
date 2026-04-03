[<AutoOpen>]
module ResumeBuilder.Education

open System
open Microsoft.Extensions.Localization
open Fun.Blazor
open FSharp.Data.Adaptive
open MudBlazor

let educationPage =
    html.inject (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        let selectedEducationIndex = cval -1
        let selectedEducation = cval None
        let education = cval (Education("", "", "", "", "", "", Uri "about:blank"))
        let school = cval ""
        let degree = cval ""
        let area = cval ""
        let grade = cval ""
        let period = cval ""
        let location = cval ""
        let website = cval ""

        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Educations"]
            }

            MudForm'' {
                adapt {
                    let! educations = store.Educations
                    let! getSelectedEducationIndex, setSelectedEducationIndex = selectedEducationIndex.WithSetter()
                    let! getSelectedEducation, setSelectedEducation = selectedEducation.WithSetter()
                    let! getEducation, setEducation = education.WithSetter()
                    let! getSchool, setSchool = school.WithSetter()
                    let! getDegree, setDegree = degree.WithSetter()
                    let! getArea, setArea = area.WithSetter()
                    let! getGrade, setGrade = grade.WithSetter()
                    let! getPeriod, setPeriod = period.WithSetter()
                    let! getLocation, setLocation = location.WithSetter()
                    let! getWebsite, setWebsite = website.WithSetter()

                    MudList'' {
                        type' Education
                        SelectedValue'(getSelectedEducation, setSelectedEducation)

                        SelectedValueChanged(fun item ->
                            match item with
                            | Some edu ->
                                let indexOption = educations |> List.tryFindIndex (fun x -> x = edu)

                                match indexOption with
                                | Some index ->
                                    setSelectedEducationIndex index
                                    setSelectedEducation item
                                    setEducation edu
                                    setSchool edu.School
                                    setDegree edu.Degree
                                    setArea edu.Area
                                    setGrade edu.Grade
                                    setPeriod edu.Period
                                    setLocation edu.Location
                                    setWebsite edu.Website.AbsoluteUri
                                | None -> ()
                            | None -> ())

                        educations
                        |> List.map (fun x ->
                            MudListItem'' {
                                Value(Some x)
                                Text $"{x.School} ({x.Period})"
                            })
                    }

                    MudTextField''<string> {
                        label' (string (localizer["School"]))
                        Variant Variant.Text
                        Value'(getSchool, setSchool)
                        Immediate true
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Degree"]))
                        Variant Variant.Text
                        Value'(getDegree, setDegree)
                        Immediate true
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Area"]))
                        Variant Variant.Text
                        Value'(getArea, setArea)
                        Immediate true
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Grade"]))
                        Variant Variant.Text
                        Value'(getGrade, setGrade)
                        Immediate true
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Location"]))
                        Variant Variant.Text
                        Value'(getLocation, setLocation)
                        Immediate true
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Period"]))
                        Variant Variant.Text
                        Value'(getPeriod, setPeriod)
                        Immediate true
                    }

                    MudTextField''<string> {
                        label' (string (localizer["Website"]))
                        Variant Variant.Text
                        Value'(getWebsite, setWebsite)
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

                            Disabled(
                                String.IsNullOrWhiteSpace getSchool
                                || String.IsNullOrWhiteSpace getDegree
                                || String.IsNullOrWhiteSpace getArea
                                || String.IsNullOrWhiteSpace getPeriod
                            )

                            OnClick(fun _ ->
                                let newEducation =
                                    Education(
                                        getSchool,
                                        getDegree,
                                        getArea,
                                        getGrade,
                                        getLocation,
                                        getPeriod,
                                        if String.IsNullOrWhiteSpace getWebsite then
                                            Uri "about:blank"
                                        else
                                            Uri getWebsite
                                    )

                                let exists = educations |> List.exists (fun x -> x = newEducation)

                                if not exists then
                                    store.Educations.Publish(educations @ [ newEducation ])
                                    setSchool ""
                                    setDegree ""
                                    setArea ""
                                    setGrade ""
                                    setPeriod ""
                                    setLocation ""
                                    setWebsite "")

                            localizer["Add"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedEducationIndex < 0)

                            OnClick(fun _ ->
                                match getSelectedEducation with
                                | None -> ()
                                | Some edu ->
                                    store.Educations.Publish(educations |> List.filter (fun x -> x <> edu))
                                    setSelectedEducation None
                                    setSelectedEducationIndex -1
                                    setSchool ""
                                    setDegree ""
                                    setArea ""
                                    setGrade ""
                                    setPeriod ""
                                    setLocation ""
                                    setWebsite "")

                            localizer["Delete"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedEducationIndex < 0)

                            OnClick(fun _ ->
                                if educations.Length > 0 && getSelectedEducationIndex > -1 then
                                    match getSelectedEducation with
                                    | Some edu ->
                                        let updatedEducation =
                                            Education(
                                                getSchool,
                                                getDegree,
                                                getArea,
                                                getGrade,
                                                getLocation,
                                                getPeriod,
                                                if String.IsNullOrWhiteSpace getWebsite then
                                                    Uri "about:blank"
                                                else
                                                    Uri getWebsite
                                            )

                                        store.Educations.Publish(
                                            educations
                                            |> List.mapi (fun i x ->
                                                if i = getSelectedEducationIndex then
                                                    updatedEducation
                                                else
                                                    x)
                                        )

                                        setSelectedEducation None
                                        setSelectedEducationIndex -1
                                        setSchool ""
                                        setDegree ""
                                        setArea ""
                                        setGrade ""
                                        setPeriod ""
                                        setLocation ""
                                        setWebsite ""

                                    | None -> ())

                            localizer["Update"]
                        }
                    }
                }
            }
        })
