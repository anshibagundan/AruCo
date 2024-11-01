# ベースイメージ
FROM golang:1.23.1 AS builder

# ワーキングディレクトリを設定
WORKDIR /project

# 依存関係をダウンロード
COPY go.mod ./
COPY go.sum ./
RUN go mod download

# ソースコードをコピー
COPY . .

# アプリケーションをビルド
RUN go build -o app .

# 最終イメージ
FROM gcr.io/distroless/base

COPY --from=builder /project/app /app

# アプリケーションを起動
CMD ["/app"]
