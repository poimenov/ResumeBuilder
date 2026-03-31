[<AutoOpen>]
module ResumeBuilder.Services

open System
open System.Diagnostics
open System.IO
open System.Runtime.InteropServices
open System.Text.RegularExpressions
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open PuppeteerSharp
open PuppeteerSharp.Media
open RazorLight
open Fun.Blazor
open Microsoft.Extensions.Localization
open Microsoft.Extensions.Options
open System.Xml.Linq

type SharedResources() = class end
let emailRegex = Regex("^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)
let phoneRegex = Regex("^\+?[0-9\s\-()]+$", RegexOptions.Compiled)

let defaultPicture =
    let path =
        Path.Combine(AppSettings.WwwRootFolderPath, "images", "man-person-icon.svg")

    if File.Exists path then
        let base64 = Convert.ToBase64String(File.ReadAllBytes path)
        $"data:image/svg+xml;base64,{base64}"

    else
        "data:image/svg+xml;base64,PHN2ZyBpZD0nTGF5ZXJfMScgZGF0YS1uYW1lPSdMYXllciAxJyB4bWxucz0naHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmcnIHZpZXdCb3g9JzAgMCAxMjAuOTIgMTIyLjg4Jz48ZGVmcz48c3R5bGU+LmNscy0xe2ZpbGwtcnVsZTpldmVub2RkO308L3N0eWxlPjwvZGVmcz48dGl0bGU+bWFuLXBlcnNvbjwvdGl0bGU+PHBhdGggY2xhc3M9J2Nscy0xJyBkPSdNNzAuNDMsNDYuOTJhMi42NCwyLjY0LDAsMSwxLTIuNjQsMi42NCwyLjY0LDIuNjQsMCwwLDEsMi42NC0yLjY0Wm0zLjQzLDI3LjgxYzQuMDgtMy44Myw3LjA5LTYuNjYsNi41MS0xMy41NGgwYTEuNjIsMS42MiwwLDAsMSwuMjYtMSwxLjU5LDEuNTksMCwwLDEsMi4yMS0uNDQsNC4zOSw0LjM5LDAsMCwwLC44My40MywyLjQyLDIuNDIsMCwwLDAsLjcuMTYsMy4xMSwzLjExLDAsMCwwLC42OSwwLDMuNDEsMy40MSwwLDAsMCwuMy0uNzRsMi01Ljc0Yy4zNi0xLjM2LjU4LTIuODgtMS4yNC0yLjc0YTUuNjQsNS42NCwwLDAsMC0yLjgzLDEuMTgsMS42MiwxLjYyLDAsMCwxLTEuMjUuMzIsMS41OSwxLjU5LDAsMCwxLTEuMy0xLjg0YzEuNS04Ljc1LjgxLTE0LjQ2LTEtMTguMzVhMTUuNjksMTUuNjksMCwwLDAtNy4wNy03QzY2LjM4LDMwLjIxLDYyLDMwLjc2LDU3LjU2LDMxLjNjLTMuNjQuNDUtNy4yNy45LTEyLjA5LDQuMjNhMTEuNjMsMTEuNjMsMCwwLDAtNC41OSw1Ljc0LDE0LjQxLDE0LjQxLDAsMCwwLS4xOSw3Ljg1LDEuNjMsMS42MywwLDAsMSwwLDEsMS42LDEuNiwwLDAsMS0yLDFsLS4yMy0uMDgtMS4yMy0uNDRjLTEuODgtLjY2LTMuMjItMS0zLjczLjIxLS4yNSwyLjQ0LS4yNCw4LDIuMDYsOS4zNWExLjc1LDEuNzUsMCwwLDAsLjkuMjMsNC4xOSw0LjE5LDAsMCwwLDEuMy0uMjEsMS44NiwxLjg2LDAsMCwxLC40OC0uMDksMS41OSwxLjU5LDAsMCwxLDEuNjIsMS41NWMuMTgsNy4yMSwzLjM4LDEwLDcuMjcsMTMuMzIuNTkuNTEsMS4yMSwxLDEuNjIsMS40MSw3LjM5LDYuNTcsMTYuNCw2LjkyLDIzLjU0LDBsMS42NS0xLjU2Wk01Ny43NCw2Mi42OGExLjIsMS4yLDAsMCwxLS40MS0uNzksMS4xNSwxLjE1LDAsMCwxLC4yNy0uODQsMS4xNywxLjE3LDAsMCwxLC44LS40MiwxLjE1LDEuMTUsMCwwLDEsLjg0LjI3LDEuODUsMS44NSwwLDAsMCwyLjQyLDAsMS4xNiwxLjE2LDAsMCwxLC44Ny0uMjgsMS4xOSwxLjE5LDAsMCwxLC43Ny40bC4wNSwwYTEuMjEsMS4yMSwwLDAsMSwuMjQuODMsMS4xOSwxLjE5LDAsMCwxLS40Mi43OSw0LjE4LDQuMTgsMCwwLDEtNS40MSwwWk0yNS45LDIxLjg4QzQyLDIuMDUsNjAuNDUtOC43Myw3NC4zNCw4LjkxYzE2Ljc0Ljg4LDIzLjQ1LDI3LjQ3LDEwLjEsMzguNjYsMCwuMjIsMCwuNDQtLjA4LjY2YTcuNDMsNy40MywwLDAsMSwxLjU1LS4zLDUsNSwwLDAsMSwyLjczLjUyLDMuODYsMy44NiwwLDAsMSwxLjksMi4zMSw3LDcsMCwwLDEtLjA5LDQsMS40MiwxLjQyLDAsMCwxLDAsLjE2bC0yLDUuNzZBNC4yNCw0LjI0LDAsMCwxLDg3LDYyLjgyYTMuOTMsMy45MywwLDAsMS0yLjgzLjY5bC0uNTgtLjA3QzgzLjUsNzAsODAuMjgsNzMuMDcsNzYuMDYsNzcsNzkuMzYsODguMjEsODcuMzUsOTAsOTUuMTMsOTEuNjRjMTAuNjgsMi4zMywyNS43OSwyLjYzLDI1Ljc5LDI0LjQzdjUuMjJhMS41OSwxLjU5LDAsMCwxLTEuNTksMS41OUgxLjU5QTEuNTksMS41OSwwLDAsMSwwLDEyMS4yOXYtNC43MUMwLDkzLjc5LDE1LjgyLDk0LjA5LDI3LjEsOTIuNGM4LjEzLTEuMjIsMTYuNDEtMi40NiwxOS42My0xMy42LS41OS0uNTEtMS4xNy0xLTEuNzctMS41NC00LjEzLTMuNTYtNy41OS02LjU0LTguMjUtMTMuOGgtLjM3YTQuOTEsNC45MSwwLDAsMS0yLjQzLS42NCw2LjY5LDYuNjksMCwwLDEtMi42OC0zLjI1LDE0LjgsMTQuOCwwLDAsMS0xLjA3LTQuODhjMC0uNTEsMC0xLjUyLDAtMi40OWEyMCwyMCwwLDAsMSwuMTMtMi4xLDEuMjYsMS4yNiwwLDAsMSwuMS0uMzhjLjg3LTIuNDIsMi4yLTMuMiwzLjk0LTMuMTVsLTEuMTUtLjc3QzMyLjU2LDM4LDM0LjM5LDI0LjQ1LDI1LjksMjEuODhabTI0LjI0LDU5LjlhOS4zMyw5LjMzLDAsMCwxLS44NC0uNTVjLTIuMTcsNi4yOC01Ljc2LDkuNTYtMTAsMTEuNDZBMzksMzksMCwwLDAsNjAuOCw5OC4zMiwzNy40OCwzNy40OCwwLDAsMCw4Mi41OCw5MC45Yy0zLjcyLTIuMS02LjkxLTUuNC05LTExLjE0LTYuMjcsNS43My0xNi4yOCw2LjQxLTIzLjQ3LDJabS4zNC0zNC44NmEyLjY0LDIuNjQsMCwxLDEtMi42NCwyLjY0LDIuNjQsMi42NCwwLDAsMSwyLjY0LTIuNjRaTTUxLjksNjYuNTlINjhjMS40OS0uMDYsMS44OC43MywxLjM4LDEuODItNC4zMSw5LjcyLTE4LjUsNC45My0xOC42OS0uMTMsMC0uNzUuMzYtMS42MywxLjItMS42OVptMjQuNTMtMjFBMS4xNSwxLjE1LDAsMSwxLDc0LjYsNDdhNS4xMyw1LjEzLDAsMCwwLTIuOTQtMi4xMiw2LjIsNi4yLDAsMCwwLTMuMzkuMzFBMS4xNSwxLjE1LDAsMSwxLDY3LjU0LDQzYzMuNTgtMS4yMSw2LjU4LS40Niw4Ljg5LDIuNjFaTTUzLjM4LDQzYTEuMTQsMS4xNCwwLDEsMS0uNzIsMi4xNyw2LjEyLDYuMTIsMCwwLDAtMy40LS4zQTUuMTksNS4xOSwwLDAsMCw0Ni4zMiw0N2ExLjE0LDEuMTQsMCwxLDEtMS44My0xLjM3YzIuMzEtMy4wOSw1LjMzLTMuODIsOC44OS0yLjYxWm0tLjI0LDI1LjEyaDE0Yy0xLjEzLDMuMzgtMTIuNzIsMy4zMS0xNCwwWicvPjwvc3ZnPg=="

let isValidUrl (url: string) =
    try
        let uri = new Uri(url)

        (uri.Scheme = Uri.UriSchemeHttp || uri.Scheme = Uri.UriSchemeHttps)
        && uri.IsAbsoluteUri
    with
    | :? UriFormatException -> false
    | _ -> false

[<Serializable>]
type Education
    (school: string, degree: string, area: string, grade: string, location: string, period: string, website: Uri) =
    member val School = school with get, set
    member val Degree = degree with get, set
    member val Area = area with get, set
    member val Grade = grade with get, set
    member val Location = location with get, set
    member val Period = period with get, set
    member val Website = website with get, set

[<Serializable>]
type Experience(company: string, website: Uri, position: string, location: string, period: string, description: string)
    =
    member val Company = company with get, set
    member val Website = website with get, set
    member val Position = position with get, set
    member val Location = location with get, set
    member val Period = period with get, set
    member val Description = description with get, set

[<Serializable>]
type Language(name: string, fluency: string, level: int) =
    member val Name = name with get, set
    member val Fluency = fluency with get, set
    member val Level = level with get, set

[<Serializable>]
type Skill(name: string, keywords: string list) =
    member val Name = name with get, set
    member val Keywords = keywords with get, set

[<Serializable>]
type Certification(title: string, issuer: string, date: string, label: string, website: Uri) =
    member val Title = title with get, set
    member val Issuer = issuer with get, set
    member val Date = date with get, set
    member val Label = label with get, set
    member val Website = website with get, set

type IShareStore with
    member store.Count = store.CreateCVal(nameof store.Count, 0)
    member store.DrawerOpen = store.CreateCVal(nameof store.DrawerOpen, true)
    member store.IsDarkMode = store.CreateCVal(nameof store.IsDarkMode, true)
    member store.Template = store.CreateCVal(nameof store.Template, "")
    member store.Name = store.CreateCVal(nameof store.Name, "")
    member store.Headline = store.CreateCVal(nameof store.Headline, "")
    member store.Email = store.CreateCVal(nameof store.Email, "")
    member store.Phone = store.CreateCVal(nameof store.Phone, "")
    member store.Location = store.CreateCVal(nameof store.Location, "")
    member store.Summary = store.CreateCVal(nameof store.Summary, "")

    member store.Picture = store.CreateCVal(nameof store.Picture, defaultPicture)

    member store.Links = store.CreateCVal(nameof store.Links, list<string>.Empty)

    member store.Experiences =
        store.CreateCVal(nameof store.Experiences, list<Experience>.Empty)

    member store.Languages = store.CreateCVal(nameof store.Languages, list<Language>.Empty)

    member store.Educations =
        store.CreateCVal(nameof store.Educations, list<Education>.Empty)

    member store.Certifications =
        store.CreateCVal(nameof store.Certifications, list<Certification>.Empty)

    member store.Skills = store.CreateCVal(nameof store.Skills, list<Skill>.Empty)

type Platform =
    | Windows
    | Linux
    | MacOS
    | Unknown

type IPlatformService =
    abstract member GetPlatform: unit -> Platform

type PlatformService() =
    interface IPlatformService with
        member _.GetPlatform() =
            if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
                Windows
            elif RuntimeInformation.IsOSPlatform OSPlatform.Linux then
                Linux
            elif RuntimeInformation.IsOSPlatform OSPlatform.OSX then
                MacOS
            else
                Unknown

type IProcessService =
    abstract member Run: command: string * arguments: string -> unit

type ProcessService(logger: ILogger<IProcessService>) =
    interface IProcessService with
        member _.Run(command, arguments) =
            try
                let psi = new ProcessStartInfo(command)
                psi.RedirectStandardOutput <- false
                psi.UseShellExecute <- true
                psi.CreateNoWindow <- true
                psi.Arguments <- arguments

                use p = new Process()
                p.StartInfo <- psi
                p.Start() |> ignore
                p.Dispose()
            with ex ->
                logger.LogError(ex, $"Error running process: {command} {arguments}")

type ILinkOpeningService =
    abstract member OpenUrl: url: string -> unit

type LinkOpeningService
    (platformService: IPlatformService, processService: IProcessService, logger: ILogger<ILinkOpeningService>) =
    interface ILinkOpeningService with
        member _.OpenUrl url =
            try
                match platformService.GetPlatform() with
                | Windows -> processService.Run("cmd", $"/c start \"\" \"{url}\"")
                | Linux -> processService.Run("xdg-open", url)
                | MacOS -> processService.Run("open", url)
                | _ -> ()
            with ex ->
                Debug.WriteLine ex
                logger.LogError(ex, "Error while opening next url = {url}")

[<Serializable>]
type Resume =
    { Name: string
      Picture: string
      Headline: string
      Email: string
      Phone: string
      Location: string
      Summary: string
      Links: string list
      Educations: Education list
      Certifications: Certification list
      Skills: Skill list
      Experiences: Experience list
      Languages: Language list }

type ResumeSerializer =
    static member FromXml(xmlContent: string) : Resume =
        let xDoc = XDocument.Parse xmlContent

        let get (el: XElement, name: string) =
            let e = el.Element(XName.Get name)
            if isNull e then "" else e.Value

        let picture =
            let pic = get (xDoc.Root, "Picture")

            if String.IsNullOrEmpty pic then
                $"{Photino.Blazor.PhotinoWebViewManager.AppBaseUri}images/man-person-icon.svg"
            else
                pic

        let links =
            xDoc.Root.Elements(XName.Get "Links")
            |> Seq.tryHead
            |> Option.map (fun el -> el.Elements(XName.Get "Link") |> Seq.map (fun l -> l.Value) |> Seq.toList)
            |> Option.defaultValue []

        let educations =
            xDoc.Root.Elements(XName.Get "Educations")
            |> Seq.tryHead
            |> Option.map (fun el ->
                el.Elements(XName.Get "Education")
                |> Seq.map (fun e ->
                    Education(
                        get (e, "School"),
                        get (e, "Degree"),
                        get (e, "Area"),
                        get (e, "Grade"),
                        get (e, "Location"),
                        get (e, "Period"),
                        get (e, "Website") |> Uri
                    ))
                |> Seq.toList)
            |> Option.defaultValue []

        let certifications =
            xDoc.Root.Elements(XName.Get "Certifications")
            |> Seq.tryHead
            |> Option.map (fun el ->
                el.Elements(XName.Get "Certification")
                |> Seq.map (fun c ->
                    Certification(
                        get (c, "Title"),
                        get (c, "Issuer"),
                        get (c, "Date"),
                        get (c, "Label"),
                        get (c, "Website") |> Uri
                    ))
                |> Seq.toList)
            |> Option.defaultValue []

        let skills =
            xDoc.Root.Elements(XName.Get "Skills")
            |> Seq.tryHead
            |> Option.map (fun el ->
                el.Elements(XName.Get "Skill")
                |> Seq.map (fun s ->
                    Skill(
                        get (s, "Name"),
                        s.Elements(XName.Get "Keywords")
                        |> Seq.tryHead
                        |> Option.map (fun k ->
                            k.Elements(XName.Get "Keyword") |> Seq.map (fun kw -> kw.Value) |> Seq.toList)
                        |> Option.defaultValue []
                    ))
                |> Seq.toList)
            |> Option.defaultValue []

        let experiences =
            xDoc.Root.Elements(XName.Get "Experiences")
            |> Seq.tryHead
            |> Option.map (fun el ->
                el.Elements(XName.Get "Experience")
                |> Seq.map (fun e ->
                    Experience(
                        get (e, "Company"),
                        get (e, "Website") |> Uri,
                        get (e, "Position"),
                        get (e, "Location"),
                        get (e, "Period"),
                        get (e, "Description")
                    ))
                |> Seq.toList)
            |> Option.defaultValue []

        let languages =
            xDoc.Root.Elements(XName.Get "Languages")
            |> Seq.tryHead
            |> Option.map (fun el ->
                el.Elements(XName.Get "Language")
                |> Seq.map (fun l -> Language(get (l, "Name"), get (l, "Fluency"), get (l, "Level") |> int))
                |> Seq.toList)
            |> Option.defaultValue []

        { Name = get (xDoc.Root, "Name")
          Picture = picture
          Headline = get (xDoc.Root, "Headline")
          Email = get (xDoc.Root, "Email")
          Phone = get (xDoc.Root, "Phone")
          Location = get (xDoc.Root, "Location")
          Summary = get (xDoc.Root, "Summary")
          Links = links
          Educations = educations
          Certifications = certifications
          Skills = skills
          Experiences = experiences
          Languages = languages }


    static member ToXml(resume: Resume) : string =
        let xDoc =
            XDocument(
                XElement(
                    XName.Get "Resume",
                    XElement(XName.Get "Name", resume.Name),
                    XElement(XName.Get "Picture", resume.Picture),
                    XElement(XName.Get "Headline", resume.Headline),
                    XElement(XName.Get "Email", resume.Email),
                    XElement(XName.Get "Phone", resume.Phone),
                    XElement(XName.Get "Location", resume.Location),
                    XElement(XName.Get "Summary", resume.Summary),
                    XElement(XName.Get "Links", resume.Links |> List.map (fun link -> XElement(XName.Get "Link", link))),
                    XElement(
                        XName.Get "Educations",
                        resume.Educations
                        |> List.map (fun edu ->
                            XElement(
                                XName.Get "Education",
                                XElement(XName.Get "School", edu.School),
                                XElement(XName.Get "Degree", edu.Degree),
                                XElement(XName.Get "Area", edu.Area),
                                XElement(XName.Get "Grade", edu.Grade),
                                XElement(XName.Get "Location", edu.Location),
                                XElement(XName.Get "Period", edu.Period),
                                XElement(XName.Get "Website", edu.Website.ToString())
                            ))
                    ),
                    XElement(
                        XName.Get "Certifications",
                        resume.Certifications
                        |> List.map (fun cert ->
                            XElement(
                                XName.Get "Certification",
                                XElement(XName.Get "Title", cert.Title),
                                XElement(XName.Get "Issuer", cert.Issuer),
                                XElement(XName.Get "Date", cert.Date),
                                XElement(XName.Get "Label", cert.Label),
                                XElement(XName.Get "Website", cert.Website.ToString())
                            ))
                    ),
                    XElement(
                        XName.Get "Skills",
                        resume.Skills
                        |> List.map (fun skill ->
                            XElement(
                                XName.Get "Skill",
                                XElement(XName.Get "Name", skill.Name),
                                XElement(
                                    XName.Get "Keywords",
                                    skill.Keywords |> List.map (fun kw -> XElement(XName.Get "Keyword", kw))
                                )
                            ))
                    ),
                    XElement(
                        XName.Get "Experiences",
                        resume.Experiences
                        |> List.map (fun exp ->
                            XElement(
                                XName.Get "Experience",
                                XElement(XName.Get "Company", exp.Company),
                                XElement(XName.Get "Website", exp.Website.ToString()),
                                XElement(XName.Get "Position", exp.Position),
                                XElement(XName.Get "Location", exp.Location),
                                XElement(XName.Get "Period", exp.Period),
                                XElement(XName.Get "Description", exp.Description)
                            ))
                    ),
                    XElement(
                        XName.Get "Languages",
                        resume.Languages
                        |> List.map (fun lang ->
                            XElement(
                                XName.Get "Language",
                                XElement(XName.Get "Name", lang.Name),
                                XElement(XName.Get "Fluency", lang.Fluency),
                                XElement(XName.Get "Level", lang.Level.ToString())
                            ))
                    )
                )
            )

        xDoc.ToString()

type IRazorEngineService =
    abstract member RenderAsync: key: string * model: Resume -> Task<string>
    abstract member GetTemplates: unit -> FileInfo array

type RazorEngineService(logger: ILogger<IRazorEngineService>) =
    let templatesPath = Path.Combine(AppSettings.WwwRootFolderPath, "templates")

    let engine =
        (new RazorLightEngineBuilder()).UseFileSystemProject(templatesPath).UseMemoryCachingProvider().Build()

    interface IRazorEngineService with
        member _.RenderAsync(key: string, model: Resume) : Task<string> =
            try
                engine.CompileRenderAsync(key, model)
            with ex ->
                logger.LogError(ex, $"Error rendering template: {key}")
                Task.FromResult ""

        member _.GetTemplates() : FileInfo array =
            let folder = templatesPath |> DirectoryInfo

            if folder.Exists then
                folder.GetFiles "*.cshtml"
            else
                Array.empty

type IGeneratePdfService =
    abstract member CreateAsync: key: string * model: Resume * outputPath: string -> Task<unit>

type GeneratePdfService(razorEngineService: IRazorEngineService, logger: ILogger<IGeneratePdfService>) =
    interface IGeneratePdfService with
        member _.CreateAsync(key: string, model: Resume, outputPath: string) : Task<unit> =
            task {
                try
                    let browserFetcher = new BrowserFetcher()
                    let! _installedBrowser = browserFetcher.DownloadAsync()
                    let! htmlContent = razorEngineService.RenderAsync(key, model)

                    let pdfOptions =
                        PdfOptions(
                            Format = PaperFormat.A4,
                            DisplayHeaderFooter = false,
                            PrintBackground = true,
                            MarginOptions = new MarginOptions(Top = "0mm", Bottom = "0mm", Left = "0mm", Right = "0mm")
                        )

                    use! browser =
                        Puppeteer.LaunchAsync(
                            LaunchOptions(Headless = true, Args = [| "--no-sandbox"; "--disable-setuid-sandbox" |])
                        )

                    use! page = browser.NewPageAsync()
                    do! page.SetContentAsync htmlContent
                    do! page.PdfAsync(outputPath, pdfOptions)
                with ex ->
                    logger.LogError(ex, $"Error generating PDF: {outputPath}")
            }

type IOpenDialogService =
    abstract member OpenFileAsync:
        ?title: string * ?defaultPath: string * ?multiSelect: bool * ?filters: struct (string * string array) array ->
            Task<string array>

    abstract member SaveFileAsync:
        ?title: string * ?defaultName: string * ?filters: struct (string * string array) array -> Task<string>

    abstract member OpenFolderAsync: ?title: string * ?defaultPath: string * ?multiSelect: bool -> Task<string array>

type OpenDialogService(app: Photino.Blazor.PhotinoBlazorApp) =
    interface IOpenDialogService with
        member this.OpenFileAsync(title, defaultPath, multiSelect, filters) =
            app.MainWindow.ShowOpenFileAsync(
                Option.toObj title,
                Option.toObj defaultPath,
                Option.defaultValue false multiSelect,
                Option.toObj filters
            )

        member this.SaveFileAsync(title, defaultName, filters) =
            app.MainWindow.ShowSaveFileAsync(Option.toObj title, Option.toObj defaultName, Option.toObj filters)

        member this.OpenFolderAsync(title, defaultPath, multiSelect) =
            app.MainWindow.ShowOpenFolderAsync(
                Option.toObj title,
                Option.toObj defaultPath,
                Option.defaultValue false multiSelect
            )

type IServices =
    abstract member LinkOpeningService: ILinkOpeningService
    abstract member RazorEngineService: IRazorEngineService
    abstract member GeneratePdfService: IGeneratePdfService
    abstract member OpenDialogService: IOpenDialogService
    abstract member Localizer: IStringLocalizer<SharedResources>
    abstract member Options: IOptions<AppSettings>
    abstract member Store: IShareStore

type Services
    (
        linkOpeningService: ILinkOpeningService,
        razorEngineService: IRazorEngineService,
        generatePdfService: IGeneratePdfService,
        openDialogService: IOpenDialogService,
        localizer: IStringLocalizer<SharedResources>,
        options: IOptions<AppSettings>,
        store: IShareStore
    ) =
    interface IServices with
        member _.LinkOpeningService = linkOpeningService
        member _.RazorEngineService = razorEngineService
        member _.GeneratePdfService = generatePdfService
        member _.OpenDialogService = openDialogService
        member _.Localizer = localizer
        member _.Options = options
        member _.Store = store
