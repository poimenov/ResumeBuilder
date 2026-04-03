[<AutoOpen>]
module ResumeBuilder.Certification

open System
open Microsoft.Extensions.Localization
open Fun.Blazor
open FSharp.Data.Adaptive
open MudBlazor

let certificationPage =
    html.inject (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        let selectedCertificationIndex = cval -1
        let selectedCertification = cval None
        let certification = cval (Certification("", "", "", "", Uri "about:blank"))
        let title = cval ""
        let issuer = cval ""
        let date = cval ""
        let label = cval ""
        let website = cval ""


        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Certifications"]
            }

            MudCard'' {
                Outlined true

                adapt {
                    let! certifications = store.Certifications

                    let! getSelectedCertificationIndex, setSelectedCertificationIndex =
                        selectedCertificationIndex.WithSetter()

                    let! getSelectedCertification, setSelectedCertification = selectedCertification.WithSetter()
                    let! getCertification, setCertification = certification.WithSetter()
                    let! getTitle, setTitle = title.WithSetter()
                    let! getIssuer, setIssuer = issuer.WithSetter()
                    let! getDate, setDate = date.WithSetter()
                    let! getLabel, setLabel = label.WithSetter()
                    let! getWebsite, setWebsite = website.WithSetter()

                    MudCardContent'' {
                        MudList'' {
                            type' Certification
                            SelectedValue'(getSelectedCertification, setSelectedCertification)
                            class' "list"

                            SelectedValueChanged(fun item ->
                                match item with
                                | Some cert ->
                                    let indexOption = certifications |> List.tryFindIndex (fun x -> x = cert)

                                    match indexOption with
                                    | Some index ->
                                        setSelectedCertificationIndex index
                                        setSelectedCertification item
                                        setCertification cert
                                        setTitle cert.Title
                                        setIssuer cert.Issuer
                                        setDate cert.Date
                                        setLabel cert.Label
                                        setWebsite cert.Website.AbsoluteUri
                                    | None -> ()
                                | None -> ())

                            certifications
                            |> List.map (fun cert ->
                                MudListItem'' {
                                    class' "list-item"
                                    Value(Some cert)
                                    title' cert.Title
                                    Text cert.Title
                                })

                        }

                        MudTextField''<string> {
                            label' (string localizer["Title"])
                            Variant Variant.Text
                            Value'(getTitle, setTitle)
                            Immediate true
                        }

                        MudTextField''<string> {
                            label' (string localizer["Issuer"])
                            Variant Variant.Text
                            Value'(getIssuer, setIssuer)
                            Immediate true
                        }

                        MudTextField''<string> {
                            label' (string localizer["Date"])
                            Variant Variant.Text
                            Value'(getDate, setDate)
                            Immediate true
                        }

                        MudTextField''<string> {
                            label' (string localizer["Label"])
                            Variant Variant.Text
                            Value'(getLabel, setLabel)
                            Immediate true
                        }

                        MudTextField''<string> {
                            label' (string localizer["Website"])
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

                    }

                    MudCardActions'' {
                        class' "card-actions"

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small

                            Disabled(
                                String.IsNullOrWhiteSpace getTitle
                                || String.IsNullOrWhiteSpace getIssuer
                                || String.IsNullOrWhiteSpace getDate
                                || String.IsNullOrWhiteSpace getLabel
                            )

                            OnClick(fun _ ->
                                let newCertification =
                                    Certification(
                                        getTitle,
                                        getIssuer,
                                        getDate,
                                        getLabel,
                                        if String.IsNullOrWhiteSpace getWebsite then
                                            Uri "about:blank"
                                        else
                                            Uri getWebsite
                                    )

                                let exists = certifications |> List.exists (fun c -> c = newCertification)

                                if not exists then
                                    store.Certifications.Publish(certifications @ [ newCertification ])
                                    setTitle ""
                                    setIssuer ""
                                    setDate ""
                                    setLabel ""
                                    setWebsite "")

                            localizer["Add"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedCertificationIndex < 0)

                            OnClick(fun _ ->
                                match getSelectedCertification with
                                | None -> ()
                                | Some cert ->
                                    store.Certifications.Publish(certifications |> List.filter (fun c -> c <> cert))
                                    setSelectedCertification None
                                    setSelectedCertificationIndex -1
                                    setTitle ""
                                    setIssuer ""
                                    setDate ""
                                    setLabel ""
                                    setWebsite "")

                            localizer["Delete"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedCertificationIndex < 0)

                            OnClick(fun _ ->
                                if certifications.Length > 0 && getSelectedCertificationIndex > -1 then
                                    match getSelectedCertification with
                                    | None -> ()
                                    | Some cert ->
                                        let updatedCertification =
                                            Certification(
                                                getTitle,
                                                getIssuer,
                                                getDate,
                                                getLabel,
                                                if String.IsNullOrWhiteSpace getWebsite then
                                                    Uri "about:blank"
                                                else
                                                    Uri getWebsite
                                            )

                                        store.Certifications.Publish(
                                            certifications
                                            |> List.map (fun c -> if c = cert then updatedCertification else c)
                                        )

                                        setSelectedCertification None
                                        setSelectedCertificationIndex -1
                                        setTitle ""
                                        setIssuer ""
                                        setDate ""
                                        setLabel ""
                                        setWebsite "")

                            localizer["Update"]
                        }
                    }

                }
            }
        })
