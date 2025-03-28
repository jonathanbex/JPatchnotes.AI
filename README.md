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
[Total Patch Notes]
# Release Notes for Version master

*Changes since commit 81670b1931f71fc7ecd1db4b47217cc208e3ffe1.*

## Features

### User Authentication
- Account Management: Introduced AccountController to handle user authentication processes, including login and logout functionalities.
- Login Interface: Implemented a user-friendly login page (Views/Account/Login.cshtml) using LoginViewModel to enhance the user experience.
- Authorization Enhancements: Secured key controllers (AdminController, FileController, and updated HomeController) with the [Authorize] attribute to ensure only authenticated users can access certain functionalities.

### Admin Dashboard
- User Management: Added an AdminController allowing administrators to manage user accounts. Features include creating, editing, and deleting users.
- Drive Management: Enabled administrators to view, add, and remove drives dynamically via the new admin interface (Views/Admin/Drives.cshtml).

### File Management
- File Operations: Introduced FileController to facilitate file uploads, downloads, and deletions, providing a robust file management system for users.
- File Results Handling: Added FileResultModel and GlobalLinkFileResult to efficiently handle and display file data.

### Global Link Sharing
- Link Generation: Developed IGlobalLinkService and its implementation GlobalLinkService to generate secure global links for file sharing.
- Link Tracking: Implemented mechanisms to track link usage and expiration through the GlobalLink model, ensuring secure and controlled access to shared files.

### User Services and Repositories
- User Service: Added IUserService and UserService to handle user-related operations such as registration, authentication, and profile management.
- Repository Pattern: Established repository interfaces and implementations for User and GlobalLink (IUserRepository, UserRepository, IGlobalLinkRepository, GlobalLinkRepository) to promote a clean separation of concerns and enhance maintainability.

## Improvements

- Project Structure Refinement: Refactored existing controllers to inherit from a new BaseController, promoting code reusability and consistency across the application.
- Configuration Management: Introduced JsonConfigurationHelper to streamline the handling and updating of application settings.
- Dependency Injection Enhancements: Expanded service registrations in Program.cs to include new services and repositories, improving scalability and maintainability.
- UI Enhancements: Upgraded various views (Views/Home/Index.cshtml, Views/File/Index.cshtml, etc.) to offer a more intuitive and responsive user interface.

## Fixes

*No bug fixes in this release. Stay tuned for some magic in the next update! ??*

## Internal

- CI/CD Pipeline: Added a new GitHub Actions workflow (.github/workflows/release.yml) to automate the build and release process for the master branch, ensuring smoother deployments.
- Security Enhancements: Implemented CryptoUtility for secure hashing operations, bolstering the application's security measures.
- Package Updates: Updated JFiler.csproj to include essential dependencies like Newtonsoft.Json and sqlite-net-pcl to support new features and improve performance.

## Other

- Documentation: Added a comprehensive README.md outlining the JFiler application's purpose, features, and setup instructions to help users get started quickly.

---

Happy coding! ?? If you encounter any issues or have suggestions, feel free to reach out. Until next time, keep those pull requests coming!
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
