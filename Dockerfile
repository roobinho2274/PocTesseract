FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore ./OcrServer/OcrServer.csproj
RUN dotnet publish ./OcrServer/OcrServer.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-por \
    tesseract-ocr-eng \
    tesseract-ocr-chi-sim \
    tesseract-ocr-chi-sim-vert \
    libtesseract-dev \
    libleptonica-dev \
    libgdiplus \
    libglib2.0-0 \
    libicu-dev \
    libjpeg62-turbo \
    libpng16-16 \
    wget \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

RUN mkdir -p /app/x64 && \
    set -eux; \
    LEPTON="$(ldconfig -p | awk '/liblept\.so/{print $NF; exit}')" && \
    TESS="$(ldconfig -p | awk '/libtesseract\.so/{print $NF; exit}')" && \
    LIBDL="$(ldconfig -p | awk '/libdl\.so\.2/{print $NF; exit}')" && \
    echo "LEPTON=$LEPTON" && echo "TESS=$TESS" && echo "LIBDL=$LIBDL" && \
    ln -sf "$LEPTON" /app/x64/libleptonica-1.82.0.so && \
    ln -sf "$TESS"   /app/x64/libtesseract50.so && \
    ln -sf "$LIBDL"  /app/x64/libdl.so && \
    ln -sf "$LEPTON" /usr/lib/x86_64-linux-gnu/libleptonica-1.82.0.so || true && \
    ln -sf "$TESS"   /usr/lib/x86_64-linux-gnu/libtesseract50.so     || true && \
    ln -sf "$LIBDL"  /usr/lib/x86_64-linux-gnu/libdl.so              || true && \
    ldconfig

RUN wget https://github.com/bblanchon/pdfium-binaries/releases/latest/download/pdfium-linux-x64.tgz \
    && tar -xvf pdfium-linux-x64.tgz \
    && cp lib/libpdfium.so /usr/lib/libpdfium.so \
    && ln -sf /usr/lib/libpdfium.so /usr/lib/libpdfium.dll.so \
    && ln -sf /usr/lib/libpdfium.so /app/pdfium.dll.so

RUN mkdir -p /app/PDFsParaProcessar && \
    rm -rf /app/tessdata && \
    ln -s /usr/share/tesseract-ocr/5/tessdata /app/tessdata || ln -s /usr/share/tesseract-ocr/4.00/tessdata /app/tessdata

ENV LD_LIBRARY_PATH="/app/x64:/app:/usr/lib/x86_64-linux-gnu:/usr/local/lib"

ENV ASPNETCORE_URLS="http://0.0.0.0:8080"
EXPOSE 8080
ENTRYPOINT ["dotnet", "OcrServer.dll"]