# ベースイメージ
FROM golang:1.23.1 AS builder

# ワーキングディレクトリを設定
WORKDIR /app

# 依存関係をダウンロード
COPY go.mod ./
COPY go.sum ./
RUN go mod download

# ソースコードをコピー
COPY . .

# アプリケーションをビルド
RUN CGO_ENABLED=0 GOOS=linux go build -o main .

# 最終イメージ
FROM gcr.io/distroless/base

WORKDIR /app
COPY --from=builder /app/main /app/main

# 環境変数の設定
ENV PORT=8080
EXPOSE 8080

# アプリケーションを起動
CMD ["/app/main"]
