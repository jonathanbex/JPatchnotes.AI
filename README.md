# JPatchnotes.AI

A funky patchnotes generator that analyzes release code-diffs and pull requests to generate whacky patch notes using ChatGPT with a dash of humor!

## Features:
- Analyzes **GitHub pull requests** and **code diffs** to generate release notes.
- Output is **markdown-formatted** with sections like **Features**, **Fixes**, **Improvements**, and **Internal Changes**.
- **Fun** tone for patch notes, with a **little humor** (but can be toggled for more serious output).

---

## Example

Here’s what an output might look like:

```
# DiscordBot Release Notes - Version `master` 🚀
*Changes since `20240818-e5acf1a9`*

Hello, Discord enthusiasts and fellow developers! Here's the latest scoop on what's brewing in the DiscordBot project. Grab your favorite beverage and let's dive in! ☕

---

## Features 🎉
- **SQLite Integration**: We've leveled up our data storage game by integrating **SQLite**! Now you can manage your guild lineups and server commands more efficiently with our new `SqliteGuildLineup` and `SqliteServerCommand` models. 📚✨

## Improvements 🔧
- **Enhanced Configuration Validation**: Improved the `ConfigurationValidator` to handle SQLite usage settings seamlessly. No more configuration headaches! 🛠️
- **Dependency Injection Refinement**: Introduced the `RepositoryResolver` to streamline dependency injection, making our codebase cleaner and more modular. 🧩

## Fixes 🐞
- **JSON Parsing Enhancement**: Fixed the JSON parsing in `JsonConfigurationHelper` by removing pesky comments that were causing hiccups. Smooth sailing ahead! 🚢🔍

## Internal 👨‍💻
- **Project Structure Overhaul**: Updated the solution and project files to incorporate new SQLite repositories and context models. Our internal architecture is now more robust and scalable. 🏗️
- **Mapper Enhancements**: Expanded `DBMapper` to support the new SQLite models, ensuring data consistency across the board. 🔄

## Other 📚
- **README Revamp**: Spruced up the README with clearer setup instructions and additional resources. Getting started with DiscordBot is now easier than ever! 📖✨

*Nothing major changed? Well, just a little tweak here and there to keep things shiny! 😉*

---

### Authors:
#### jonathanbex
- **Files Changed**: 14
- **Additions**: 5202
- **Deletions**: 670

Meet Jonathan! When he's not typing away like a caffeinated squirrel, he's meticulously crafting code to make DiscordBot awesome. 🐿️☕

---

Stay tuned for more updates, and happy coding! If you have any questions or feedback, feel free to reach out. Let's make DiscordBot the best it can be together! 🤝💬
```

## Setup Instructions

To get **JPatchnotes.AI** up and running, follow the steps below:

### 1. **Create GitHub Personal Access Token**

To access pull requests and code diffs, you’ll need to generate a **GitHub Personal Access Token** with the `repo` scope.

- Go to: [GitHub Tokens Page](https://github.com/settings/tokens)
- Click **Generate new token**.
- Choose the **`repo`** scope to allow read access to your repositories.
- Copy the generated token.

### 2. **Obtain OpenAI API Key and Deployment Details**

For generating patch notes via **OpenAI**, you'll need:

- **OpenAI Endpoint URL**
  - Available in the Azure OpenAI resource portal.
  - Example: `https://<your-openai-resource>.openai.azure.com/`

- **Deployment Name (Model)**:  
  - This is the name of the model deployment you created (e.g., `gpt-4o` or `o1-mini`).
  
- **OpenAI API Key**  
  - Get this from the **Keys and Endpoint** section in the Azure portal.

### 3. **Create a Configuration File**

In your project, create a configuration file (`config.json`) to store these settings:

```json
{
  "Github": {
    "Token": "<your-github-token>"
  },
  "OpenAI": {
    "Endpoint": "<your-openai-endpoint>",
    "DeploymentName": "<your-deployment-name>",
    "APIKey": "<your-openai-api-key>"
  }
}
```

Make sure to replace the placeholders (<your-github-token>, <your-openai-endpoint>, etc.) with your actual values.

## Running JPatchnotes.AI

Once your configuration is set up, simply run the program. It will:

- Fetch the latest release from GitHub.
- Compare commits and pull requests.
- Generate patch notes, with a little bit of fun!

### 🧠 Key Points:
- The **configuration** is stored in a `config.json` file with GitHub and OpenAI details.
- **Example output** shows what the generated patch notes will look like.
- Detailed instructions on **how to set up GitHub token** and **Azure OpenAI keys**.

Let me know if you need any other sections or adjustments!
