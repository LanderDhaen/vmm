# GitHub Actions Overview

## Purpose

This document describes the automated workflows that validate code quality, security, and compliance for the .NET project.

## Workflows

### 1. Dependabot PR Management

**Trigger**: Pull request opened/synchronized by Dependabot

**Purpose**: Handle automated dependency updates

**Checks**:

- Validates Dependabot-initiated pull requests
- Extracts update type metadata
- Requires manual review before merging

**Tools**: Dependabot metadata fetch action

---

### 2. Setup Branch Protection

**Trigger**: Manual workflow dispatch

**Purpose**: Configure branch protection rules

**Checks**:

- Enables/disables branch protection for specified branches
- Configures required status checks
- Sets up repository rulesets for newer GitHub APIs
- Validates protection rule existence before operations

**Tools**: GitHub Script action, GitHub REST API

---

### 3. Tests and Approval Required

**Trigger**: Push/PR to main/develop branches

**Purpose**: Execute comprehensive testing and coverage analysis

**Checks**:

- **Build Validation**: .NET SDK 8.0 build with dependency restoration
- **Test Execution**: Automated test runs with coverage collection
- **Coverage Reporting**: Code coverage analysis with Cobertura format
- **Artifact Management**: Test results and coverage reports stored for 30 days
- **PR Integration**: Automatic coverage comments and failure notifications

**Tools**: .NET SDK, ReportGenerator, GitHub Actions artifacts

---

### 4. Code Quality and Security

**Trigger**: Push/PR to main/develop branches

**Purpose**: Perform static analysis and security scanning

**Checks**:

- **Code Quality Job**: .NET 9.0 release build validation with dependency restoration
- **CodeQL Security Job**: Language-specific vulnerability scanning for C# code
- **Automated Compilation**: Context-aware build process for security analysis

**Tools**: CodeQL Analysis, .NET SDK 9.0, GitHub Security features

## Validation Process

1. **Dependency Management**: Automated updates require manual approval
2. **Branch Protection**: Enforced quality gates before merges
3. **Testing Coverage**: Comprehensive test execution with reporting
4. **Security Scanning**: Static analysis for vulnerabilities and code quality issues

