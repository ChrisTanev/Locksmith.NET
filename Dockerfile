FROM mcr.microsoft.com/dotnet/sdk:9.0

# Install basic tools
RUN apt-get update && apt-get install -y \
    curl gnupg ca-certificates lsb-release apt-transport-https wget

# Configure Microsoft package feed (correct one for Core Tools)
RUN wget -qO packages-microsoft-prod.deb \
     https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb \
 && dpkg -i packages-microsoft-prod.deb

# Update and install Core Tools v4
RUN apt-get update \
 && apt-get install -y azure-functions-core-tools-4

WORKDIR /workspace
EXPOSE 7071
CMD ["bash"]
