FROM mcr.microsoft.com/dotnet/sdk:9.0

# Install system dependencies
RUN apt-get update && apt-get install -y \
    curl gnupg ca-certificates lsb-release apt-transport-https wget gnupg2

# Install Node.js (needed for Azurite and Core Tools)
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs

# Install Azurite globally
RUN npm install -g azurite

# ✅ Install latest Azure Functions Core Tools v4 from npm (NOT apt)
# This ensures compatibility with .NET 8/9
RUN npm install -g azure-functions-core-tools@4 --unsafe-perm true

# Set working directory
WORKDIR /workspace

# Expose common Azure Functions & Azurite ports
EXPOSE 7071 10000 10001 10002

# Fix execute permissions for in-proc host binary, if needed (defensive)
RUN chmod +x /usr/lib/azure-functions-core-tools-4/in-proc6/func || true

# Set default shell
CMD ["bash"]
