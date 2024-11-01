# ベースイメージ
FROM golang:1.23.1 AS builder

# ワーキングディレクトリを設定
WORKDIR /build

# 依存関係をダウンロード
COPY go.mod ./
COPY go.sum ./
RUN go mod download

# ソースコードをコピー
COPY . .

# アプリケーションをビルド
RUN CGO_ENABLED=0 GOOS=linux go build -o main

# 最終イメージ
FROM gcr.io/distroless/static:nonroot

# 実行ファイルをコピー
COPY --from=builder /build/main /main

# 環境変数の設定
ENV PORT=8080

# ポートの公開
EXPOSE 8080

# 実行コマンドの設定
CMD ["/main"]
