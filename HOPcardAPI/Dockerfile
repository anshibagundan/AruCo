# syntax = docker/dockerfile:1

# ビルドステージ
FROM --platform=$BUILDPLATFORM golang:1.23.1 AS builder

WORKDIR /app

COPY go.mod .
COPY go.sum .
RUN go mod download

COPY . .

ARG TARGETOS
ARG TARGETARCH
RUN GOOS=$TARGETOS GOARCH=$TARGETARCH go build -o main .

# 実行ステージ
FROM --platform=$TARGETPLATFORM alpine:latest

WORKDIR /app

COPY --from=builder /app/main .

RUN chmod +x /app/main

RUN adduser -D appuser
USER appuser

CMD ["/app/main"]