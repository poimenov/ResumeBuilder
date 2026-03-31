[<AutoOpen>]
module ResumeBuilder.BasicInfo

open System
open System.IO
open System.Linq
open MudBlazor
open Microsoft.Extensions.Localization
open Fun.Blazor

let getPictureSource (path: string) : string =
    if String.IsNullOrWhiteSpace path then
        defaultPicture
    else
        try
            let uri = Uri path
            let filePath = if uri.IsFile then uri.LocalPath else path

            if File.Exists filePath then
                let bytes = File.ReadAllBytes filePath

                let mime =
                    match Path.GetExtension(filePath).ToLowerInvariant() with
                    | ".jpg"
                    | ".jpeg" -> "image/jpeg"
                    | ".png" -> "image/png"
                    | ".gif" -> "image/gif"
                    | ".bmp" -> "image/bmp"
                    | _ -> "application/octet-stream"

                let base64 = Convert.ToBase64String bytes
                sprintf "data:%s;base64,%s" mime base64
            else
                defaultPicture
        with _ ->
            defaultPicture

let basicInfoPage =
    html.inject
        (fun (store: IShareStore, openDialogService: IOpenDialogService, localizer: IStringLocalizer<SharedResources>) ->
            fragment {
                SectionContent'' {
                    SectionName "Title"
                    localizer["BasicInfo"]
                }

                MudForm'' {
                    adapt {
                        let! userName = store.Name.WithSetter()
                        let! headline = store.Headline.WithSetter()
                        let! location = store.Location.WithSetter()
                        let! email = store.Email.WithSetter()
                        let! phone = store.Phone.WithSetter()
                        let! picture = store.Picture

                        MudImage'' {
                            Height 140
                            Width 140
                            Elevation 25
                            class' "rounded-lg profile-img"
                            alt (string (localizer["ProfilePicture"]))
                            src picture
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Primary
                            Size Size.Small
                            class' "select-picture"

                            OnClick(fun _ ->
                                task {
                                    let jpgFilter = struct ("JPG files", [| "*.jpg" |])
                                    let jpegFilter = struct ("JPEG files", [| "*.jpeg" |])
                                    let pngFilter = struct ("PNG files", [| "*.png" |])
                                    let gifFilter = struct ("GIF files", [| "*.gif" |])
                                    let bmpFilter = struct ("BMP files", [| "*.bmp" |])

                                    let! file =
                                        openDialogService.OpenFileAsync(
                                            title = string (localizer["SelectImgFile"]),
                                            multiSelect = false,
                                            filters = [| jpgFilter; jpegFilter; pngFilter; gifFilter; bmpFilter |]
                                        )
                                        |> Async.AwaitTask

                                    if file.Any() then
                                        let pictureSource = file.First() |> getPictureSource
                                        store.Picture.Publish pictureSource
                                })

                            localizer["SelectPicture"]
                        }

                        MudTextField'' {
                            label' (string (localizer["Name"]))
                            Variant Variant.Text
                            Value' userName
                            Immediate true
                        }

                        MudTextField'' {
                            label' (string (localizer["Headline"]))
                            Variant Variant.Text
                            Value' headline
                            Immediate true
                        }

                        MudTextField'' {
                            label' (string (localizer["Location"]))
                            Variant Variant.Text
                            Value' location
                            Immediate true
                        }

                        MudTextField'' {
                            label' (string (localizer["Email"]))
                            Variant Variant.Text
                            Value' email
                            Immediate true

                            Validation(
                                Func<string, string>(fun v ->
                                    if String.IsNullOrEmpty v || emailRegex.IsMatch v then
                                        null
                                    else
                                        string (localizer["InvalidEmailMessage"]))
                            )
                        }

                        MudTextField'' {
                            label' (string (localizer["Phone"]))
                            Variant Variant.Text
                            Value' phone
                            Immediate true

                            Validation(
                                Func<string, string>(fun v ->
                                    if String.IsNullOrEmpty v || phoneRegex.IsMatch v then
                                        null
                                    else
                                        string (localizer["InvalidPhoneMessage"]))
                            )
                        }
                    }
                }
            })
