FROM mcr.microsoft.com/dotnet/sdk:8.0.406-bookworm-slim AS build

# Install strace
RUN apt update && apt install -y strace procps

WORKDIR /src

# Restore packages
COPY **/*.csproj *.targets *.sln ./
RUN for f in *.csproj; do mkdir "${f%.csproj}" && mv "$f" "${f%.csproj}"; done
RUN dotnet restore *.sln

# Copy source files
COPY . .

# Build the tracer
ENV ASSERT_SYSCALL_TRACER_DIR=/src/bin
RUN dotnet publish AssertSyscall.Tracer/AssertSyscall.Tracer.csproj --no-restore -c Release -o $ASSERT_SYSCALL_TRACER_DIR -p:GenerateDocumentationFile=false

# Run tests
RUN dotnet test AssertSyscall.Tests/AssertSyscall.Tests.csproj --no-restore -c Release
RUN dotnet test AssertSyscall.NUnit.Tests/AssertSyscall.NUnit.Tests.csproj --no-restore -c Release

# Build the packages
RUN dotnet pack AssertSyscall/AssertSyscall.csproj --no-restore -c Release -o $ASSERT_SYSCALL_TRACER_DIR -p:IncludeTracer=true -p:TracerDir=$ASSERT_SYSCALL_TRACER_DIR
RUN dotnet pack AssertSyscall.NUnit/AssertSyscall.NUnit.csproj --no-restore -c Release -o $ASSERT_SYSCALL_TRACER_DIR
