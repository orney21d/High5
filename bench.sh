#!/usr/bin/env bash
[[ -e test.sh ]] || { echo >&2 "Please cd into the script location before running it."; exit 1; }
set -e
dotnet run -c Release --project tests/Benchmarks/ParseFive.Benchmarks.csproj
