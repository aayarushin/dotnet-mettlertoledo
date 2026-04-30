# Azure DevOps CI/CD Setup Guide

## Overview

This document describes how to set up the build, test, and NuGet deployment pipeline for the MettlerToledoScales project in Azure DevOps Server.

**Target Azure DevOps repo**: `http://chdev1034:8080/tfs/CSAM/MettlerToledoScales/_git/MettlerToledoScales`  
**NuGet feed**: `https://nexus.omda.com/repository/CSAM-nuGet/`

---

## Step 1: Migrate the Git Repository

Run these commands from your local clone:

```bash
# Add Azure DevOps as a new remote
git remote add azdo http://chdev1034:8080/tfs/CSAM/MettlerToledoScales/_git/MettlerToledoScales

# Push all branches and tags
git push azdo --all
git push azdo --tags
```

If the Azure DevOps repo already has content and you want to mirror:

```bash
git push azdo --mirror
```

---

## Step 2: Set Up Nexus Credentials in Azure DevOps

The pipeline uses a variable `$(NexusApiKey)` to authenticate with Nexus. You need to configure this:

### Option A: Pipeline Variable (Simplest)

1. Go to **Pipelines** > your pipeline > **Edit** > **Variables**
2. Add a variable:
   - **Name**: `NexusApiKey`
   - **Value**: Your Nexus API key or password
   - **Keep this value secret**: ✅ Check this box

### Option B: Variable Group (Reusable across pipelines)

1. Go to **Pipelines** > **Library** > **+ Variable group**
2. Name it `NexusCredentials`
3. Add variable `NexusApiKey` (mark as secret)
4. In `azure-pipelines.yml`, add under `variables`:
   ```yaml
   variables:
     - group: NexusCredentials
   ```

> **Note**: If your Nexus uses username/password instead of an API key, replace the `dotnet nuget push` command's `--api-key` with:
> ```
> -s "https://nexus.omda.com/repository/CSAM-nuGet/"
> -k "$(NexusApiKey)"
> ```
> Or use `dotnet nuget push` with `--source` and supply credentials via `nuget.config` or `dotnet nuget add source`.

---

## Step 3: Create the Environment with Approval Gate

The pipeline's deploy stage uses an **environment** called `NexusProduction` with a manual approval gate:

1. Go to **Pipelines** > **Environments**
2. Create a new environment named **`NexusProduction`**
3. Click on the environment > **⋮ (More)** > **Approvals and checks**
4. Add an **Approvals** check
5. Add yourself (or the appropriate approvers) as required approvers

This ensures the NuGet publish only happens after someone manually approves it.

---

## Step 4: Create the Pipeline

1. Go to **Pipelines** > **New Pipeline**
2. Select **Azure Repos Git**
3. Select the **MettlerToledoScales** repository
4. Choose **Existing Azure Pipelines YAML file**
5. Select `/azure-pipelines.yml`
6. Review and **Run**

---

## Step 5: Verify

After a successful build:
- ✅ Build artifacts contain `.nupkg` and `.snupkg` files
- ✅ Test results appear on the build summary
- ✅ Code coverage report is published
- ✅ Approving the deploy stage pushes the package to Nexus
- ✅ Package is available at `https://nexus.omda.com/repository/CSAM-nuGet/`

---

## Pipeline Summary

| Stage | Trigger | What it does |
|-------|---------|-------------|
| **Build & Test** | Every push on any branch | Restore → Build → Test (xUnit + coverage) → Pack `.nupkg` |
| **Publish to Nexus** | Manual approval | Downloads artifact, pushes `.nupkg` to Nexus |

---

## Versioning

Package version is controlled by the `<Version>` property in `RICADO.MettlerToledo.csproj`. Update it before release:

```xml
<Version>1.0.0</Version>
```

---

## Files Created

| File | Purpose |
|------|---------|
| `azure-pipelines.yml` | Multi-stage CI/CD pipeline (build, test, deploy) |
| `nuget.config` | NuGet source configuration (nuget.org + Nexus) |
| `AZURE_DEVOPS_SETUP.md` | This setup guide |
