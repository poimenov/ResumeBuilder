[<AutoOpen>]
module ResumeBuilder.App

open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Routing
open Microsoft.Extensions.Localization
open Microsoft.Extensions.Options
open MudBlazor
open Fun.Blazor
open Fun.Blazor.Router
open Microsoft.JSInterop

let appHeader =
    html.inject (fun (services: IServices) ->
        MudAppBar'' {
            Elevation 1

            MudIconButton'' {
                Icon Icons.Material.Filled.Menu
                Color Color.Inherit
                Edge Edge.Start
                onclick (fun _ -> services.Store.DrawerOpen.Publish not)
            }

            MudText'' {
                Typo Typo.h5
                class' "ml-3"
                AppSettings.ApplicationName
            }

            MudSpacer''

            adapt {
                let! isDarkMode = services.Store.IsDarkMode

                let darkLightModeButtonIcon =
                    if isDarkMode then
                        Icons.Material.Rounded.LightMode
                    else
                        Icons.Material.Outlined.DarkMode

                MudIconButton'' {
                    Icon darkLightModeButtonIcon
                    Color Color.Inherit

                    onclick (fun _ ->
                        let newMode = not isDarkMode
                        services.Store.IsDarkMode.Publish newMode
                        services.Options.Value.IsDarkMode <- newMode
                        lock services.Options.Value (fun () -> services.Options.Value.Save()))
                }

                appMenu services
            }
        })

let navmenus =
    html.injectWithNoKey (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        adapt {
            let! drawerOpen = store.DrawerOpen.WithSetter()

            MudDrawer'' {
                Open' drawerOpen
                Width "160px"
                Elevation 2
                ClipMode DrawerClipMode.Always

                MudNavMenu'' {

                    MudNavLink'' {
                        Href "/"
                        Match NavLinkMatch.All
                        Icon Icons.Material.Filled.Person
                        localizer["BasicInfo"]
                    }

                    MudNavLink'' {
                        Href "/links"
                        Match NavLinkMatch.All
                        Icon Icons.Material.Filled.Link
                        localizer["Links"]
                    }

                    MudNavLink'' {
                        Href "/summary"
                        Match NavLinkMatch.All
                        Icon Icons.Material.Filled.Description
                        localizer["Summary"]
                    }

                    MudNavLink'' {
                        Href "/experience"
                        Match NavLinkMatch.All
                        Icon Icons.Material.Filled.Work
                        localizer["Experiences"]
                    }

                    MudNavLink'' {
                        Href "/education"
                        Match NavLinkMatch.All
                        Icon Icons.Material.Filled.School
                        localizer["Educations"]
                    }

                    MudNavLink'' {
                        Href "/certification"
                        Match NavLinkMatch.All
                        Icon Icons.Material.Filled.CardMembership
                        localizer["Certifications"]
                    }
                }
            }

        })

let routes =
    html.route
        [| routeCi "/links" linksPage
           routeCi "/summary" summaryPage
           routeCi "/experience" experiencePage
           routeCi "/education" educationPage
           routeCi "/certification" certificationPage
           routeAny basicInfoPage |]

let mudTheme =
    let theme = new MudTheme()
    theme.LayoutProperties <- new LayoutProperties()

    let paletteDark =
        let p = new PaletteDark()
        p.Primary <- "#7e6fff"
        p.Surface <- "#1e1e2d"
        p.Background <- "#1a1a27"
        p.BackgroundGray <- "#151521"
        p.AppbarText <- "#92929f"
        p.AppbarBackground <- "rgba(26,26,39,0.8)"
        p.DrawerBackground <- "#1a1a27"
        p.ActionDefault <- "#74718e"
        p.ActionDisabled <- "#9999994d"
        p.ActionDisabledBackground <- "#605f6d4d"
        p.TextPrimary <- "#b2b0bf"
        p.TextSecondary <- "#92929f"
        p.TextDisabled <- "#ffffff33"
        p.DrawerIcon <- "#92929f"
        p.DrawerText <- "#92929f"
        p.GrayLight <- "#2a2833"
        p.GrayLighter <- "#1e1e2d"
        p.Info <- "#4a86ff"
        p.Success <- "#3dcb6c"
        p.Warning <- "#ffb545"
        p.Error <- "#ff3f5f"
        p.LinesDefault <- "#33323e"
        p.TableLines <- "#33323e"
        p.Divider <- "#292838"
        p.OverlayLight <- "#1e1e2d80"
        p

    let paletteLight =
        let p = new PaletteLight()
        p.Black <- "#110e2d"
        p.AppbarText <- "#424242"
        p.AppbarBackground <- "rgba(255,255,255,0.8)"
        p.DrawerBackground <- "#ffffff"
        p.GrayLight <- "#e8e8e8"
        p.GrayLighter <- "#f9f9f9"
        p

    theme.PaletteDark <- paletteDark
    theme.PaletteLight <- paletteLight
    theme

let app =
    html.inject (fun (hook: IComponentHook, store: IShareStore, js: IJSRuntime, settings: IOptions<AppSettings>) ->
        hook.AddFirstAfterRenderTask(fun _ ->
            task {
                store.IsDarkMode.Publish settings.Value.IsDarkMode
                js.InvokeVoidAsync("split").AsTask() |> Async.AwaitTask |> ignore
            })

        ErrorBoundary'' {
            ErrorContent(fun e ->
                MudAlert'' {
                    Severity Severity.Error
                    string e
                })

            adapt {
                let! isDarkMode = store.IsDarkMode

                MudThemeProvider'' {
                    Theme mudTheme
                    IsDarkMode isDarkMode
                }
            }

            MudSnackbarProvider''
            MudPopoverProvider''
            MudDialogProvider''

            MudLayout'' {
                appHeader
                navmenus

                MudMainContent'' {
                    MudStack'' {
                        Row true
                        Spacing 0
                        class' "main-stack "

                        div {
                            id "left"

                            MudText'' {
                                Typo Typo.h6
                                class' "pa-4"

                                SectionOutlet'' { SectionName "Title" }
                            }

                            MudContainer'' {
                                class' "pa-4"
                                routes
                            }
                        }

                        div {
                            id "right"
                            iframe { id "targetWindow" }
                        }

                        previewPage
                    }

                }
            }
        })

type App() =
    inherit FunComponent()

    override _.Render() = app
