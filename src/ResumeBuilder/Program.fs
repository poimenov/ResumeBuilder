module Program

open System
open System.Globalization
open System.IO
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open MudBlazor.Services
open MudBlazor.Translations
open log4net.Config
open Photino.Blazor
open ResumeBuilder


[<STAThread>]
[<EntryPoint>]
let main args =
    let DATA_DIRECTORY = "DATA_DIRECTORY"
    let builder = PhotinoBlazorAppBuilder.CreateDefault args

    builder.RootComponents.Add<App> "app"

    let configuration =
        ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppSettings.AppConfigFileName),
                true,
                false
            )
            .Build()

    builder.Services.AddFunBlazorWasm() |> ignore
    builder.Services.AddMudServices() |> ignore
    builder.Services.AddMudTranslations() |> ignore

    builder.Services.AddLogging(fun logging -> logging.ClearProviders().AddLog4Net() |> ignore)
    |> ignore

    builder.Services.AddLocalization(fun options -> options.ResourcesPath <- "Resources")
    |> ignore

    builder.Services.AddSingleton<IConfiguration> configuration |> ignore
    builder.Services.Configure<AppSettings> configuration |> ignore
    builder.Services.AddSingleton<IPlatformService, PlatformService>() |> ignore
    builder.Services.AddSingleton<IProcessService, ProcessService>() |> ignore
    builder.Services.AddTransient<IOpenDialogService, OpenDialogService>() |> ignore

    builder.Services.AddSingleton<ILinkOpeningService, LinkOpeningService>()
    |> ignore

    builder.Services.AddSingleton<IRazorEngineService, RazorEngineService>()
    |> ignore

    builder.Services.AddSingleton<IGeneratePdfService, GeneratePdfService>()
    |> ignore

    let application = builder.Build()
    AppDomain.CurrentDomain.SetData("DataDirectory", AppSettings.AppDataPath)
    Environment.SetEnvironmentVariable(DATA_DIRECTORY, AppSettings.AppDataPath)
    let logger = application.Services.GetRequiredService<ILogger<_>>()
    logger.LogInformation "Starting application"

    match FileInfo AppSettings.LogConfigPath with
    | file when file.Exists -> XmlConfigurator.Configure file |> ignore
    | _ -> logger.LogWarning "Config file not found"

    let settings = application.Services.GetRequiredService<IOptions<AppSettings>>()
    CultureInfo.DefaultThreadCurrentCulture <- CultureInfo.GetCultureInfo settings.Value.CultureName
    CultureInfo.DefaultThreadCurrentUICulture <- CultureInfo.GetCultureInfo settings.Value.CultureName

    // customize window
    application.MainWindow
        .RegisterSizeChangedHandler(
            EventHandler<Drawing.Size>(fun _ args ->
                settings.Value.WindowWidth <- args.Width
                settings.Value.WindowHeight <- args.Height
                lock settings.Value (fun () -> settings.Value.Save()))
        )
        .SetSize(settings.Value.WindowWidth, settings.Value.WindowHeight)
        .SetIconFile(Path.Combine(AppSettings.WwwRootFolderName, "images", AppSettings.FavIconFileName))
        .SetTitle
        AppSettings.ApplicationName
    |> ignore

    AppDomain.CurrentDomain.UnhandledException.Add(fun e ->
        match e.ExceptionObject with
        | :? Exception as ex ->
            application.Services.GetRequiredService<ILogger<_>>().LogError(ex, ex.Message)
            application.MainWindow.ShowMessage(ex.Message, "Error") |> ignore
        | _ ->
            application.Services
                .GetRequiredService<ILogger<_>>()
                .LogError("Non-exception thrown: {0}", e.ExceptionObject))

    application.Run()
    0
