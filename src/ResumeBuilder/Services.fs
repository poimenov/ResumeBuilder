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

type SharedResources() = class end
let emailRegex = Regex("^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)
let phoneRegex = Regex("^\+?[0-9\s\-()]+$", RegexOptions.Compiled)

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
    (school: string, degree: string, area: string, grade: string, location: string, period: string, website: string) =
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
    member store.Name = store.CreateCVal(nameof store.Name, "")
    member store.Headline = store.CreateCVal(nameof store.Headline, "")
    member store.Email = store.CreateCVal(nameof store.Email, "")
    member store.Phone = store.CreateCVal(nameof store.Phone, "")
    member store.Location = store.CreateCVal(nameof store.Location, "")
    member store.Summary = store.CreateCVal(nameof store.Summary, "")

    member store.Picture =
        store.CreateCVal(
            nameof store.Picture,
            $"{Photino.Blazor.PhotinoWebViewManager.AppBaseUri}images/man-person-icon.svg"
        )

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
    (platformService: IPlatformService, processService: IProcessService, logger: ILogger<LinkOpeningService>) =
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

type IRazorEngineService =
    abstract member RenderAsync: key: string * model: Resume -> Task<string>
    abstract member Render: key: string * model: Resume -> string
    abstract member GetTemplates: unit -> FileInfo array

type RazorEngineService() =
    let templatesPath = Path.Combine(AppSettings.WwwRootFolderPath, "templates")

    let engine =
        (new RazorLightEngineBuilder()).UseFileSystemProject(templatesPath).UseMemoryCachingProvider().Build()

    interface IRazorEngineService with
        member _.RenderAsync(key: string, model: Resume) : Task<string> = engine.CompileRenderAsync(key, model)

        member this.Render(key: string, model: Resume) : string =
            engine.CompileRenderAsync(key, model).Result

        member _.GetTemplates() : FileInfo array =
            let folder = templatesPath |> DirectoryInfo

            if folder.Exists then
                folder.GetFiles "*.cshtml"
            else
                Array.empty

type IGeneratePdfService =
    abstract member CreateAsync: key: string * model: Resume * outputPath: string -> Task<unit>

type GeneratePdfService(razorEngineService: IRazorEngineService) =
    interface IGeneratePdfService with
        member _.CreateAsync(key: string, model: Resume, outputPath: string) : Task<unit> =
            task {
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
            }

type IOpenDialogService =
    abstract member OpenFileAsync:
        ?title: string * ?defaultPath: string * ?multiSelect: bool * ?filters: struct (string * string array) array ->
            Task<string array>

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

        member this.OpenFolderAsync(title, defaultPath, multiSelect) =
            app.MainWindow.ShowOpenFolderAsync(
                Option.toObj title,
                Option.toObj defaultPath,
                Option.defaultValue false multiSelect
            )
