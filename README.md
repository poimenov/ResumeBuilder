# ResumeBuilder

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![F#](https://img.shields.io/badge/F%23-10.0-lightblue.svg)](https://fsharp.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**ResumeBuilder** is a modern desktop application for creating professional resumes, written in F# using Photino.Blazor and MudBlazor. Create impressive resumes quickly and easily!

![Screenshot of the UI](/img/screen.jpg)

## ✨ Features

- **Intuitive Interface**: Simple and user-friendly design using MudBlazor components
- **Multilingual Support**: Localization in Russian, English
- **Ready-made Templates**: Several resume styles to choose from
- **Export**: Save resumes in XML, HTML or PDF format for further use
- **Customizable Templates**: You can add your own templates without having to rebuild the application.
- **Modern Technologies**: Built on .NET 10.0 with Blazor

## 🛠 Technologies

- **[F#](https://github.com/dotnet/fsharp)** - Main programming language
- **[.NET 10.0](https://github.com/dotnet/dotnet)** - Development platform
- **[Photino.Blazor](https://github.com/tryphotino/photino.Blazor)**: - A lightweight native window to host the Blazor UI
- **[Fun.Blazor](https://github.com/slaveOftime/Fun.Blazor)**: A famous F#-first DSL for building Blazor UI components
- **[MudBlazor](https://github.com/MudBlazor/MudBlazor)** - Material Design components for Blazor
- **[MudBlazor.HtmlEditor](https://github.com/erinnmclaughlin/MudBlazor.HtmlEditor)** - A customizable HTML editor component powered by [QuillJS](https://quilljs.com/)
- **[puppeteer-sharp](https://github.com/hardkoded/puppeteer-sharp)** - Headless Chrome .NET API
- **[RazorLight](https://github.com/toddams/RazorLight)** - Template engine based on Microsoft's Razor parsing engine for .NET Core
- **[log4net](https://github.com/apache/logging-log4net)**  Logging

## 📋 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or newer

## 🚀 Installation and Running

1. **Clone the repository:**
   ```bash
   git clone https://github.com/poimenov/ResumeBuilder.git
   cd ResumeBuilder
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run --project src/ResumeBuilder/ResumeBuilder.fsproj
   ```