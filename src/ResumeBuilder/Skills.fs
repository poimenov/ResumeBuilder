[<AutoOpen>]
module ResumeBuilder.Skills

open System
open Microsoft.Extensions.Localization
open Fun.Blazor
open FSharp.Data.Adaptive
open MudBlazor

let skillsPage =
    html.inject (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        let selectedSkillIndex = cval -1
        let selectedSkill = cval None
        let skill = cval (Skill("", List.empty))
        let skillGroupName = cval ""
        let keywords = cval List.empty
        let keyword = cval ""


        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Skills"]
            }

            MudCard'' {
                Outlined true

                adapt {
                    let! skills = store.Skills
                    let! getSelectedSkillIndex, setSelectedSkillIndex = selectedSkillIndex.WithSetter()
                    let! getSelectedSkill, setSelectedSkill = selectedSkill.WithSetter()
                    let! getSkill, setSkill = skill.WithSetter()
                    let! getSkillGroupName, setSkillGroupName = skillGroupName.WithSetter()
                    let! getKeywords, setKeywords = keywords.WithSetter()
                    let! getKeyword, setKeyword = keyword.WithSetter()

                    MudCardContent'' {
                        MudList'' {
                            type' Skill
                            SelectedValue'(getSelectedSkill, setSelectedSkill)
                            class' "list"

                            SelectedValueChanged(fun item ->
                                match item with
                                | Some skl ->
                                    let indexOption = skills |> List.tryFindIndex (fun x -> x = skl)

                                    match indexOption with
                                    | Some index ->
                                        setSelectedSkillIndex index
                                        setSelectedSkill item
                                        setSkill skl
                                        setSkillGroupName skl.Name
                                        setKeywords skl.Keywords
                                    | None -> ()
                                | None -> ())

                            skills
                            |> List.map (fun x ->
                                MudListItem'' {
                                    class' "list-item"
                                    Value(Some x)
                                    title' x.Name
                                    Text x.Name
                                })
                        }

                        MudTextField''<string> {
                            label' (string (localizer["SkillGroupName"]))
                            Variant Variant.Text
                            Value'(getSkillGroupName, setSkillGroupName)
                        }

                        MudInputLabel'' {
                            style' "margin-top: 16px;"
                            localizer["Keywords"]
                        }

                        MudChipSet''<string> {
                            style' "max-height: 120px;overflow-y: auto;"
                            AllClosable true

                            getKeywords
                            |> List.map (fun x ->
                                MudChip''<string> {
                                    Variant Variant.Outlined

                                    OnClose(fun (chip: MudChip<string>) ->
                                        setKeywords (getKeywords |> List.filter (fun x -> x <> chip.Text)))

                                    Text x
                                })
                        }

                        MudStack'' {
                            Row true
                            Justify Justify.FlexEnd
                            style' "margin-top: 16px;"

                            MudTextField''<string> {
                                label' (string (localizer["Keyword"]))
                                Variant Variant.Text
                                Immediate true
                                Value'(getKeyword, setKeyword)
                            }

                            MudFab'' {
                                Color Color.Default
                                Size Size.Small
                                StartIcon Icons.Material.Filled.Add
                                Disabled(String.IsNullOrWhiteSpace getKeyword)
                                title' (string (localizer["Add"]))

                                OnClick(fun _ ->
                                    let exists =
                                        getKeywords
                                        |> List.exists (fun x ->
                                            x.Equals(getKeyword, StringComparison.InvariantCultureIgnoreCase))

                                    if not exists then
                                        setKeywords (getKeywords @ [ getKeyword ])
                                        setKeyword "")
                            }
                        }
                    }

                    MudCardActions'' {
                        class' "card-actions"

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small

                            Disabled(String.IsNullOrWhiteSpace getSkillGroupName || getKeywords.Length < 1)

                            OnClick(fun _ ->
                                let newSkill = Skill(getSkillGroupName, getKeywords)

                                let exists =
                                    skills
                                    |> List.exists (fun x ->
                                        x.Name.Equals(getSkillGroupName, StringComparison.InvariantCultureIgnoreCase))

                                if not exists then
                                    store.Skills.Publish(skills @ [ newSkill ])
                                    setSelectedSkill None
                                    setSelectedSkillIndex -1
                                    setSkillGroupName ""
                                    setKeywords List.empty
                                    setKeyword "")

                            localizer["Add"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedSkillIndex < 0)

                            OnClick(fun _ ->
                                match getSelectedSkill with
                                | None -> ()
                                | Some skl ->
                                    store.Skills.Publish(skills |> List.filter (fun x -> x <> skl))
                                    setSelectedSkill None
                                    setSelectedSkillIndex -1
                                    setSkillGroupName ""
                                    setKeywords List.empty
                                    setKeyword "")

                            localizer["Delete"]
                        }

                        MudButton'' {
                            Variant Variant.Filled
                            Color Color.Default
                            Size Size.Small
                            Disabled(getSelectedSkillIndex < 0)

                            OnClick(fun _ ->
                                if skills.Length > 0 && getSelectedSkillIndex > -1 then
                                    let updatedSkill = Skill(getSkillGroupName, getKeywords)

                                    store.Skills.Publish(
                                        skills
                                        |> List.mapi (fun i x ->
                                            if i = getSelectedSkillIndex then updatedSkill else x)
                                    )

                                    setSelectedSkill None
                                    setSelectedSkillIndex -1
                                    setSkillGroupName ""
                                    setKeywords List.Empty
                                    setKeyword "")

                            localizer["Update"]
                        }
                    }
                }
            }
        }


    )
