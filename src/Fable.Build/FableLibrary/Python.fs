namespace Build.FableLibrary

open System.IO
open Fake.IO
open Build.Utils
open SimpleExec
open BlackFox.CommandLine

type BuildFableLibraryPython() =
    inherit
        BuildFableLibrary(
            "python",
            Path.Combine("src", "fable-library-py"),
            Path.Combine("src", "fable-library-py", "fable_library"),
            Path.Combine("temp", "fable-library-py"),
            Path.Combine("temp", "fable-library-py", "fable_library")
        )

    override this.FableBuildStage() =
        let args =
            CmdLine.appendRaw this.SourceDir
            >> CmdLine.appendPrefix "--outDir" this.OutDir
            >> CmdLine.appendPrefix "--fableLib" "."
            >> CmdLine.appendPrefix "--lang" this.Language
            >> CmdLine.appendPrefix "--exclude" "Fable.Core"
            >> CmdLine.appendPrefix "--define" "FABLE_LIBRARY"
            >> CmdLine.appendRaw "--noCache"
            // Target implementation can require additional arguments
            >> this.FableArgsBuilder

        Command.Fable(args)


    override this.CopyStage() =
        // // Copy all *.rs files to the build directory
        Directory.GetFiles(this.LibraryDir, "*") |> Shell.copyFiles this.BuildDir

        Directory.GetFiles(this.SourceDir, "*.py") |> Shell.copyFiles this.OutDir
        // Python extension modules
        Directory.GetFiles(Path.Combine(this.SourceDir, "core"), "*")
        |> Shell.copyFiles (Path.Combine(this.OutDir, "core"))
        // Rust sources for building the extension modules
        Directory.GetFiles(Path.Combine(this.LibraryDir, "src"), "*")
        |> Shell.copyFiles (Path.Combine(this.BuildDir, "src"))


    override this.PostFableBuildStage() =
        // Fix issues with Fable .fsproj not supporting links
        let linkedFileFolder =
            Path.Combine(this.BuildDir, "fable_library", "fable-library-ts")

        Command.Run("poetry", "config virtualenvs.in-project true", this.BuildDir)
        Command.Run("poetry", "install", this.BuildDir) // Maturn needs a local virtual environment
        Command.Run("maturin", "develop --release", this.BuildDir)

        Directory.GetFiles(linkedFileFolder, "*") |> Shell.copyFiles this.OutDir

        Shell.deleteDir (this.BuildDir </> "fable_library/fable-library-ts")

        // Install the python dependencies at the root of the project
        Command.Run("poetry", "install")

        // Run Ruff linter checking import sorting and fix any issues
        Command.Run("poetry", $"run ruff check --select I --fix {this.BuildDir}")
        // Run Ruff formatter on all generated files
        Command.Run("poetry", $"run ruff format {this.BuildDir}")
