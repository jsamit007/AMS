# GitHub Actions Workflows

This directory contains automated CI/CD workflows for the AMS (Attendance Management System) project.

## Available Workflows

### 1. **CI/CD Pipeline** (`ci.yml`)
Main continuous integration workflow that runs on every push and pull request.

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

**Jobs:**
- **build**: Restores dependencies, builds the solution, and runs unit tests
- **code-quality**: Performs static code analysis using StyleCop
- **security-scan**: Runs Trivy vulnerability scanner on the codebase
- **build-artifacts**: Creates release builds and publishes API artifacts (main branch only)

### 2. **Code Analysis** (`code-analysis.yml`)
Static code analysis using SonarCloud for code quality metrics.

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

**Jobs:**
- **sonarcloud**: Analyzes code using SonarCloud

**Setup Required:**
- Configure `SONARCLOUD_TOKEN` secret in repository settings
- Set your SonarCloud project key in the workflow

### 3. **Release** (`release.yml`)
Automated release creation when you push version tags.

**Triggers:**
- Push of tags matching pattern `v*` (e.g., `v1.0.0`, `v2.1.3`)

**Jobs:**
- **create-release**: Builds release artifacts and creates GitHub release with downloadable archives

## Getting Started

### Prerequisites
- GitHub repository with Actions enabled
- .NET 9.0 SDK (for local testing)

### Configuration

#### 1. GitHub Secrets
Set up the following secrets in your repository settings (`Settings > Secrets and variables > Actions`):

For SonarCloud:
- `SONARCLOUD_TOKEN`: Your SonarCloud authentication token

#### 2. sonar-project.properties
Create a `sonar-project.properties` file in the repository root for SonarCloud analysis:

```properties
sonar.projectKey=AMS
sonar.organization=your-organization-key
sonar.sources=AMS.API,AMS.Authentication,AMS.Command,AMS.Query,AMS.Repository,AMS.Contracts
sonar.exclusions=**/bin/**,**/obj/**,**/*.Tests/**
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml
```

### Usage

#### Running Tests
To ensure code quality, tests run automatically on every push/PR:
```bash
dotnet test --configuration Release
```

#### Creating a Release
1. Tag a commit with a version:
```bash
git tag v1.0.0
git push origin v1.0.0
```

2. The Release workflow will automatically:
   - Build the project
   - Run all tests
   - Create a GitHub release with artifacts

#### Manual Workflow Runs
You can manually trigger workflows from the GitHub Actions tab:
1. Go to your repository
2. Click **Actions**
3. Select a workflow
4. Click **Run workflow**

## Workflow Status

View the status of all workflows:
- Visit the **Actions** tab in your GitHub repository
- Each workflow shows pass/fail status
- Click on a workflow run for detailed logs

## Best Practices

1. **Commit Messages**: Use clear, descriptive commit messages
2. **Pull Requests**: All code should go through PR reviews before merging to main
3. **Tags**: Follow semantic versioning for release tags (v1.2.3)
4. **Secrets**: Never commit secrets; always use GitHub Secrets
5. **Branch Protection**: Consider enabling branch protection rules that require status checks to pass

## Troubleshooting

### Workflow Failures
1. Check the detailed logs in the **Actions** tab
2. Look for specific error messages
3. Common issues:
   - Missing .NET SDK version
   - Failed tests
   - NuGet package restore issues

### Code Analysis Skipped
- Ensure `SONARCLOUD_TOKEN` is configured
- Check that `sonar-project.properties` exists with correct settings

### Release Not Created
- Verify the tag follows the pattern `v*`
- Check that all tests pass before release creation

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [SonarCloud Documentation](https://docs.sonarcloud.io)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
