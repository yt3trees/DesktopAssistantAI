# DesktopAssistantAI

![gif](./_image/DesktopAssistantAI.gif)

This is a desktop mascot that runs OpenAI or Azure OpenAI's Assistants API.
It can also perform basic operations such as creating and modifying Assistant, VectorStore, and File.

## System Requirements
- OS
  - Windows 10
  - Windows 11

## Feature
### Configuration
OpenAI or AzureOpenAI APIs are available.

### Avatar
The avatar to be displayed can be any png image.

### Keeping Threads
Threads are stored in a local file as history. As long as the thread is alive, past conversations can be referenced.

## How to create an installer
### Requirements
- Visual Studio 2022
### Procedure
1. Open `DesktopAssistantAI.sln` in Visual Studio.
2. Build the `Installer` project.
3. The `DesktopAssistantAISetup.msi` file will be output to the `.\Installer` folder.

## License
[MIT](https://github.com/yt3trees/DesktopAssistantAI/blob/main/LICENSE)
