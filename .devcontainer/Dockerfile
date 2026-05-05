FROM mcr.microsoft.com/devcontainers/base:bookworm

# Work around broken Yarn apt source key in some Codespaces images.
RUN rm -f /etc/apt/sources.list.d/yarn.list /etc/apt/sources.list.d/yarn*.list
