## Introduction

This document outlines the Git workflow and branching model for the VAT Filing Pricing Tool project. It defines the branch types, naming conventions, workflow processes, and best practices for version control. Following these guidelines ensures consistent and efficient collaboration across the development team.

## Branching Model

The VAT Filing Pricing Tool project uses a modified GitFlow branching model that supports continuous integration and delivery while maintaining stable production code.

### Core Branches

The repository maintains two long-lived branches:

- **main**: The production branch containing the code currently deployed to production. All code in this branch must be production-ready.
- **develop**: The integration branch for features being developed for the next release. This branch serves as the base for feature branches and contains the latest development changes.

### Supporting Branches

In addition to the core branches, the following supporting branch types are used for specific purposes:

- **feature/**: Short-lived branches for developing new features or enhancements
- **bugfix/**: Short-lived branches for fixing bugs in the develop branch
- **hotfix/**: Short-lived branches for critical fixes to production code
- **release/**: Short-lived branches for preparing releases
- **docs/**: Short-lived branches for documentation updates only
- **refactor/**: Short-lived branches for code refactoring without changing functionality

### Branch Lifecycle

Supporting branches follow a specific lifecycle:

1. **Creation**: Branch is created from its parent branch (e.g., feature branches from develop)
2. **Development**: Changes are made and committed to the branch
3. **Testing**: Automated tests and code reviews ensure quality
4. **Integration**: Changes are merged back to the parent branch via pull request
5. **Cleanup**: Branch is deleted after successful integration

## Branch Naming Conventions

Consistent branch naming helps identify the purpose of branches and associate them with work items.

### Pattern

All supporting branches should follow this naming pattern:

```
<type>/<issue-id>-<short-description>
```

Where:
- `<type>` is one of: feature, bugfix, hotfix, release, docs, refactor
- `<issue-id>` is the ID of the associated work item or issue (optional for docs)
- `<short-description>` is a brief, hyphenated description of the change

### Examples

```
feature/123-country-selector-component
bugfix/456-fix-calculation-rounding-error
hotfix/789-critical-security-vulnerability
release/v1.2.0
docs/update-api-documentation
refactor/extract-calculation-service
```

### Restrictions

- Branch names should use lowercase letters, numbers, and hyphens only
- Avoid using special characters or spaces in branch names
- Keep branch names concise but descriptive (under 50 characters)
- Ensure the description clearly indicates the purpose of the branch

## Workflow Processes

This section describes the specific workflows for different types of development activities.

### Feature Development

The process for developing new features:

1. Create a feature branch from the latest `develop` branch:
   ```bash
   git checkout develop
   git pull
   git checkout -b feature/123-new-feature-name
   ```

2. Develop the feature, making regular commits:
   ```bash
   git add .
   git commit -m "feat: implement feature component"
   ```

3. Push the branch to the remote repository:
   ```bash
   git push -u origin feature/123-new-feature-name
   ```

4. Keep the feature branch updated with changes from develop:
   ```bash
   git checkout develop
   git pull
   git checkout feature/123-new-feature-name
   git merge develop
   ```

5. When the feature is complete, create a pull request to merge into `develop`

6. After the pull request is approved and merged, delete the feature branch

### Bug Fixing

The process for fixing bugs in the development branch:

1. Create a bugfix branch from the latest `develop` branch:
   ```bash
   git checkout develop
   git pull
   git checkout -b bugfix/456-bug-description
   ```

2. Fix the bug and commit the changes:
   ```bash
   git add .
   git commit -m "fix: resolve calculation error"
   ```

3. Push the branch and create a pull request to `develop`

4. After the pull request is approved and merged, delete the bugfix branch

### Hotfix Process

The process for fixing critical issues in production:

1. Create a hotfix branch from the `main` branch:
   ```bash
   git checkout main
   git pull
   git checkout -b hotfix/789-critical-issue
   ```

2. Fix the issue and commit the changes:
   ```bash
   git add .
   git commit -m "fix: resolve critical security vulnerability"
   ```

3. Push the branch and create pull requests to both `main` and `develop`

4. After both pull requests are approved and merged, delete the hotfix branch

5. Tag the new version on the main branch:
   ```bash
   git checkout main
   git pull
   git tag -a v1.0.1 -m "Hotfix: Critical security vulnerability"
   git push origin v1.0.1
   ```

### Release Process

The process for preparing and deploying releases:

1. Create a release branch from the `develop` branch:
   ```bash
   git checkout develop
   git pull
   git checkout -b release/v1.2.0
   ```

2. Make any final adjustments, version updates, and release preparations:
   ```bash
   # Update version numbers in relevant files
   git add .
   git commit -m "chore: bump version to 1.2.0"
   ```

3. Push the branch and create pull requests to both `main` and `develop`

4. After both pull requests are approved and merged, delete the release branch

5. Tag the new version on the main branch:
   ```bash
   git checkout main
   git pull
   git tag -a v1.2.0 -m "Release v1.2.0"
   git push origin v1.2.0
   ```

## Commit Message Guidelines

The project follows the Conventional Commits specification for commit messages to ensure consistency and enable automated versioning and changelog generation.

### Commit Message Format

Each commit message should follow this format:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

Where:
- `<type>` indicates the kind of change (feat, fix, docs, style, refactor, test, chore)
- `[optional scope]` indicates the section of the codebase affected
- `<description>` is a concise description of the change
- `[optional body]` provides additional context or details
- `[optional footer(s)]` contains references to issues or breaking changes

### Commit Types

- **feat**: A new feature or enhancement
- **fix**: A bug fix
- **docs**: Documentation changes only
- **style**: Changes that don't affect code functionality (formatting, etc.)
- **refactor**: Code changes that neither fix a bug nor add a feature
- **test**: Adding or modifying tests
- **chore**: Changes to build process, tools, etc.
- **perf**: Performance improvements
- **ci**: Changes to CI/CD configuration
- **build**: Changes to build system or dependencies

### Examples

```
feat(calculator): add support for multiple countries

Implement the ability to select multiple countries for VAT calculations.
The country selector now supports multi-select functionality with search.

Closes #123
```

```
fix(api): correct calculation rounding error

Fix decimal rounding issue in VAT calculations that caused small
discrepancies in the final amount.

Fixes #456
```

```
chore: update dependencies to latest versions
```

### Breaking Changes

Breaking changes must be indicated in the commit message with a `BREAKING CHANGE:` footer:

```
feat(api): change response format for calculations

BREAKING CHANGE: The calculation response format has changed from a
flat structure to a nested structure with separate country costs.
Clients will need to update their parsing logic.
```

## Pull Request Process

Pull requests (PRs) are the primary method for integrating changes from supporting branches into the core branches.

### Creating Pull Requests

1. Push your branch to the remote repository
2. Create a pull request through the GitHub interface
3. Fill out the PR template with all required information
4. Link the PR to any relevant issues
5. Assign appropriate reviewers
6. Add relevant labels

### PR Requirements

All pull requests must meet these requirements before merging:

1. Pass all automated checks (build, tests, linting)
2. Meet code coverage requirements (minimum 80%)
3. Receive approval from at least one reviewer
4. Address all reviewer comments
5. Have no merge conflicts with the target branch
6. Include appropriate tests for new functionality
7. Follow the project's coding standards

### PR Review Process

1. Reviewers examine the code for correctness, quality, and adherence to standards
2. Reviewers provide feedback through comments
3. The author addresses feedback with additional commits or explanations
4. Reviewers approve the PR when satisfied with the changes
5. The author merges the PR once all requirements are met

### Merge Strategies

The preferred merge strategies for different scenarios:

- **Feature branches to develop**: Squash and merge (consolidates all commits into one)
- **Bugfix branches to develop**: Squash and merge
- **Hotfix branches to main/develop**: Create a merge commit (preserves commit history)
- **Release branches to main/develop**: Create a merge commit

## Versioning Strategy

The VAT Filing Pricing Tool follows Semantic Versioning (SemVer) for version numbering.

### Version Format

Versions follow the format: `MAJOR.MINOR.PATCH`

Where:
- **MAJOR**: Incremented for incompatible API changes
- **MINOR**: Incremented for backward-compatible new functionality
- **PATCH**: Incremented for backward-compatible bug fixes

### Version Incrementing

- Increment MAJOR version when making breaking changes
- Increment MINOR version when adding features
- Increment PATCH version when fixing bugs
- Reset PATCH to 0 when incrementing MINOR
- Reset MINOR and PATCH to 0 when incrementing MAJOR

### Pre-release Versions

Pre-release versions can be indicated with a hyphen and identifier:

- Alpha: `1.0.0-alpha.1`
- Beta: `1.0.0-beta.1`
- Release Candidate: `1.0.0-rc.1`

### Version Tagging

All releases are tagged in Git using the version number prefixed with 'v':

```bash
git tag -a v1.2.3 -m "Release v1.2.3"
git push origin v1.2.3
```

## CI/CD Integration

The branching strategy is designed to work seamlessly with the project's CI/CD pipeline.

### Automated Builds

- Pull requests trigger build and test workflows
- Pushes to `develop` trigger build, test, and deployment to the development environment
- Pushes to `main` trigger build, test, and deployment to the staging environment
- Tags trigger build, test, and deployment to the production environment

### Environment Mapping

- **develop branch**: Deploys to Development environment
- **main branch**: Deploys to Staging environment
- **release tags**: Deploy to Production environment

### Build Artifacts

- Docker images are tagged with the branch name and commit SHA
- Release builds are additionally tagged with the version number
- Artifacts are stored in Azure Container Registry

### Deployment Approvals

- Deployments to Development are automatic
- Deployments to Staging require approval from a team lead
- Deployments to Production require approval from a product owner

## Branch Protection Rules

Branch protection rules are configured in GitHub to enforce the branching strategy.

### Main Branch Protection

The `main` branch is protected with these rules:

- Require pull request before merging
- Require at least 2 approvals
- Require status checks to pass before merging
- Require branches to be up to date before merging
- Do not allow bypassing the above settings
- Restrict who can push to matching branches

### Develop Branch Protection

The `develop` branch is protected with these rules:

- Require pull request before merging
- Require at least 1 approval
- Require status checks to pass before merging
- Require branches to be up to date before merging
- Do not allow bypassing the above settings

### Status Checks

Required status checks include:

- Build and test workflow
- Code coverage threshold
- SonarCloud quality gate
- Security scanning

## Best Practices

These best practices help maintain an efficient and effective version control workflow.

### General Guidelines

- Keep branches short-lived (merge within days, not weeks)
- Commit frequently with small, focused changes
- Write clear, descriptive commit messages
- Pull changes from the parent branch regularly
- Resolve conflicts promptly
- Delete branches after they are merged

### Code Review Best Practices

- Review code in manageable chunks (< 400 lines per review)
- Provide constructive, specific feedback
- Focus on code quality, correctness, and adherence to standards
- Respond to review comments promptly
- Use GitHub's suggestion feature for simple changes

### Conflict Resolution

When resolving merge conflicts:

1. Pull the latest changes from the parent branch
2. Resolve conflicts locally
3. Test thoroughly after resolving conflicts
4. Commit the conflict resolution separately
5. Push the resolved branch

### Rewriting History

- Avoid rewriting history on shared branches
- Use `git rebase` only on local or personal branches
- Squash commits before merging feature branches
- Use interactive rebase to clean up commit history before creating a PR:
  ```bash
  git rebase -i origin/develop
  # Change 'pick' to 'reword' for commits to modify
  ```

## Troubleshooting

Common Git issues and their solutions.

### Reverting Changes

To revert a commit that has been pushed:

```bash
git revert <commit-hash>
git push
```

### Fixing Commit Messages

To fix the most recent commit message:

```bash
git commit --amend
```

To fix older commit messages (before pushing):

```bash
git rebase -i HEAD~3  # Replace 3 with the number of commits to go back
# Change 'pick' to 'reword' for commits to modify
```

### Recovering Lost Changes

To recover uncommitted changes:

```bash
git stash list          # List stashed changes
git stash apply stash@{0}  # Apply the most recent stash
```

To recover a deleted branch:

```bash
git reflog              # Find the commit hash of the branch tip
git checkout -b branch-name <commit-hash>  # Recreate the branch
```

### Handling Large Files

- Avoid committing large files to the repository
- Use Git LFS for large binary files if necessary
- Add large generated files to .gitignore

## Tools and Extensions

Recommended tools and extensions to enhance Git workflow.

### Git Clients

- **Visual Studio Git Tools**: Integrated Git support in Visual Studio
- **Visual Studio Code Git Integration**: Built-in Git support in VS Code
- **GitHub Desktop**: User-friendly Git client
- **GitKraken**: Powerful Git GUI with visual commit graph
- **SourceTree**: Another popular Git GUI client

### Visual Studio Extensions

- **Git Extensions**: Enhanced Git functionality
- **Pull Requests for Visual Studio**: Review PRs within VS
- **GitFlow for Visual Studio**: GitFlow workflow support

### VS Code Extensions

- **GitLens**: Enhanced Git capabilities
- **GitHub Pull Requests**: PR management in VS Code
- **Git History**: Visual Git history
- **Git Graph**: Visualize Git graph

### Command Line Tools

- **Git Bash**: Unix-like shell for Git commands
- **Oh My Zsh with Git plugins**: Enhanced Git command line
- **Hub**: GitHub command line tool
- **Git Flow AVH Edition**: Command line tools for GitFlow

## References

Additional resources for Git and branching strategies.

### Internal Documentation

- Project Coding Standards: Guidelines for code formatting, style, and quality
- [CI/CD Pipeline](../deployment/ci-cd-pipeline.md): CI/CD pipeline documentation

### External Resources

- [Git Documentation](https://git-scm.com/doc): Official Git documentation
- [GitHub Flow](https://guides.github.com/introduction/flow/): GitHub's branching model
- [GitFlow](https://nvie.com/posts/a-successful-git-branching-model/): Original GitFlow branching model
- [Semantic Versioning](https://semver.org/): SemVer specification
- [Conventional Commits](https://www.conventionalcommits.org/): Commit message convention