[<AutoOpen>]
module ResumeBuilder.Preview

open Fun.Blazor
open System.IO
open Microsoft.JSInterop

let setTargetOutputAsync (js: IJSRuntime, templateName: string, resume: Resume, engine: IRazorEngineService) =
    task {
        let! output = engine.RenderAsync(templateName, resume)
        let outputStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes output)
        let strRef = new DotNetStreamReference(outputStream, false)

        js.InvokeVoidAsync("setSource", "targetWindow", strRef).AsTask()
        |> Async.AwaitTask
        |> ignore
    }

let previewPage =
    html.inject (fun (js: IJSRuntime, store: IShareStore, engine: IRazorEngineService) ->
        fragment {
            adapt {
                let! name = store.Name
                let! headline = store.Headline
                let! picture = store.Picture
                let! email = store.Email
                let! phone = store.Phone
                let! location = store.Location
                let! summary = store.Summary
                let! links = store.Links
                let! educations = store.Educations
                let! certifications = store.Certifications
                let! skills = store.Skills
                let! experiences = store.Experiences
                let! languages = store.Languages
                let! template = store.Template

                let resume =
                    { Name = name
                      Picture = picture
                      Headline = headline
                      Email = email
                      Phone = phone
                      Location = location
                      Summary = summary
                      Links = links
                      Educations = educations
                      Certifications = certifications
                      Skills = skills
                      Experiences = experiences
                      Languages = languages }

                setTargetOutputAsync(js, template, resume, engine).Result |> ignore
            }
        })
