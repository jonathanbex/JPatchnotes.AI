# JPatchnotes.AI

A funky patchnotes generator that analyzes release code-diffs and pull requests to generate whacky patch notes with a dash of humor!

## Features:
- Analyzes **GitHub pull requests** and **code diffs** to generate release notes.
- Output is **markdown-formatted** with sections like **Features**, **Fixes**, **Improvements**, and **Internal Changes**.
- **Fun** tone for patch notes, with a **little humor** (but can be toggled for more serious output).

---

## Example

Here’s what an output might look like:

```
[Total Patch Notes]
# Release Notes - master

*Changes since `20241130-f35d82d5`*

## Features
- **Introducing `BotApplication` Solution**
  We've added a brand new `BotApplication.sln` to better organize our bot projects. Say hello to a more streamlined development experience!

## Improvements
- **Updated CI/CD Pipeline**
  Our GitHub Actions workflow (`.github/workflows/dotnet-desktop.yml`) now restores and builds the `BotApplication` project instead of the old `DiscordBot`. Faster, cleaner, better.

## Fixes
- **Goodbye, DiscordBot**
  Removed outdated `DiscordBot` components including controllers, middleware, and project files. It's not you, it's us. We're moving forward with `BotApplication`!

## Internal
- **Repository Cleanup**
  Tidied up the codebase by deleting deprecated `DiscordBot` solution files, middleware, and launch settings. Less clutter, more clarity.

---

Keep coding, keep smiling! ??
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
