# Project Instructions

## Design Doc

The design doc for this project lives in DESIGN-DOC.md. Keep it up to date when adding, removing, or modifying features. 

## General Guidelines

Re-use generic functionality instead of re-implementing it where possible. In particular, re-use data structures/algorithms provided by scripts in the Util folder. When adding new functionality that could be reusable, add it to Util.

## Unity Guidelines

Don't modify scene files or prefabs directly. Instead, give the user instructions for how to make those modifications manually in the editor.

## Coding Style

Follow the [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) for all C# code in this project, including the [Identifier Naming Guidelines](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names), unless a rule in this file explicitly states otherwise.

### Formatting and Naming

- Always use curly braces for `if`, `else`, `for`, etc., even if the body is only a single line.