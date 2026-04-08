[<AutoOpen>]
module ResumeBuilder.AppMenu

open System
open System.IO
open System.Linq
open MudBlazor
open Fun.Blazor
open FSharp.Data.Adaptive

let appMenu (services: IServices) =
    let templates =
        services.RazorEngineService.GetTemplates()
        |> Array.map (fun t -> Path.GetFileNameWithoutExtension t.Name)
        |> Array.sort

    if services.Store.Template.Value = "" && templates.Length > 0 then
        services.Store.Template.Publish(templates.First())

    let isCurrentCulture (name: string) =
        if services.Options.Value.CultureName = name then
            Icons.Material.Filled.Check
        else
            null

    let setCurrentCulture (name: string) =
        services.Options.Value.CultureName <- name
        lock services.Options.Value (fun () -> services.Options.Value.Save())

    let resume (store: IShareStore) =
        { Name = store.Name.Value
          Picture = store.Picture.Value
          Headline = store.Headline.Value
          Email = store.Email.Value
          Phone = store.Phone.Value
          Location = store.Location.Value
          Summary = store.Summary.Value
          Links = store.Links.Value
          Educations = store.Educations.Value
          Certifications = store.Certifications.Value
          Skills = store.Skills.Value
          Experiences = store.Experiences.Value
          Languages = store.Languages.Value }

    let publish (store: IShareStore, resume: Resume) =
        transact (fun () ->
            store.Name.Value <- resume.Name
            store.Headline.Value <- resume.Headline
            store.Picture.Value <- resume.Picture
            store.Email.Value <- resume.Email
            store.Phone.Value <- resume.Phone
            store.Location.Value <- resume.Location
            store.Summary.Value <- resume.Summary
            store.Links.Value <- resume.Links
            store.Educations.Value <- resume.Educations
            store.Certifications.Value <- resume.Certifications
            store.Skills.Value <- resume.Skills
            store.Experiences.Value <- resume.Experiences
            store.Languages.Value <- resume.Languages)

    let clear (store: IShareStore) =
        transact (fun () ->
            store.Name.Value <- ""
            store.Headline.Value <- ""
            store.Picture.Value <- defaultPicture
            store.Email.Value <- ""
            store.Phone.Value <- ""
            store.Location.Value <- ""
            store.Summary.Value <- ""
            store.Links.Value <- []
            store.Educations.Value <- []
            store.Certifications.Value <- []
            store.Skills.Value <- []
            store.Experiences.Value <- []
            store.Languages.Value <- [])

    MudMenu'' {
        label' (string (services.Localizer["MainMenu"]))
        StartIcon Icons.Material.Filled.Settings
        style' "width: 180px"
        RelativeWidth DropdownWidth.Relative
        AnchorOrigin Origin.BottomLeft
        TransformOrigin Origin.BottomLeft
        Dense true

        MudMenuItem'' {
            label' (string (services.Localizer["LoadXML"]))
            Icon Icons.Material.Filled.FileOpen

            OnClick(fun _ ->
                task {
                    let! file =
                        services.OpenDialogService.OpenFileAsync(
                            title = string (services.Localizer["SelectXmlFile"]),
                            multiSelect = false,
                            filters = [| struct ("XML files", [| "*.xml" |]) |]
                        )
                        |> Async.AwaitTask

                    if file.Any() then
                        let! xmlContent = File.ReadAllTextAsync(file.First()) |> Async.AwaitTask
                        let resume = ResumeSerializer.FromXml xmlContent
                        publish (services.Store, resume)

                }
                |> ignore)
        }

        MudMenuItem'' {
            label' (string (services.Localizer["ClearAll"]))
            Icon Icons.Material.Filled.ClearAll

            OnClick(fun _ -> task { clear services.Store } |> ignore)
        }


        MudMenu'' {
            label' (string (services.Localizer["SaveAs"]))
            StartIcon Icons.Material.Filled.SaveAs
            AnchorOrigin Origin.BottomLeft
            TransformOrigin Origin.BottomRight

            MudMenuItem'' {
                label' $"""XML {services.Localizer["File"]}"""

                OnClick(fun _ ->
                    task {
                        let! file =
                            services.OpenDialogService.SaveFileAsync(
                                title = string (services.Localizer["SelectXmlFile"]),
                                filters = [| struct ("XML files", [| "*.xml" |]) |]
                            )
                            |> Async.AwaitTask

                        if not (String.IsNullOrEmpty file) then
                            let xmlContent = ResumeSerializer.ToXml(resume services.Store)
                            do! File.WriteAllTextAsync(file, xmlContent) |> Async.AwaitTask
                    }
                    |> ignore)
            }

            MudMenuItem'' {
                label' $"""HTML {services.Localizer["File"]}"""

                OnClick(fun _ ->
                    task {
                        let! file =
                            services.OpenDialogService.SaveFileAsync(
                                title = string (services.Localizer["SelectHtmlFile"]),
                                filters = [| struct ("HTML files", [| "*.html" |]) |]
                            )
                            |> Async.AwaitTask

                        if not (String.IsNullOrEmpty file) then
                            let! htmlContent =
                                services.RazorEngineService.RenderAsync(
                                    services.Store.Template.Value,
                                    resume services.Store
                                )

                            do! File.WriteAllTextAsync(file, htmlContent) |> Async.AwaitTask
                    }
                    |> ignore)

            }

            MudMenuItem'' {
                label' $"""PDF {services.Localizer["File"]}"""

                OnClick(fun _ ->
                    task {
                        let! file =
                            services.OpenDialogService.SaveFileAsync(
                                title = string (services.Localizer["SelectPdfFile"]),
                                filters = [| struct ("PDF files", [| "*.pdf" |]) |]
                            )
                            |> Async.AwaitTask

                        if not (String.IsNullOrEmpty file) then
                            do!
                                services.GeneratePdfService.CreateAsync(
                                    services.Store.Template.Value,
                                    resume services.Store,
                                    file
                                )
                                |> Async.AwaitTask
                    }
                    |> ignore)
            }
        }

        adapt {
            let! isDarkMode = services.Store.IsDarkMode

            let darkLightModeButtonIcon, label =
                if isDarkMode then
                    Icons.Material.Rounded.LightMode, string (services.Localizer["LightMode"])
                else
                    Icons.Material.Outlined.DarkMode, string (services.Localizer["DarkMode"])

            MudMenu'' {
                label' (string (services.Localizer["Template"]))
                StartIcon Icons.Material.Filled.FileCopy
                AnchorOrigin Origin.BottomLeft
                TransformOrigin Origin.BottomRight

                templates
                |> Array.map (fun template ->
                    MudMenuItem'' {
                        label' template

                        Icon(
                            if services.Store.Template.Value = template then
                                Icons.Material.Filled.Check
                            else
                                null
                        )

                        OnClick(fun _ -> services.Store.Template.Publish template)
                    })
            }

            MudMenu'' {
                label' (string (services.Localizer["Language"]))
                StartIcon Icons.Material.Filled.Language
                title' (string (services.Localizer["NeedsAppRestart"]))
                AnchorOrigin Origin.BottomLeft
                TransformOrigin Origin.BottomRight

                MudMenuItem'' {
                    label' (string (services.Localizer["English"]))
                    Icon(isCurrentCulture "en-US")
                    OnClick(fun _ -> setCurrentCulture "en-US")
                }

                MudMenuItem'' {
                    label' (string (services.Localizer["Russian"]))
                    Icon(isCurrentCulture "ru-RU")
                    OnClick(fun _ -> setCurrentCulture "ru-RU")
                }
            }

            MudMenuItem'' {
                label' label
                Icon darkLightModeButtonIcon

                OnClick(fun _ ->
                    let newMode = not isDarkMode
                    services.Store.IsDarkMode.Publish newMode
                    services.Options.Value.IsDarkMode <- newMode
                    lock services.Options.Value (fun () -> services.Options.Value.Save()))

            }

            MudMenuItem'' {
                Icon Icons.Custom.Brands.GitHub
                label' (string (services.Localizer["SourceCode"]))
                OnClick(fun _ -> services.LinkOpeningService.OpenUrl "https://github.com/poimenov/ResumeBuilder")
            }
        }
    }
