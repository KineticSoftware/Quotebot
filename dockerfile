ARG REPO=mcr.microsoft.com/dotnet/runtime
ARG TAG=6.0
FROM ${REPO}:${TAG}

LABEL maintainer="devops@conerd.org"
LABEL dotnet="${TAG}"

ARG user=runtime
ARG uid=1000
ARG gid=1000
ARG home=/${user}
ARG source="src/Quotebot/bin/Release/net6.0/publish/"

RUN addgroup --gid ${gid} ${user} && \
    adduser --home ${home} --gid ${gid} --uid ${uid} ${user} --disabled-password --gecos "" && \
    chown -R ${user}:${user} ${home}

WORKDIR ${home}

COPY ${source} ${home}

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Quotebot.dll"]
