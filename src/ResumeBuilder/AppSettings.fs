[<AutoOpen>]
module ResumeBuilder.AppSettings

open System
open System.Globalization
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

type public AppSettings() =
    static member ApplicationName = "ResumeBuilder"
    static member FavIconFileName = "favicon.ico"
    static member LogConfigFileName = "log4net.config"
    static member AppConfigFileName = "appsettings.json"
    static member WwwRootFolderName = "wwwroot"

    static member AppDataPath =
        Path.Combine(
            Environment.GetFolderPath Environment.SpecialFolder.LocalApplicationData,
            AppSettings.ApplicationName
        )

    static member LogConfigPath =
        Path.Combine(AppContext.BaseDirectory, AppSettings.LogConfigFileName)

    static member WwwRootFolderPath =
        Path.Combine(AppContext.BaseDirectory, AppSettings.WwwRootFolderName)

    member val WindowWidth: int = 1024 with get, set
    member val WindowHeight: int = 768 with get, set
    member val CultureName: string = "en-US" with get, set

    [<JsonIgnore>]
    member val CurrentRegion = RegionInfo.CurrentRegion with get

    member this.Save() =
        let filePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppSettings.AppConfigFileName)

        let options = JsonSerializerOptions(WriteIndented = true)
        let jsonString = JsonSerializer.Serialize(this, options)
        File.WriteAllText(filePath, jsonString)
